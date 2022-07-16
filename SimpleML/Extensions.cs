using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleML
{
    public static class Extensions
    {
        public static float Median(this IEnumerable<float> target)
        {
            float idealTarget;
            var candidateCount = target.Count();
            var sortedCandidates = target.OrderBy(c => c);
            int halfIndex = candidateCount / 2;
            if (candidateCount % 2 == 0)
            {
                idealTarget = (sortedCandidates.ElementAt(halfIndex) + sortedCandidates.ElementAt(halfIndex - 1)) / 2;
            }
            else
            {
                idealTarget = sortedCandidates.ElementAt(halfIndex);
            }
            return idealTarget;
        }
    }
}
