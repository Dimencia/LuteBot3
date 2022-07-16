using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleML
{
    public class MLResult<T>
    {
        public T BaseObject { get; }
        public float Weight { get; internal set; }
        public Dictionary<MLParameter<T>, float> ParameterWeights { get; internal set; } = new Dictionary<MLParameter<T>, float>();
        public Dictionary<MLParameter<T>, float> ParameterBaseValues { get; } = new Dictionary<MLParameter<T>, float>();

        public MLResult(T baseObject)
        {
            BaseObject = baseObject;
        }
    }
}
