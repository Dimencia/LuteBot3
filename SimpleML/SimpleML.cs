using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleML
{
    public class SimpleML<T>
    {
        // A very simple ML implementation that ranks and returns a set of options of type T, based on arbitrary parameters and criteria

        // Note that by default, any parameters are compared against the min and max for the given set to choose from, unless explicitly defined with absolute min/max

        public MLParameter<T>[] Parameters { get; }
        public bool Trained { get; private set; } // For outside use, to avoid querying it before ready

        public SimpleML(params Func<T, float>[] retrievalFunctions)
        {
            Parameters = new MLParameter<T>[retrievalFunctions.Length];
            for (int i = 0; i < retrievalFunctions.Length; i++)
            {
                Parameters[i] = new MLParameter<T>(retrievalFunctions[i]);
            }
        }

        // TODO: implement a singular getWeight, which requires you to specify min and max, or requires the param to be Absolute
        // But taking in an array lets us calculate it
        public IEnumerable<MLResult<T>> GetWeights(IEnumerable<T> input)
        {
            // We want each weight to be reflective of the min/max for each arg retrieval - i.e retrieval results should be 0-1, before applying target and weight
            Dictionary<MLParameter<T>, float[]> storedValues = new Dictionary<MLParameter<T>, float[]>();
            int inputLength = input.Count();
            foreach (var p in Parameters)
            {
                if (!p.AbsoluteValue)
                {
                    p.MinValue = float.MaxValue;
                    p.MaxValue = float.MinValue;
                }
                var values = new float[inputLength];
                for (int i = 0; i < inputLength; i++)
                {
                    values[i] = p.Retriever(input.ElementAt(i));
                    if (!p.AbsoluteValue)
                    {
                        if (values[i] < p.MinValue)
                            p.MinValue = values[i];
                        if (values[i] > p.MaxValue)
                            p.MaxValue = values[i];
                    }
                    else
                    {
                        // Clamp it to its defined min/max if it is supposed to be absolute
                        values[i] = Math.Min(Math.Max(values[i], p.MinValue), p.MaxValue);
                    }
                }
                storedValues[p] = values;
            }
            // Now we have all the values, mins, and maxes.  Output a final weight that is between 0-1
            var results = new List<MLResult<T>>();

            for (int i = 0; i < inputLength; i++)
            {
                var inputResult = new MLResult<T>(input.ElementAt(i));
                foreach (var p in Parameters)
                {
                    var val = storedValues[p][i];
                    if (p.MaxValue != p.MinValue) // Avoid divide by 0
                    {
                        inputResult.ParameterBaseValues[p] = (val - p.MinValue) / (p.MaxValue - p.MinValue); // 0-1, the base value of the param before comparing to target/weight
                        inputResult.ParameterWeights[p] = Math.Abs(inputResult.ParameterBaseValues[p] - p.Target) * p.Weight; // final weight for this param
                        inputResult.Weight += inputResult.ParameterWeights[p]; // final weight for all params
                    }
                    else // Div by 0, we can't give weight to this
                    {
                        inputResult.ParameterWeights[p] = 0;
                        inputResult.ParameterBaseValues[p] = 0;
                    }
                }
                // Then divide weight by number of params, to make the final result in 0-1 range
                inputResult.Weight /= Parameters.Length;
                results.Add(inputResult);
            }
            return results;
        }


        // First construct a TrainingTarget out of a set of TrainingCandidates, each of which contains the 'correct' choice, given an array of T for it to choose from
        public void Train(TrainingTarget<T> target)
        {
            foreach (var candidate in target.Candidates)
            {
                // Build a set of desired Target values that would give each given candidate a 0-weight (perfect score)
                var initialResults = GetWeights(candidate.Values);

                foreach (var result in initialResults)
                {
                    // Find the correct target... this could probably be optimized
                    // We make the user specify an EqualityComparator func for the TrainingTarget, mostly because I need regular comparisons outside of this
                    // But inside, it is convenient to assume a different comparator.  Rarely.  This can probably go away
                    if (!target.EqualityComparator(result.BaseObject, candidate.TopAnswer))
                        continue;

                    foreach (var p in Parameters)
                        candidate.IdealTargets[p] = result.ParameterBaseValues[p];

                    break;
                }
            }

            // Get the median of ideal targets; the results are a bit better than .Average, in my tests
            foreach (var p in Parameters)
            {
                p.Target = target.Candidates.Select(c => c.IdealTargets[p]).Median();
            }

            // Then build a set of weights that optimize toward these targets
            foreach (var candidate in target.Candidates)
            {
                var targetedResults = GetWeights(candidate.Values.ToArray());
                foreach (var result in targetedResults)
                {
                    if (!target.EqualityComparator(result.BaseObject, candidate.TopAnswer))
                        continue;
                    foreach (var p in Parameters)
                    {
                        // It turns out to be better to increase the difference between this and the next best in the category, rather than minimize the output value
                        var min = targetedResults.Where(r => !target.EqualityComparator(r.BaseObject, result.BaseObject)).Min(r => r.ParameterBaseValues[p]);
                        candidate.IdealWeights[p] = Math.Abs(result.ParameterBaseValues[p] - min); // Still 0-1
                    }
                    break;
                }
            }

            // Average all the ideal weights
            foreach (var p in Parameters)
            {
                p.Weight = target.Candidates.Average(c => c.IdealWeights[p]);
            }

            // Just do one more and show how many were correct, purely optional
            int numIncorrect = 0;
            foreach (var candidate in target.Candidates)
            {
                var targetedResults = GetWeights(candidate.Values.ToArray()).OrderBy(w => w.Weight);
                if (!target.EqualityComparator(targetedResults.First().BaseObject, candidate.TopAnswer))
                {
                    numIncorrect++;
                }
            }
            
            Console.WriteLine($"--- #Incorrect: {numIncorrect}");
            Trained = true;
        }

    }
}
