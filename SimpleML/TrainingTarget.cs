using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleML
{
    /// <summary>
    /// Contains a set of TrainingCandidates; given as a larger TrainingTarget which attempts to optimize to be correct for all Candidates
    /// </summary>
    public class TrainingTarget<T>
    {
        public IEnumerable<TrainingCandidate<T>> Candidates { get; }
        public Func<T,T,bool> EqualityComparator { get; }
        public TrainingTarget(IEnumerable<TrainingCandidate<T>> candidates, Func<T,T,bool> eqComparer)
        {
            Candidates = candidates;
            EqualityComparator = eqComparer;
        }
    }
}
