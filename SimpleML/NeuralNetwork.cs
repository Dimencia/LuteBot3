using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SimpleML
{
    public class NeuralNetwork
    {
        // An attempt at a basic neural network implementation

        private int[] layers { get; set; } // The sizes of the networks, in order, input/hidden/output
        private float[][] neurons { get; set; } // Holds the values generated; the first index is the layer, the second is the neuron
        private float[][] biases { get; set; } // Biases work in the same way
        private float[][][] weights; // First index is layer, second is neuron, third is the other neuron
        private int[] activations { get; set; }//layers
        // Weights stored for each neuron are the weight to each neuron in the previous layer

        public float learningRate { get; set; } = 0.01f;//learning rate
        public float cost { get; private set; } = 0;
        private Random random;

        private float[][] deltaBiases { get; set; }//biasses
        private float[][][] deltaWeights { get; set; }//weights
        private int deltaCount { get; set; }

        // Populates the arrays... 
        public NeuralNetwork(int[] layers, string[] layerActivations)
        {
            this.layers = new int[layers.Length];
            layers.CopyTo(this.layers, 0);

            random = new Random();

            activations = new int[layers.Length - 1];
            for (int i = 0; i < layers.Length - 1; i++)
            {
                string action = layerActivations[i];
                switch (action)
                {
                    case "sigmoid":
                        activations[i] = 0;
                        break;
                    case "tanh":
                        activations[i] = 1;
                        break;
                    case "relu":
                        activations[i] = 2;
                        break;
                    case "leakyrelu":
                        activations[i] = 3;
                        break;
                    case "softmax":
                        activations[i] = 4;
                        break;
                    default:
                        activations[i] = 2;
                        break;
                }
            }

            InitNeurons();
            InitBiases();
            InitWeights();
        }

        // Initialize empty arrays, of size specified by the layer
        private void InitNeurons()
        {
            List<float[]> neuronsList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                neuronsList.Add(new float[layers[i]]);
            }
            neurons = neuronsList.ToArray();
        }

        // Initialize biases, which are randomized for simplicity; this would need to be reworked to avoid gradient problems, if the algorithm wasn't genetic
        private void InitBiases()
        {
            List<float[]> biasList = new List<float[]>();
            for (int i = 1; i < layers.Length; i++)
            {
                float[] bias = new float[layers[i]];
                for (int j = 0; j < layers[i]; j++)
                {
                    bias[j] = (float)(random.NextDouble() - 0.5f);
                }
                biasList.Add(bias);
            }
            biases = biasList.ToArray();
        }

        // Weights are also randomized.  They are stored in [layer#][neuron#][neuron# from previous layer]
        private void InitWeights()
        {
            List<float[][]> weightsList = new List<float[][]>();
            for (int i = 1; i < layers.Length; i++) // First layer doesn't have any previous, so no weights
            {
                List<float[]> layerWeightsList = new List<float[]>();
                int neuronsInPreviousLayer = layers[i - 1];
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float[] neuronWeights = new float[neuronsInPreviousLayer];
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        neuronWeights[k] = (float)(random.NextDouble() - 0.5f);
                    }
                    layerWeightsList.Add(neuronWeights);
                }
                weightsList.Add(layerWeightsList.ToArray());
            }
            weights = weightsList.ToArray();
        }


        // Activation function
        // These are complicated
        // The point is that it takes the weighted sum, and does something nonlinear to it, because what we're modeling probably isn't linear

        // Backpropagation apparently means we need to define many of them, and their derivatives
        // Thanks to this guy: https://github.com/kipgparker/BackPropNetwork/blob/master/BackpropNeuralNetwork/Assets/NeuralNetwork.cs
        public float Activate(float value, int layer)//all activation functions
        {
            switch (activations[layer])
            {
                case 0:
                    return sigmoid(value);
                case 1:
                    return tanh(value);
                case 2:
                    return relu(value);
                case 3:
                    return leakyrelu(value);
                default:
                    return relu(value);
            }
        }
        public float activateDer(float value, int layer)//all activation function derivatives
        {
            switch (activations[layer])
            {
                case 0:
                    return sigmoidDer(value);
                case 1:
                    return tanhDer(value);
                case 2:
                    return reluDer(value);
                case 3:
                    return leakyreluDer(value);
                case 4:
                    return sigmoidDer(value); // softmax has the same deriv as sigmoid
                default:
                    return reluDer(value);
            }
        }

        public float sigmoid(float x)//activation functions and their corrosponding derivatives
        {
            float k = (float)Math.Exp(x);
            return k / (1.0f + k);
        }
        public float tanh(float x)
        {
            return (float)Math.Tanh(x);
        }
        public float relu(float x)
        {
            return (0 >= x) ? 0 : x;
        }
        public float leakyrelu(float x)
        {
            return (0 >= x) ? 0.01f * x : x;
        }
        public float sigmoidDer(float x)
        {
            return x * (1 - x);
        }
        public float tanhDer(float x)
        {
            return 1 - (x * x);
        }
        public float reluDer(float x)
        {
            return (0 >= x) ? 0 : 1;
        }
        public float leakyreluDer(float x)
        {
            return (0 >= x) ? 0.01f : 1;
        }

        // I want to use this, but our outputs are just... one value.  Unless I can rework this to take in a whole song and make it try
        public float[] Softmax(float[] oSums)
        {
            // determine max output sum
            // does all output nodes at once so scale doesn't have to be re-computed each time
            float max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor -- sum of exp(each val - max)
            float scale = 0.0f;
            for (int i = 0; i < oSums.Length; ++i)
                scale += (float)(Math.Exp(oSums[i] - max));

            float[] result = new float[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = (float)(Math.Exp(oSums[i] - max) / scale);

            return result; // now scaled so that xi sum to 1.0
        }


        public object ParallelLock = new object();

        public void BackPropagate(float[] inputs, float[] expected)//backpropogation;
        {
            lock (ParallelLock) // Well this is pointless.  Obviously I can't go parallel when the inputs need to be populated throughout all of this
            {
                // One way to make this parallel might involve locking, deep-copying the neurons, unlocking, 
                // Doing our math and calculating a final set of adjustments to make
                // Then lock and apply them

                float[] output = FeedForward(inputs);//runs feed forward to ensure neurons are populated correctly
                cost = 0;
                for (int i = 0; i < output.Length; i++) cost += (float)Math.Pow(output[i] - expected[i], 2);//calculated cost of network
                                                                                                            //for (int i = 0; i < output.Length; i++) cost += (output[i] - expected[i]);//calculated cost of network - removed pow because we were overflowing to nan
                cost = cost / 2;//this value is not used in calculions, rather used to identify the performance of the network

                float[][] gamma;


                List<float[]> gammaList = new List<float[]>();
                for (int i = 0; i < layers.Length; i++)
                {
                    gammaList.Add(new float[layers[i]]);
                }
                gamma = gammaList.ToArray();//gamma initialization

                int layer = layers.Length - 2;
                for (int i = 0; i < output.Length; i++) gamma[layers.Length - 1][i] = (output[i] - expected[i]) * activateDer(output[i], layer);//Gamma calculation


                for (int i = 0; i < layers[layers.Length - 1]; i++)//calculates the w' and b' for the last layer in the network
                {
                    biases[layers.Length - 2][i] -= gamma[layers.Length - 1][i] * learningRate;
                    for (int j = 0; j < layers[layers.Length - 2]; j++)
                    {
                        weights[layers.Length - 2][i][j] -= gamma[layers.Length - 1][i] * neurons[layers.Length - 2][j] * learningRate;//*learning 
                    }
                }

                for (int i = layers.Length - 2; i > 0; i--)//runs on all hidden layers
                {
                    layer = i - 1;
                    for (int j = 0; j < layers[i]; j++)//outputs
                    {
                        gamma[i][j] = 0;
                        for (int k = 0; k < gamma[i + 1].Length; k++)
                        {
                            gamma[i][j] += gamma[i + 1][k] * weights[i][k][j];
                        }
                        gamma[i][j] *= activateDer(neurons[i][j], layer);//calculate gamma
                    }

                    for (int j = 0; j < layers[i]; j++)//itterate over outputs of layer
                    {
                        biases[i - 1][j] -= gamma[i][j] * learningRate;//modify biases of network
                        for (int k = 0; k < layers[i - 1]; k++)//itterate over inputs to layer
                        {
                            weights[i - 1][j][k] -= gamma[i][j] * neurons[i - 1][k] * learningRate;//modify weights of network
                        }
                    }
                }
            }
        }


        public (List<(int, int, int, float)>, List<(int, int, float)>) BackPropagateParallel(float[] inputs, float[] expected)
        {
            float[] output = FeedForward(inputs);//runs feed forward to ensure neurons are populated correctly
            cost = 0;
            for (int i = 0; i < output.Length; i++) cost += (float)Math.Pow(output[i] - expected[i], 2);//calculated cost of network
                                                                                                        //for (int i = 0; i < output.Length; i++) cost += (output[i] - expected[i]);//calculated cost of network - removed pow because we were overflowing to nan
            cost = cost / 2;//this value is not used in calculions, rather used to identify the performance of the network

            float[][] gamma;
            List<(int, int, int, float)> weightModifications = new List<(int, int, int, float)>();
            List<(int, int, float)> biasModifications = new List<(int, int, float)>();

            List<float[]> gammaList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                gammaList.Add(new float[layers[i]]);
            }
            gamma = gammaList.ToArray();//gamma initialization

            int layer = layers.Length - 2;
            for (int i = 0; i < output.Length; i++) gamma[layers.Length - 1][i] = (output[i] - expected[i]) * activateDer(output[i], layer);//Gamma calculation
            for (int i = 0; i < layers[layers.Length - 1]; i++)//calculates the w' and b' for the last layer in the network
            {
                //biases[layers.Length - 2][i] -= gamma[layers.Length - 1][i] * learningRate;
                biasModifications.Add((layers.Length - 2, i, -gamma[layers.Length - 1][i] * learningRate));
                for (int j = 0; j < layers[layers.Length - 2]; j++)
                {
                    weightModifications.Add((layers.Length - 2, i, j, -gamma[layers.Length - 1][i] * neurons[layers.Length - 2][j] * learningRate));
                    //weights[layers.Length - 2][i][j] -= gamma[layers.Length - 1][i] * neurons[layers.Length - 2][j] * learningRate;//*learning 
                }
            }

            for (int i = layers.Length - 2; i > 0; i--)//runs on all hidden layers
            {
                layer = i - 1;
                for (int j = 0; j < layers[i]; j++)//outputs
                {
                    gamma[i][j] = 0;
                    for (int k = 0; k < gamma[i + 1].Length; k++)
                    {
                        gamma[i][j] += gamma[i + 1][k] * weights[i][k][j];
                    }
                    gamma[i][j] *= activateDer(neurons[i][j], layer);//calculate gamma
                }
                for (int j = 0; j < layers[i]; j++)//itterate over outputs of layer
                {
                    //biases[i - 1][j] -= gamma[i][j] * learningRate;//modify biases of network
                    biasModifications.Add((i - 1, j, -gamma[i][j] * learningRate));
                    for (int k = 0; k < layers[i - 1]; k++)//itterate over inputs to layer
                    {
                        //weights[i - 1][j][k] -= gamma[i][j] * neurons[i - 1][k] * learningRate;//modify weights of network
                        weightModifications.Add((i - 1, j, k, -gamma[i][j] * neurons[i - 1][k] * learningRate));
                    }
                }
            }

            return (weightModifications, biasModifications);

        }


        public float[] FeedForwardRecurrent(float[][] inputs)
        {
            float[] currentInputs = new float[layers[0]];
            int inputPosition = 0;
            float[] output = new float[layers[layers.Length - 1]];

            // First populate it with layers[0]/2 inputs, assuming that they're already ascending and time-ordered
            for (int i = inputPosition; i < inputs.Length && i < inputPosition + (layers[0] / 2); i++)
            {
                for (int j = 0; j < inputs[i].Length; j++)
                {
                    currentInputs[i + j] = inputs[i][j];
                }
            }
            // And the input position only increases by 1
            inputPosition++;

            while (inputPosition < inputs.Length - layers[0] / 2)
            {
                // Then feed it forward... we'll store the ouput but we don't really care, we actually need the output from the last neurons if we have any more input
                // We only have more input if inputPosition is less than layers[0]/2, otherwise all the data is already there
                // Or to put that in a more normal way, if input position is less than inputs.Length-layers[0]/2
                output = FeedForward(currentInputs);
                if (inputPosition < inputs.Length - layers[0] / 2)
                {
                    var lastLayerOutputs = neurons[layers.Length - 2];
                    // Now give it the next set of inputs, with these tacked on the end
                    for (int i = inputPosition; i < inputs.Length && i < inputPosition + (layers[0] / 2); i++)
                    {
                        for (int j = 0; j < inputs[i].Length; j++)
                        {
                            currentInputs[i - inputPosition + j] = inputs[i][j];
                        }
                    }
                    for (int i = 0; i < lastLayerOutputs.Length; i++)
                    {
                        currentInputs[i + layers[0] / 2] = lastLayerOutputs[i];
                    }
                    inputPosition++;
                }
            }
            return output;
        }


        // So for the 'recurrent memory' implementation, we have memory_count*noteParams*2 inputs; 16 notes, each time we process another note, we push the others up the chain
        // and pop off the oldest.  Then we have memory_count*noteParams in the final hidden layer, which should be fed back into the last memory_count*noteParams*2 inputs next time

        // We need an alternate backPropagation that doesn't feedforward, or rather, does all of this before it tries correcting
        public void BackPropagateRecurrent(float[][] inputs, float[] expected)
        {
            // So first, FeedForward everything in inputs... oh.  No this works as normal doesn't it... 
            // No.  inputs is huge.  Each [0] contains 48 inputs, from the 15 last notes and the next one, for each note...
            // That's really huge.  Stupidly huge.  
            // Well alright, each one just contains 3 inputs, but is in order
            // So we start by popping in the first 16 input sets, then start iterating until we're done
            // I guess we'll put the most recent notes at the bottom, the natural way it'd be ordered


            float[] output = FeedForwardRecurrent(inputs);
            // Output is the actual answer now, and the rest should work as normal
            // Though I seriously doubt this is going to be at all useful
            // How could it possibly train it on the data going through without adjusting weights/biases each note?
            // Oh well. 



            cost = 0;
            for (int i = 0; i < output.Length; i++) cost += (float)Math.Pow(output[i] - expected[i], 2);//calculated cost of network
            cost = cost / 2;//this value is not used in calculions, rather used to identify the performance of the network

            float[][] gamma;


            List<float[]> gammaList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                gammaList.Add(new float[layers[i]]);
            }
            gamma = gammaList.ToArray();//gamma initialization

            int layer = layers.Length - 2;
            for (int i = 0; i < output.Length; i++) gamma[layers.Length - 1][i] = (output[i] - expected[i]) * activateDer(output[i], layer);//Gamma calculation
            for (int i = 0; i < layers[layers.Length - 1]; i++)//calculates the w' and b' for the last layer in the network
            {
                biases[layers.Length - 2][i] -= gamma[layers.Length - 1][i] * learningRate;
                for (int j = 0; j < layers[layers.Length - 2]; j++)
                {

                    weights[layers.Length - 2][i][j] -= gamma[layers.Length - 1][i] * neurons[layers.Length - 2][j] * learningRate;//*learning 
                }
            }

            for (int i = layers.Length - 2; i > 0; i--)//runs on all hidden layers
            {
                layer = i - 1;
                for (int j = 0; j < layers[i]; j++)//outputs
                {
                    gamma[i][j] = 0;
                    for (int k = 0; k < gamma[i + 1].Length; k++)
                    {
                        gamma[i][j] += gamma[i + 1][k] * weights[i][k][j];
                    }
                    gamma[i][j] *= activateDer(neurons[i][j], layer);//calculate gamma
                }
                for (int j = 0; j < layers[i]; j++)//itterate over outputs of layer
                {
                    biases[i - 1][j] -= gamma[i][j] * learningRate;//modify biases of network
                    for (int k = 0; k < layers[i - 1]; k++)//itterate over inputs to layer
                    {
                        weights[i - 1][j][k] -= gamma[i][j] * neurons[i - 1][k] * learningRate;//modify weights of network
                    }
                }
            }
        }


        // And this is basically the meat of the thing
        // It should iterate each neuron, get each value from the previous layer, multiply it by the value of the weight
        // Each neuron adds all these together, the values*weights of every previous layer's neuron
        // Adds the bias, runs it through the activation function, and then sets its own value
        // The process then continues forward
        // It then returns the final set of neuron values
        public float[] FeedForward(float[] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[0][i] = inputs[i]; // The inputs go straight into the 0th layer
            }
            for (int i = 1; i < layers.Length; i++) // Start at layer 1, the first with any previous nuerons to work on
            {
                int layer = i - 1;
                for (int j = 0; j < layers[i]; j++)
                {
                    float value = 0f;
                    for (int k = 0; k < layers[i - 1]; k++)
                    {
                        value += weights[i - 1][j][k] * neurons[i - 1][k];
                    }// For each neuron in this layer, get each neuron's value * weight from the previous layer, and sum them
                    // Then add the bias and activate
                    //if (i != layers.Length - 1)
                    if (activations[layer] != 4)
                        neurons[i][j] = Activate(value + biases[i - 1][j], layer);
                    else // Force softmax, which must run on the whole set
                        neurons[i][j] = value + biases[i - 1][j];
                }
                if (activations[layer] == 4)
                    neurons[i] = Softmax(neurons[i]);
            }
            //neurons[layers.Length - 1] = Softmax(neurons[layers.Length - 1]);
            return neurons[layers.Length - 1];
        }



        //Load from file so the results can actually be saved/reused
        // Copied from the internet, this looks like a really bad technique to me, but whatever
        public void Load(string path)
        {
            TextReader tr = new StreamReader(path);
            int NumberOfLines = (int)new FileInfo(path).Length;
            string[] ListLines = new string[NumberOfLines];
            int index = 1;
            for (int i = 1; i < NumberOfLines; i++)
            {
                ListLines[i] = tr.ReadLine();
            }
            tr.Close();
            if (new FileInfo(path).Length > 0)
            {
                for (int i = 0; i < biases.Length; i++)
                {
                    for (int j = 0; j < biases[i].Length; j++)
                    {
                        biases[i][j] = float.Parse(ListLines[index], CultureInfo.InvariantCulture);
                        index++;
                    }
                }
                for (int i = 0; i < weights.Length; i++)
                {
                    for (int j = 0; j < weights[i].Length; j++)
                    {
                        for (int k = 0; k < weights[i][j].Length; k++)
                        {
                            weights[i][j][k] = float.Parse(ListLines[index], CultureInfo.InvariantCulture);
                            index++;
                        }
                    }
                }
            }
        }

        public void Save(string path)
        {
            File.Create(path).Close();
            StreamWriter writer = new StreamWriter(path, true);

            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    writer.WriteLine(biases[i][j].ToString(CultureInfo.InvariantCulture));
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        writer.WriteLine(weights[i][j][k].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            writer.Close();
        }


    }
}
