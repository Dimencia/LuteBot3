using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleML
{
    public class MLParameter<T>
    {
        public float Weight { get; internal set; }
        public float Target { get; internal set; }
        
        public Func<T,float> Retriever { get; }

        public float MinValue { get; internal set; }
        public float MaxValue { get; internal set; }

        public bool AbsoluteValue { get; private set; }

        public MLParameter(Func<T,float> retriever)
        {
            Retriever = retriever;
        }
        
        // Ensures the value is clamped between min/max, and the min/max is never changed based on the data
        public void SetAbsolute(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
            AbsoluteValue = true;
        }
    }
}
