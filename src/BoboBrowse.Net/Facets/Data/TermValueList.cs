﻿//* Bobo Browse Engine - High performance faceted/parametric search implementation 
//* that handles various types of semi-structured data.  Written in Java.
//* 
//* Copyright (C) 2005-2006  John Wang
//*
//* This library is free software; you can redistribute it and/or
//* modify it under the terms of the GNU Lesser General Public
//* License as published by the Free Software Foundation; either
//* version 2.1 of the License, or (at your option) any later version.
//*
//* This library is distributed in the hope that it will be useful,
//* but WITHOUT ANY WARRANTY; without even the implied warranty of
//* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//* Lesser General Public License for more details.
//*
//* You should have received a copy of the GNU Lesser General Public
//* License along with this library; if not, write to the Free Software
//* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//* 
//* To contact the project administrators for the bobo-browse project, 
//* please go to https://sourceforge.net/projects/bobo-browse/, or 
//* send mail to owner@browseengine.com. 

// Version compatibility level: 3.1.0
namespace BoboBrowse.Net.Facets.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ITermValueList : IList<string>
    {
        new int Count { get; }
        string Get(int index);
        IComparable GetComparableValue(int index);
        object GetRawValue(int index);
        string Format(object o);
        int IndexOf(object o);
        void Add(string o); // TODO: Add boolean return value here.
        List<string> GetInnerList();
        void Seal();
        Type Type { get; }
    }

    /// <summary>This class behaves as List<string> with a few extensions:
    /// <ul>
    /// <li> Semi-immutable, e.g. once added, cannot be removed. </li>
    /// <li> Assumes sequence of values added are in sorted order </li>
    /// <li> <seealso cref="#indexOf(Object)"/> return value conforms to the contract of <seealso cref="Arrays#binarySearch(Object[], Object)"/></li>
    /// <li> <seealso cref="#seal()"/> is introduce to trim the List size, similar to <seealso cref="ArrayList#TrimToSize()"/>, once it is called, no add should be performed.</li>
    /// </u> </summary>
    public abstract class TermValueList<T> : ITermValueList
    {
        public abstract string Format(object o);
        public virtual void Seal()
        {
            _innerList.TrimExcess();
        }

        protected List<T> _innerList;

        protected TermValueList()
        {
            _innerList = new List<T>();
        }

        protected TermValueList(int capacity)
        {
            _innerList = new List<T>(capacity);
        }

        /// <summary>
        /// The user of this method should not try to alter the content of the list,
        /// which may result in data inconsistency.
        /// And of the content can be accessed using the getRawValue(int) method.
        /// </summary>
        /// <returns>the inner list</returns>
        public virtual List<string> GetInnerList()
        {
            //return new List<string>(_innerList.Select(x => Format(x)));
            return new List<string>(_innerList.Select(x => Convert.ToString(x)));
        }

        /// <summary>
        /// Add a new value to the list. <b>It is important to add the values in sorted (ASC) order.</b>
        /// Our algorithm uses binary searches and priority queues, both of which fails when the ordering is wrong.
        /// </summary>
        /// <param name="o"></param>
        public abstract void Add(string o); // From IList<string>

        //public virtual void Add(int index, string element)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        public virtual void AddAll(IEnumerable<T> c) 
        {
            foreach (var s in c)
            {
                this.Add(Convert.ToString(s));
            }
        }

        //public virtual bool AddAll(int index, IEnumerable<T> c)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        public virtual void Clear() // From IList<string>
        {
            _innerList.Clear();
        }

        public virtual bool Contains(object o)
        {
            return IndexOf(o) >= 0;
        }

        public abstract bool ContainsWithType(T val);

        //public bool ContainsAll(IEnumerable<T> c)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        public virtual Type Type
        {
            get { return typeof(T); }
        }

        public virtual string Get(int index)
        {
            return Format(_innerList[index]);
        }

        public virtual object GetRawValue(int index)
        {
            return _innerList[index];
        }

        //public virtual TRaw GetRawValue<TRaw>(int index)
        //{
        //    return (TRaw)_innerList[index];
        //}

        public virtual IComparable GetComparableValue(int index)
        {
            return (IComparable)_innerList[index];
        }

        public abstract int IndexOf(object o);

        //public virtual int IndexOfWithOffset(Object value, int offset)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        public abstract int IndexOfWithType(T o);

        public virtual bool IsEmpty()
        {
            return _innerList.Count == 0;
        }

        public virtual int LastIndexOf(object o)
        {
            return IndexOf(o);
        }

        //public virtual ListIterator<string> ListIterator()
        //{
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual ListIterator<string> ListIterator(int index)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual bool Remove(Object o)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual string Remove(int index) {
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual bool RemoveAll(IEnumerable<T> c)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual boolean RetainAll(IEnumerable<T> c)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        //public virtual string Set(int index, string element)
        //{
        //    throw new NotSupportedException("not supported");
        //}

        public virtual int Size
        {
            get { return this.Count; }
        }

        //public virtual List<string> SubList(int fromIndex, int toIndex)
        //{
        //    throw new InvalidOperationException("not supported");
        //}

        public virtual int IndexOf(string item)// From IList<string>
        {
            return this.IndexOf((object)item);
        }

        public virtual void Insert(int index, string item)// From IList<string>
        {
            throw new InvalidOperationException("not supported");
        }

        public virtual void RemoveAt(int index)// From IList<string>
        {
            throw new InvalidOperationException("not supported");
        }

        public virtual string this[int index]// From IList<string>
        {
            get
            {
                return Format(_innerList[index]);
            }
            set
            {
                throw new NotSupportedException("not supported");
            }
        }


        public virtual bool Contains(string item) // From IList<string>
        {
            return this.Contains((object)item);
        }

        public virtual void CopyTo(string[] array, int arrayIndex)// From IList<string>
        {
            _innerList.Select(x => Convert.ToString(x)).ToList().CopyTo(array, arrayIndex);
        }

        public virtual int Count// From IList<string>
        {
            get { return _innerList.Count; }
        }

        public virtual bool IsReadOnly// From IList<string>
        {
            get { return false; }
        }

        public virtual bool Remove(string item)// From IList<string>
        {
            throw new NotSupportedException("not supported");
        }

        public virtual IEnumerator<string> GetEnumerator()// From IList<string>
        {
            return new TermValueListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()// From IList<string>
        {
            return new TermValueListEnumerator(this);
        }

        public class TermValueListEnumerator : IEnumerator<string>
        {
            private readonly TermValueList<T> parent;
            private readonly IEnumerator<T> iter;

            public TermValueListEnumerator(TermValueList<T> parent)
            {
                this.parent = parent;
                this.iter = parent._innerList.GetEnumerator();
            }

            public string Current
            {
                get { return parent.Format(iter.Current); }
            }

            public void Dispose()
            {
                iter.Dispose();
            }

            object IEnumerator.Current
            {
                get { return parent.Format(iter.Current); }
            }

            public bool MoveNext()
            {
                return iter.MoveNext();
            }

            public void Reset()
            {
                iter.Reset();
            }
        }
    }
}
