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
namespace BoboBrowse.Net.Query
{
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    public interface IScorerBuilder
    {
        // NOTE: The Weight.Scorer method lost the scoreDocsInOrder and topScorer parameters between
        // Lucene 4.3.0 and 4.8.0. They are not used by BoboBrowse anyway, so the code here diverges 
        // from the original Java source to remove these two parameters.

        // Scorer CreateScorer(Scorer innerScorer, AtomicReader reader, bool scoreDocsInOrder, bool topScorer);
        Scorer CreateScorer(Scorer innerScorer, AtomicReader reader);
        Explanation Explain(AtomicReader reader, int doc, Explanation innerExplanation);
    }
}
