﻿//* Bobo Browse Engine - High performance faceted/parametric search implementation 
//* that handles various types of semi-structured data.  Originally written in Java.
//*
//* Ported and adapted for C# by Shad Storhaug.
//*
//* Copyright (C) 2005-2015  John Wang
//*
//* Licensed under the Apache License, Version 2.0 (the "License");
//* you may not use this file except in compliance with the License.
//* You may obtain a copy of the License at
//*
//*   http://www.apache.org/licenses/LICENSE-2.0
//*
//* Unless required by applicable law or agreed to in writing, software
//* distributed under the License is distributed on an "AS IS" BASIS,
//* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//* See the License for the specific language governing permissions and
//* limitations under the License.

// Version compatibility level: 4.0.2
namespace BoboBrowse.Net.Facets.Impl
{
    using BoboBrowse.Net.Facets.Data;
    using BoboBrowse.Net.Support;
    using BoboBrowse.Net.Support.Logging;
    using BoboBrowse.Net.Util;
    using C5 = Lucene.Net.Support.C5;
    using Lucene.Net.Index;
    using Lucene.Net.Util;
    using System;
    using System.Collections.Generic;
    using Lucene.Net.Support;

    public class VirtualSimpleFacetHandler : SimpleFacetHandler
    {
        private static readonly ILog log = LogProvider.For<VirtualSimpleFacetHandler>();

        protected IFacetDataFetcher m_facetDataFetcher;

        /// <summary>
        /// Plugin constructor for TreeView, which has its own way of logging.
        /// Unfortunately, the author of TreeView didn't have the foresight to 
        /// log different types such as Error, Info, Warn, etc.
        /// </summary>
        static VirtualSimpleFacetHandler()
        {
            C5.Logger.Log = (string message) =>
            {
                log.Info(message);
            };
        }

        public VirtualSimpleFacetHandler(string name,
                                         string indexFieldName,
                                         TermListFactory termListFactory,
                                         IFacetDataFetcher facetDataFetcher,
                                         ICollection<string> dependsOn)
            : base(name, null, termListFactory, dependsOn)
        {
            m_facetDataFetcher = facetDataFetcher;
        }

        public VirtualSimpleFacetHandler(string name,
                                   TermListFactory termListFactory,
                                   IFacetDataFetcher facetDataFetcher,
                                   ICollection<string> dependsOn)
            : this(name, null, termListFactory, facetDataFetcher, dependsOn)
        {
        }

        public override FacetDataCache Load(BoboSegmentReader reader)
        {
            TreeDictionary<object, List<int>> dataMap = null;
            List<int> docList = null;

            int nullMinId = -1;
            int nullMaxId = -1;
            int nullFreq = 0;
            int doc = -1;

            IBits liveDocs = reader.LiveDocs;
            for (int i = 0; i < reader.MaxDoc; ++i)
            {
                if (liveDocs != null && !liveDocs.Get(i))
                {
                    continue;
                }
                doc = i;
                object val = m_facetDataFetcher.Fetch(reader, doc);
                if (val == null)
                {
                    if (nullMinId < 0)
                        nullMinId = doc;
                    nullMaxId = doc;
                    ++nullFreq;
                    continue;
                }
                if (dataMap == null)
                {
                    // Initialize.
                    if (val is long[])
                    {
                        if (m_termListFactory == null)
                            m_termListFactory = new TermFixedLengthInt64ArrayListFactory(
                              ((long[])val).Length);

                        dataMap = new TreeDictionary<object, List<int>>(new VirtualSimpleFacetHandlerInt16ArrayComparer());
                    }
                    else if (val is IComparable)
                    {
                        dataMap = new TreeDictionary<object, List<int>>();
                    }
                    else
                    {
                        dataMap = new TreeDictionary<object, List<int>>(new VirtualSimpleFacetHandlerObjectComparer());
                    }
                }

                if (dataMap.Contains(val))
                    docList = dataMap[val];
                else
                    docList = null;

                if (docList == null)
                {
                    docList = new List<int>();
                    dataMap[val] = docList;
                }
                docList.Add(doc);
            }

            m_facetDataFetcher.Cleanup(reader);

            int maxDoc = reader.MaxDoc;
            int size = dataMap == null ? 1 : (dataMap.Count + 1);

            BigSegmentedArray order = new BigInt32Array(maxDoc);
            ITermValueList list = m_termListFactory == null ?
              new TermStringList(size) :
              m_termListFactory.CreateTermList(size);

            int[] freqs = new int[size];
            int[] minIDs = new int[size];
            int[] maxIDs = new int[size];

            list.Add(null);
            freqs[0] = nullFreq;
            minIDs[0] = nullMinId;
            maxIDs[0] = nullMaxId;

            if (dataMap != null)
            {
                int i = 1;
                int? docId;
                foreach (var entry in dataMap)
                {
                    list.Add(list.Format(entry.Key));
                    docList = entry.Value;
                    freqs[i] = docList.Count;
                    minIDs[i] = docList.Get(0, int.MinValue);
                    while ((docId = docList.Poll(int.MinValue)) != int.MinValue)
                    {
                        doc = (int)docId;
                        order.Add(doc, i);
                    }
                    maxIDs[i] = doc;
                    ++i;
                }
            }
            list.Seal();

            FacetDataCache dataCache = new FacetDataCache(order, list, freqs, minIDs, maxIDs, 
                TermCountSize.Large);
            return dataCache;
        }

        /// <summary>
        /// NOTE: This was VirtualSimpleFacetHandlerLongArrayComparator in bobo-browse
        /// </summary>
        private class VirtualSimpleFacetHandlerInt16ArrayComparer : IComparer<object>
        {
            public virtual int Compare(object big, object small)
            {
                if (((long[])big).Length != ((long[])small).Length)
                {
                    throw new RuntimeException("" + Arrays.ToString((long[])big) + " and " +
                      Arrays.ToString(((long[])small)) + " have different length.");
                }

                long r = 0;
                for (int i = 0; i < ((long[])big).Length; ++i)
                {
                    r = ((long[])big)[i] - ((long[])small)[i];
                    if (r != 0)
                        break;
                }

                if (r > 0)
                    return 1;
                else if (r < 0)
                    return -1;

                return 0;
            }
        }

        private class VirtualSimpleFacetHandlerObjectComparer : IComparer<object>
        {
            public virtual int Compare(object big, object small)
            {
                return string.CompareOrdinal(Convert.ToString(big), Convert.ToString(small));
            }
        }
    }
}
