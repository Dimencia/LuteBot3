using LuteBot.TrackSelection;
using Newtonsoft.Json;
using SimpleML;
using System.Data;

namespace LuteBot.UI
{
    public partial class NeuralNetworkForm : Form
    {
        private PartitionsForm pf;
        public NeuralNetworkForm(PartitionsForm pf)
        {
            this.pf = pf;
            InitializeComponent();

            if (File.Exists(savePath) && File.Exists(savePath + ".config"))
            {
                var sizes = JsonConvert.DeserializeObject<int[]>(File.ReadAllText(NeuralNetworkForm.savePath + ".config"));
                textBoxSizes.Enabled = false;
                textBoxSizes.Text = string.Join(",", sizes);
            }
        }

        public static readonly string savePath = Path.Combine(LuteBotForm.lutebotPath, "CustomNetworkv2");

        private async void buttonTrain_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonTrain.Text == "Train")
                {
                    //Invoke((MethodInvoker)delegate
                    //{
                    buttonTrain.Text = "Pause";

                    textBoxSizes.Enabled = false;
                    buttonTrain.Enabled = false;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    textBoxParallel.Enabled = false;
                    textBoxTrainingRate.Enabled = false;
                    richTextBox1.AppendText("\n\n\n\nLoading song data for training...");
                    //});
                    var sizes = textBoxSizes.Text.Split(',').Select(s => int.Parse(s)).ToArray();
                    int numPerfect = int.Parse(textBox1.Text);
                    float percentForSuccess = float.Parse(textBox2.Text);
                    int parallel = int.Parse(textBoxParallel.Text);
                    float trainingRate = float.Parse(textBoxTrainingRate.Text);
                    cancelled = false;
                    await TrainNetwork(numPerfect, percentForSuccess, parallel, trainingRate, sizes).ConfigureAwait(false);
                }
                else
                {
                    cancelled = true;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                richTextBox1.AppendText($"\nERROR: {ex.Message}.  Cannot continue");
                textBoxSizes.Enabled = true;
                buttonTrain.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBoxParallel.Enabled = true;
            }
        }


        private bool cancelled = false;

        MidiChannelItem[][] neuralTrainingCandidates = null;
        MidiChannelItem[][] neuralTestCandidates = null;

        private async Task<MidiChannelItem[]> ProcessFileForCandidates(string partitionName)
        {
            string midiPath = Path.Combine(PartitionsForm.partitionMidiPath, partitionName + ".mid");
            if (File.Exists(midiPath))
            {
                var tsm = await LuteBotForm.Instance.LoadFile(midiPath).ConfigureAwait(false);
                if (tsm.Player.dryWetFile != null)
                {
                    // To prevent it from trying to train on re-generated midis, which have incorrect lengths
                    // Require at least 3 channels

                    if (tsm.MidiChannels.Count > 2)
                    {
                        var activeChannels = tsm.MidiChannels.Values.Where(c => c.Id != 9 && c.MidiInstrument != 9 && c.TickNotes.Any(tn => tn.Value.Any())).OrderBy(c => c.Id).ToArray();
                        var activeTracks = tsm.MidiTracks.Values.Where(c => c.TickNotes.Any(tn => tn.Value.Any(n => n.channel != 9 && !tsm.MidiChannels.Values.Any(mc => mc.Id == n.channel && mc.MidiInstrument == 9)))).OrderBy(c => c.Id).ToArray();
                        if (activeChannels.Any(c => !c.Settings[1].Active) && activeChannels.Any(c => c.Settings[1].Active) && activeTracks.All(t => t.Settings[1].Active))
                        {
                            var tempNeuralCandidates = activeChannels;
                            foreach (var ca in tempNeuralCandidates)
                            {
                                ca.Settings[1].Active = tsm.MidiChannels.Values.Where(c => c.Id == ca.Id).Single().Settings[1].Active;
                            }
                            return tempNeuralCandidates;
                        }
                        else if (activeTracks.Any(c => !c.Settings[1].Active) && activeTracks.Any(c => c.Settings[1].Active) && activeChannels.All(c => c.Settings[1].Active))
                        {
                            // If some tracks are disabled and all channels are enabled, we'll take the tracks as data
                            // Track notes should already have discarded anything with channel 9
                            MidiChannelItem[] tempNeuralCandidates = activeTracks;
                            foreach (var ca in tempNeuralCandidates)
                            { // This may not be necessary; I could probably just take the MidiTracks from DataDictionary[1] but, it seemed to give odd results like it didn't have everything
                                ca.Settings[1].Active = tsm.MidiTracks.Values.Where(c => c.Id == ca.Id).Single().Settings[1].Active;
                            }
                            return tempNeuralCandidates;
                        }
                    }
                }
            }
            return null;
        }

        private async Task TrainNetwork(int numPerfect, float percentForSuccess, int parallelism, float trainingRate, params int[] sizes)
        {
            try
            {
                var candidates = new List<TrainingCandidate<MidiChannelItem>>();
                var random = new Random();


                if (neuralTrainingCandidates == null || neuralTestCandidates == null)
                {
                    await this.InvokeAsync(() =>
                    {
                        progressBarTraining.Maximum = pf.index.PartitionNames.Count + 1;
                        progressBarTraining.Value = 0;
                    }).ConfigureAwait(false);
                    int progress = 0;
                    var partNames = pf.index.PartitionNames.ToArray();

                    var tasks = new List<Task>();

                    var parallelNeuralCandidates = new MidiChannelItem[partNames.Length][];

                    // I tried a thousand ways to make this faster with parallel and async, but it was only ever slower, even when batching
                    // ... Which is weird that saving midis is so damn fast when it's parallel

                    // I think basically, the heavy processing to build candidates is too much for async to handle very well
                    // And I can't realistically load many TrackSelectionManagers at once due to memory 
                    await Parallel.ForEachAsync(partNames, async (part, cancel) =>
                    //foreach (var part in partNames)
                    {
                        var c = await ProcessFileForCandidates(part).ConfigureAwait(false);
                        var progressBar = Interlocked.Increment(ref progress);
                        parallelNeuralCandidates[progressBar - 1] = c;
                        BeginInvoke((MethodInvoker)delegate // Intentionally not awaiting so we can continue faster
                        {
                            progressBarTraining.Value = progressBar;
                        });
                    }).ConfigureAwait(false);

                    var orderedCandidates = parallelNeuralCandidates.Where(c => c != null).Select(c => c.OrderBy(ch => ch.Id).ToArray()).OrderBy(c => random.Next());
                    var nTestCandidates = (int)(orderedCandidates.Count() * 0.3f);
                    neuralTrainingCandidates = orderedCandidates.Skip(nTestCandidates).ToArray();
                    neuralTestCandidates = orderedCandidates.Take(nTestCandidates).ToArray();
                }

                var numTestCandidates = neuralTestCandidates.Length;


                float costTotal = 999f;

                int numTestsCorrect = 0;
                int numActualTestsCorrect = 0;

                BeginInvoke((MethodInvoker)delegate
                {
                    buttonTrain.Enabled = true;
                    richTextBox1.AppendText($"\n\nTraining neural network until all {numTestCandidates} non-training candidates are correct");
                    progressBarTraining.Maximum = numTestCandidates;
                    progressBarTraining.Value = 0;
                });

                var neural = TrackSelectionManager.SetupNeuralNetwork(sizes);
                int i = 0;
                int numSuccesses = 0;
                while (numSuccesses < numPerfect && !cancelled)
                //while(costTotal > 0.1f && i < trainCount)
                //for (int i = 0; i < trainCount; i++)
                {
                    costTotal = 0;
                    object songCostLock = new object();
                    numTestsCorrect = 0;
                    numActualTestsCorrect = 0;
                    Parallel.ForEach(neuralTrainingCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        if (!cancelled)
                        {
                            var songCostTotal = 0f;
                            int count = 0;
                            foreach (var channel in song)
                            {
                                // So for the 'recurrent memory' implementation, we have memory_count*noteParams*2 inputs; 16 notes, each time we process another note, we push the others up the chain
                                // and pop off the oldest.  Then we have memory_count*noteParams in the final hidden layer, which should be fed back into the last memory_count*noteParams*2 inputs next time

                                // We need an alternate backPropagation that doesn't feedforward, or rather, does all of this before it tries correcting
                                //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);

                                var inputs = channel.GetNeuralInputs(count++, song.Length);
                                var expected = new float[1];

                                if (channel.Settings[1].Active)
                                {
                                    expected[0] = 0.5f;
                                }
                                else
                                {
                                    expected[0] = -0.5f; // 1 and -1 gets sort of stuck because of the nature of tanh, let's try .75 though
                                }

                                /*
                                var inputs = song.GetNeuralInput();

                                float numActive = song.Count(c => c.Active);
                                float[] expected = new float[16];

                                for(int j = 0; j < 16; j++)
                                {
                                    var channel = song.Where(c => c.Id == j).SingleOrDefault();
                                    //var channel = song.Values[j];

                                    expected[j] = 0;

                                    if (channel != null) {
                                        if (channel.Active)
                                            expected[j] = 1 / numActive;
                                    }
                                    else
                                    {
                                        for (int k = 0; k < numParamsPerChannel; k++)
                                            inputs[j * numParamsPerChannel + k] = 0; // Tanh should evaluate this as -1 to keep it from taking weight
                                    }
                                }
                                */

                                //tsm.neural.BackPropagateRecurrent(inputs, expected);
                                songCostTotal += neural.BackPropagate(inputs, expected);
                            }
                            if ((i + 1) % 20 == 0)
                                lock (songCostLock)
                                    costTotal += songCostTotal;
                        }
                    });

                    Parallel.ForEach(neuralTrainingCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        if (!cancelled)
                        {
                            Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();

                            int count = 0;
                            foreach (var channel in song)
                            {
                                var inputs = channel.GetNeuralInputs(count++, song.Length);
                                //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);
                                //var neuralResults = tsm.neural.FeedForwardRecurrent(inputs);
                                var neuralResults = neural.FeedForward(inputs);
                                channelResults[channel] = neuralResults[0];
                            }

                            var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);

                            /*
                            var inputs = song.GetNeuralInput();

                            var neuralResults = tsm.neural.FeedForward(inputs);
                            // The output here is a channel ID to confidence map...
                            // I need to find the ID of the best one... and rank the rest...
                            // So let's just build a quick dictionary I guess
                            var results = new Dictionary<int, float>();

                            for (int j = 0; j < neuralResults.Length; j++)
                            {
                                results[j] = neuralResults[j];
                            }

                            var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                            */
                            // Check the number of active flute channels...
                            var numFlute = song.Where(s => s.Settings[1].Active).Count();
                            bool correct = true;

                            for (int j = 0; j < numFlute; j++)
                            {
                                bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Settings?[1]?.Active;
                                if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                                    correct = false;
                            }

                            if (correct)
                                Interlocked.Increment(ref numTestsCorrect);

                        }
                        //await Task.Delay(0); // Let the form live between iterations
                    });
                    Parallel.ForEach(neuralTestCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        if (!cancelled)
                        {
                            Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                            int count = 0;
                            foreach (var channel in song)
                            {
                                //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);
                                //var neuralResults = tsm.neural.FeedForwardRecurrent(inputs);
                                var inputs = channel.GetNeuralInputs(count++, song.Length);
                                var neuralResults = neural.FeedForward(inputs);
                                channelResults[channel] = neuralResults[0];
                            }

                            var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);

                            /*
                            var inputs = song.GetNeuralInput();

                            var neuralResults = tsm.neural.FeedForward(inputs);
                            // The output here is a channel ID to confidence map...
                            // I need to find the ID of the best one... and rank the rest...
                            // So let's just build a quick dictionary I guess
                            var results = new Dictionary<int, float>();

                            for (int j = 0; j < neuralResults.Length; j++)
                            {
                                results[j] = neuralResults[j];
                            }

                            var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                            */
                            // Check the number of active flute channels...
                            var numFlute = song.Where(s => s.Settings[1].Active).Count();
                            bool correct = true;

                            for (int j = 0; j < numFlute; j++)
                            {
                                bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Settings?[1]?.Active;
                                if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                                    correct = false;
                            }

                            if (correct)
                            {
                                Interlocked.Increment(ref numActualTestsCorrect);
                            }

                            //int fluteCount = 0;
                            //foreach (var channel in channelResults.Keys)
                            ////foreach (var channel in activeChannels)
                            //{
                            //    //var channel = activeChannels.Where(c => c.Id == orderedResults.ElementAt(i).Key).SingleOrDefault();
                            //    
                            //    if (channel != null)
                            //    {
                            //        Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {channelResults[channel]}");
                            //        //Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {neuralResults[channel.Id]}");
                            //        //channel.Name += $"(Flute Rank {++fluteCount} - {Math.Round(channelResults[channel], 2)}%)";
                            //    }
                            //}

                            //await Task.Delay(0); // Let the form live between iterations
                        }
                    });
                    float invokeTotal = costTotal;
                    int numTestCorrect = numActualTestsCorrect;
                    int numTrainingCorrect = numTestsCorrect;
                    Interlocked.Increment(ref i);
                    int trainingNum = i;
                    if (i % 20 == 0)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (trainingNum % (20 * 1000) == 0)
                            {
                                // 1k lines, let's clear it
                                richTextBox1.Clear();
                            }
                            richTextBox1.AppendText($"\n{numTestCorrect}/{neuralTestCandidates.Length} ({(float)numTestCorrect / neuralTestCandidates.Length * 100}%) tests correct; {percentForSuccess}% {numPerfect} times in a row to finish.  Training Set: {numTrainingCorrect}/{neuralTrainingCandidates.Length}\nTraining #{trainingNum} - TotalCost: {invokeTotal} (This number should go down eventually)");
                            richTextBox1.ScrollToCaret();
                            progressBarTraining.Value = numActualTestsCorrect;

                        });
                        await Task.Delay(1).ConfigureAwait(false); // Let the form live between iterations
                    }

                    //orderedCandidates = neuralCandidates.OrderBy(c => random.Next());
                    //neuralTrainingCandidates = orderedCandidates.Take(numTrainingCandidates);
                    //neuralTestCandidates = orderedCandidates.Skip(numTrainingCandidates);

                    if ((float)numActualTestsCorrect / neuralTestCandidates.Count() < (percentForSuccess / 100f))
                        numSuccesses = 0;
                    else
                        Interlocked.Increment(ref numSuccesses);


                }

                BeginInvoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText($"\n\n----Training Complete----\n\nNetwork saved to {savePath}\nYou may now load midis and check it for yourself, and this will be your new default network. \n\nYou can revert at any time by deleting this file, or train again to replace it");
                    richTextBox1.ScrollToCaret();
                    buttonTrain.Enabled = true;
                    textBox1.Enabled = true;
                    textBoxParallel.Enabled = true;
                    textBox2.Enabled = true;
                    textBoxTrainingRate.Enabled = true;
                    buttonTrain.Text = "Train";
                });
                if (File.Exists(savePath))
                    File.Delete(savePath);
                neural.Save(savePath);
                if (File.Exists(savePath + ".config"))
                    File.Delete(savePath + ".config");
                File.WriteAllText(savePath + ".config", JsonConvert.SerializeObject(sizes));

                /* It is still odd that this gives different results than the last one that ran
                numTestsCorrect = 0;
                foreach (var song in neuralCandidates)
                {
                    float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                    float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                    float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                    Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                    foreach (var channel in song)
                    {
                        var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                        var neuralResults = tsm.neural.FeedForward(inputs);
                        channelResults[channel] = neuralResults[0];
                    }

                    var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);



                    //var inputs = song.GetNeuralInput();
                    //
                    //var neuralResults = tsm.neural.FeedForward(inputs);
                    //// The output here is a channel ID to confidence map...
                    //// I need to find the ID of the best one... and rank the rest...
                    //// So let's just build a quick dictionary I guess
                    //var results = new Dictionary<int, float>();
                    //
                    //for (int j = 0; j < neuralResults.Length; j++)
                    //{
                    //    results[j] = neuralResults[j];
                    //}
                    //
                    //var orderedResults = results.OrderByDescending(kvp => kvp.Value);

                    // Check the number of active flute channels...
                    var numFlute = song.Where(s => s.Active).Count();
                    bool correct = true;

                    for (int j = 0; j < numFlute; j++)
                    {
                        bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Active;
                        if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                            correct = false;
                    }

                    if (correct)
                        numTestsCorrect++;
                }
                Console.WriteLine($"Complete - {numTestsCorrect} tests correct out of {neuralCandidates.Count()}");
                // TODO: Save the values somewhere... I've got them pasted as a screenshot in officers chat, if I need to
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //await TestMusicMaker().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class NeuralNetworkCandidate
    {
        public MidiChannelItem[][] Channels { get; set; }
        public TrackSelectionManager TrackSelectionManager { get; set; }
    }
}
