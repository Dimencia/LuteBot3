using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleML
{
    public class TrainingCandidate<T>
    {
        public T[] Values { get; }
        public T TopAnswer { get; }

        public Dictionary<MLParameter<T>, float> IdealTargets { get; } = new Dictionary<MLParameter<T>, float>();
        public Dictionary<MLParameter<T>, float> IdealWeights { get; } = new Dictionary<MLParameter<T>, float>();

        public TrainingCandidate(IEnumerable<T> values, T topAnswer)
        {
            Values = values.ToArray(); // Enforce copying to avoid them unintentionally wiping their ref
            TopAnswer = topAnswer;
        }
    }
}
