using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SimpleML
{
    public class Utils
    {
        public static void MatrixAdd(float[][][] left, float[][][] right, ref float[][][] result)
        {
            for (int i = 0; i < left.Length; i++)
                result[i] = MatrixAdd(left[i], right[i]);
        }

        public static float[][] MatrixAdd(float[][] left, float[][] right)
        {
            float[][] result = new float[left.Length][];
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = new float[left[i].Length];
                MatrixAdd(left[i], right[i], ref result[i]);
            }
            return result;
        }

        public static float[] MatrixAdd(float left, float[] right)
        {
            var result = new float[right.Length];
            MatrixAdd(left, right, ref result);
            return result;
        }

        public static void MatrixAdd(float left, float[] right, ref float[] result)
        {
            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            int length = right.Length;

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left);
                var v2 = new Vector<float>(right, i);
                (v1 + v2).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = left + right[i];
            }
        }

        public static float[][] MatrixSub(float[][] left, float[][] right)
        {
            var result = new float[left.Length][];
            for (int i = 0; i < left.Length; i++)
                result[i] = MatrixSub(left[i], right[i]);
            return result;
        }

        public static float[] MatrixSub(float[] left, float[] right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (left.Length != right.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length");
            }

            int length = left.Length;
            var result = new float[length];

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left, i);
                var v2 = new Vector<float>(right, i);
                (v1 - v2).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = left[i] - right[i];
            }

            return result;
        }

        public static float[] MatrixAdd(float[] left, float[] right)
        {
            var result = new float[left.Length];
            MatrixAdd(left, right, ref result);
            return result;
        }

        public static void MatrixAddDotTanh(float[][] left, float[] right, float[] add, ref float[] result)
        {
            //// Plus activation... 
            //Utils.MatrixAdd(Utils.MatrixDot(weights[i - 1], neurons[i - 1]), biases[i - 1], ref neurons[i]);
            //// TODO: Activate better...
            //for (int j = 0; j < layers[i]; j++)
            //    neurons[i][j] = Activate(neurons[i][j], i - 1);

            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (left[0].Length != right.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length");
            }

            int baseLength = left.Length;
            int baseRemaining = baseLength % Vector<float>.Count;

            // Oh.  There's Vector.Dot...

            // Trying to figure out how to do it all vectory though.  dotting any two pairs gives us one value
            // And I guess we'd sum those values.  To sum something with SIMD we can dot the result with a vector of 1's...
            // Or, as we get each single value, just add it to our result for this x

            // For each x, we want to do the dot and add and activation all in the iteration
            for (int x = 0; x < baseLength; x++)
            {
                int length = left[x].Length;

                // Get the number of elements that can't be processed in the vector
                // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
                int remaining = length % Vector<float>.Count;

                for (int i = 0; i < length - remaining; i += Vector<float>.Count)
                {
                    var v1 = new Vector<float>(left[x], i);
                    var v2 = new Vector<float>(right, i);
                    result[x] += Vector.Dot(v1, v2);
                }

                for (int i = length - remaining; i < length; i++)
                {
                    result[x] += left[x][i] * right[i];
                }
                // Then add and tanh each x...?
                // I think this really has to become a vector... 

                // This is pretty hard to work through how to do this... 
                // But maybe only if x%count == 0, then we do the next however many?

                // No, they aren't ready at that point.  We do the previous ones
                // And we have to start when X==7, we then take from 0 to 7, 8 values
                if (x%Vector<float>.Count == Vector<float>.Count-1 && x > 0)
                {
                    var pos = x - (Vector<float>.Count - 1);
                    var v1 = new Vector<float>(result, pos);
                    // We take this, and add and activate
                    var v2 = new Vector<float>(add, pos);
                    (v1 + v2).CopyTo(result, pos); // We can't tanh here... 
                    // We kinda have to go back and re-iterate to tanh... so I wonder if any of this is worth it
                    for (int y = pos; y <= x; y++)
                        result[y] = (float)Math.Tanh(result[y]);
                }
                else if(x >= baseLength - baseRemaining)
                {
                    result[x] = (float)Math.Tanh(result[x] + add[x]);
                }

                
            }


        }

        public static void MatrixAdd(float[] left, float[] right, ref float[] result)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (left.Length != right.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length");
            }

            int length = left.Length;

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left, i);
                var v2 = new Vector<float>(right, i);
                (v1 + v2).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = left[i] + right[i];
            }
        }

        public static float[] MatrixDot(float[][] left, float[] right)
        {
            // left[] must be the same size as right (ie, left[n][x] should map to right[n])...?

            // Implying the hidden is set by summing all Result[j]+=HiddenToHidden[j][k]*Hidden[j] for all given k (input) and one given j (neuron)
            // Which should be equivalent to summing them then multiplying..?  (a+b)*c = a*c+b*c right?

            // But really it should come in as [k][j]*[j] and expect left[x][n] to map to right[n] I think

            // Honestly I can make it just work either way....?

            // I mean.  If we want the result to be the same as left.Length (we do)
            // Then the inner elements of left must match those of right, that's all there is to it
            //if (left.Length == right.Length)
            //    left = TransposeValues(left);


            // Something's still very stupidly wrong here.
            // I know that float[]*float[] should be float
            // So what is float[n][]*float[]... ? I think that's basically float[]*float[] n times, so a float[]

            var result = new float[left.Length];

            for (int j = 0; j < left.Length; j++)
            {
                result[j] = MatrixDot(left[j], right);
                //for(int k = 0; k < left[j].Length; k++)
                //    result[j] += left[j][k]*right[j];
            }
            return result;
        }

        public static float MatrixDot(float[] left, float[] right)
        {
            return MatrixMultiply(left, right).Sum();
        }

        public static void MatrixMultiply(float[][][] left, float right, ref float[][][] result)
        {
            for (int i = 0; i < left.Length; i++)
                MatrixMultiply(left[i], right, ref result[i]);
        }

        public static void MatrixMultiply(float[][] left, float right, ref float[][] result)
        {
            for (int i = 0; i < left.Length; i++)
                result[i] = MatrixMultiply(left[i], right);
        }

        public static float[][] MatrixMultiply(float[][] left, float right)
        {
            int length = left.Length;

            var result = new float[length][];

            for (int i = 0; i < length; i++)
                result[i] = MatrixMultiply(left[i], right);

            return result;
        }

        public static float[] MatrixMultiply(float[] left, float right)
        {
            int length = left.Length;

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;
            var result = new float[length];

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left, i);
                (v1 * right).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = left[i] * right;
            }

            return result;
        }

        public static float[][] MatrixMultiply(float[][] left, float[][] right)
        {
            // Multiplies each matching element of left vs right
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (left.Length != right.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length");
            }

            int length = left.Length;
            var result = new float[length][];

            for (int i = 0; i < length; i++)
            {
                if (left[i].Length != right[i].Length)
                {
                    throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length on the inner array");
                }
                result[i] = MatrixMultiply(left[i], right[i]);

            }

            return result;
        }

        public static float[] MatrixMultiply(float[] left, float[] right)
        {
            var result = new float[left.Length];
            MatrixMultiply(left, right, ref result);
            return result;
        }

        public static void MatrixMultiply(float[] left, float[] right, ref float[] result)
        { // Copied from https://learn.microsoft.com/en-us/dotnet/standard/simd 
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (left.Length != right.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} are not the same length");
            }

            if (left.Length != result.Length)
            {
                throw new ArgumentException($"{nameof(left)} and {nameof(result)} are not the same length");
            }

            if (right.Length != result.Length)
            {
                throw new ArgumentException($"{nameof(right)} and {nameof(result)} are not the same length");
            }

            int length = left.Length;

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left, i);
                var v2 = new Vector<float>(right, i);
                (v1 * v2).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = left[i] * right[i];
            }
        }

        public static float[] MatrixDerivativeTanh(float[] left)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }
            // 1 - x * x

            int length = left.Length;
            var result = new float[length];

            // Get the number of elements that can't be processed in the vector
            // NOTE: Vector<T>.Count is a JIT time constant and will get optimized accordingly
            int remaining = length % Vector<float>.Count;

            for (int i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(left, i);
                var v2 = new Vector<float>(1);
                (v2 - (v1 * v1)).CopyTo(result, i);
            }

            for (int i = length - remaining; i < length; i++)
            {
                result[i] = 1 - left[i] * left[i];
            }
            return result;
        }

        public static float[][] TransposeValues(float[][] x)
        {
            int a = x.Length;
            int b = x[0].Length;
            float[][] y = new float[b][];
            for (int j = 0; j < b; j++)
            {
                y[j] = new float[a];
                for (int i = 0; i < a; i++)
                {
                    y[j][i] = x[i][j];
                }
            }
            return y;
        }
    }
}
