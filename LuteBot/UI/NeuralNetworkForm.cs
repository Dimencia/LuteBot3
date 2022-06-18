using LuteBot.TrackSelection;
using Newtonsoft.Json;
using SimpleML;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class NeuralNetworkForm : Form
    {
        private TrackSelectionManager tsm;
        private PartitionsForm pf;
        public NeuralNetworkForm(TrackSelectionManager tsm, PartitionsForm pf)
        {
            this.tsm = tsm;
            this.pf = pf;
            InitializeComponent();
        }

        public static string savePath = Path.Combine(LuteBotForm.lutebotPath, "CustomNetwork");

        private async void buttonTrain_Click(object sender, EventArgs e)
        {
            try
            {
                //Invoke((MethodInvoker)delegate
                //{
                textBoxSizes.Enabled = false;
                buttonTrain.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBoxParallel.Enabled = false;
                richTextBox1.AppendText("\n\n\n\nLoading song data for training...");
                //});
                var sizes = textBoxSizes.Text.Split(',').Select(s => int.Parse(s)).ToArray();
                int numPerfect = int.Parse(textBox1.Text);
                float percentForSuccess = float.Parse(textBox2.Text);
                int parallel = int.Parse(textBoxParallel.Text);
                await TrainNetwork(numPerfect, percentForSuccess, parallel, sizes);
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

        private Random random = new Random();

        private void TrainMusicMaker(IEnumerable<MidiChannelItem[]> neuralTrainingCandidates)
        {
            // This is a simple implementation that will assumedly be ass because it has no memory or concept of time
            // But the idea is to give it say, 10 notes, and ask it for the next one
            // For each note, we give it value, deltaTick, duration, and velocity

            // The output should be a value, deltaTick, duration, and velocity (nvm, we don't even track that atm)

            // I kind of want to try relu or leakyrelu again; maybe it is more appropriate here?  Nah it was still totally broken
            int numNotes = 60;

            var layers = new int[] { numNotes * 3 + 1, 512, 512, 256, 256, 128, 128, 64, 64, 32, 32, 32, 32, 32, 32, 32, 32, 3 };
            var activations = new string[] { "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh" };

            var neural = new NeuralNetwork(layers, activations);

            


            int numIterations = 50000;

            var candidates = neuralTrainingCandidates.Select(s => s.SelectMany(c => c.tickNotes.Values.Select(nl => nl.OrderBy(n => n.note).Where((n,i) => (i == 0 || i == c.tickNotes.Count-1 || (c.tickNotes.Count > 2 && i == c.tickNotes.Count/2)) && n.channel != 9 && n.length > 0 && n.timeLength >= 0.005f))).SelectMany(nl => nl.GroupBy(n => n.note).Select(ng => ng.FirstOrDefault())).OrderBy(n => n.tickNumber).ThenBy(n => n.note).ToArray()).Where(nl => nl.Count() > numNotes).ToArray();

            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                float costTotal = 0;

                Parallel.ForEach(candidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = 12 }, notes =>
                //foreach (var song in neuralTrainingCandidates.OrderBy(n => random.NextDouble()))
                {
                    // Each time we train for a song, we should skip some arbitrary percentage into it
                    //var notes = song.SelectMany(c => c.tickNotes.Values).SelectMany(tn => tn).Where(n => n.channel != 9 && n.length > 0 && n.timeLength > 0).OrderBy(n => n.tickNumber).ToArray();

                    var percentToSkip = random.NextDouble();
                    int startPosition = (int)(notes.Length * percentToSkip);

                    if (startPosition + numNotes >= notes.Length) // Note that startPos+10 is actually 11 elements, which is what we want
                        startPosition -= numNotes;

                    var notesToTrain = notes.Skip(startPosition).Take(numNotes).OrderBy(n => n.tickNumber).ThenBy(n => n.note).ToArray();
                    var nextNote = notes[startPosition + numNotes];

                    float[] inputs = new float[layers[0]];

                    float ticksPerSecond = 0;
                    for (int i = 0; i < notesToTrain.Length; i++)
                    {
                        var note = notesToTrain[i];

                        // Find out how many ticks are in a second, or rather, a scalar that converts a tick duration to a time one for the current tempo
                        ticksPerSecond = note.length / note.timeLength;

                        inputs[i * 3] = (note.note - 64) / 64f;
                        inputs[i * 3 + 1] = Math.Max(Math.Min((((note.tickNumber - notesToTrain[Math.Max(i - 1, 0)].tickNumber) / ticksPerSecond) - 1f), 1), -1);
                        inputs[i * 3 + 2] = Math.Max(Math.Min((Math.Max(note.timeLength, 0.005f) - 1f), 1), -1);
                    }
                    inputs[inputs.Length - 1] = (float)percentToSkip;
                    ticksPerSecond = nextNote.length / nextNote.timeLength;
                    var expected = new float[] { (nextNote.note - 64) / 64f, Math.Max(Math.Min((((nextNote.tickNumber - notesToTrain[notesToTrain.Length - 1].tickNumber) / ticksPerSecond) - 1f), 1), -1), Math.Max(Math.Min((Math.Max(nextNote.timeLength, 0.005f) - 1f), 1), -1) };

                    neural.BackPropagate(inputs, expected);
                    costTotal += neural.cost;
                });
                Console.WriteLine($"#{iteration}: Cost - {costTotal}");
            }

            neural.Save("TestSongMaker");
            Console.WriteLine("Done");
        }


        private int GetPentatonicNote(int original)
        {
            // Pentatonic notes are: C, D, E, G, A, C
            // Which means they are in positions 0,2,4,7,9, then 12

            // I guess one way to do that, if it's <=5 round to the nearest even... (how do you even...)
            // and >= 6 round to the nearest higher odd

            // Alternatively, we can call it 5 notes out of 12 in the octave
            // So take original%12/12f*5 and have that go through a lookup array

            int[] pentatonics = new int[] { 0, 2, 4, 7, 9 };
            int targetIndex = (int)((original % 12) / 12f * pentatonics.Length);

            return (original - original%12) + pentatonics[targetIndex];
        }


        private async Task TestMusicMaker()
        {
            int numNotes = 60;

            var layers = new int[] { numNotes * 3 + 1, 512, 512, 256, 256, 128, 128, 64, 64, 32, 32, 32, 32, 32, 32, 32, 32, 3 };
            var activations = new string[] { "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh", "tanh" };
            var neural = new NeuralNetwork(layers, activations);

            

            neural.Load("TestSongMaker");

            // Assume it has some song given to us, arbitrarily, from which we will take 10 sequential notes from somewhere random
            // Each time it gives us an output, we append it to the end and drop the oldest

            // The issue is getting this into anything... we don't know how to save midis
            // I think the only feasible test is building a lutemod partition out of it

            // But even that, maybe not, because it expects to have a midi it can copy... 
            // Let's assume something is loaded.  Clear the tracks, and add one of our own, when we're done

            var notes = tsm.MidiChannels.Values.SelectMany(c => c.tickNotes.Values.Select(nl => nl.Where((n, i) => (i == 0 || i == c.tickNotes.Count - 1 || (c.tickNotes.Count > 2 && i == c.tickNotes.Count / 2)) && n.channel != 9 && n.length > 0 && n.timeLength >= 0.005f))).SelectMany(nl => nl.GroupBy(n => n.note).Select(ng => ng.FirstOrDefault())).OrderBy(n => n.tickNumber).ThenBy(n => n.note).ToArray();
            //var notes = tsm.MidiChannels.Values.SelectMany(s => s.tickNotes.Values).SelectMany(nl => nl).Where(n => n.channel != 9 && n.length > 0 && n.timeLength > 0).OrderBy(n => n.tickNumber).ToArray();

            var percentToSkip = random.NextDouble();
            int startPosition = (int)(notes.Length * percentToSkip);

            if (startPosition + numNotes >= notes.Length) // Note that startPos+10 is actually 11 elements, which is what we want
                startPosition -= numNotes;

            var notesToTrain = notes.Skip(startPosition).Take(numNotes).OrderBy(n => n.tickNumber).ThenBy(n => n.note).ToArray();

            float totalLength = 0;

            float[] inputs = new float[layers[0]];

            float ticksPerSecond = 0;
            for (int i = 0; i < notesToTrain.Length; i++)
            {
                var note = notesToTrain[i];

                // Find out how many ticks are in a second, or rather, a scalar that converts a tick duration to a time one for the current tempo
                ticksPerSecond = note.length / note.timeLength;

                inputs[i * 3] = (note.note - 64) / 64f;
                inputs[i * 3 + 1] = Math.Max(Math.Min((((note.tickNumber - notesToTrain[Math.Max(i - 1, 0)].tickNumber) / ticksPerSecond) - 1f), 1), -1);
                inputs[i * 3 + 2] = Math.Max(Math.Min((Math.Max(note.timeLength, 0.005f) - 1f), 1), -1);
            }
            inputs[inputs.Length - 1] = 0;

            List<MidiNote> resultNotes = new List<MidiNote>();

            var track = new Sanford.Multimedia.Midi.Track();

            int position = 0;

            float targetSecondsLength = 120f;
            // We won't update TicksPerSecond anymore to keep it from screwing with the tempo

            // Well.  It does screw with it tbh.  Maybe we should try it
            // We can only do it after, or with a lastNoteLength

            // So this is probably gonna be weird.  


            // It is putting lots of notes in all the same spots.  I should enforce a notes per tick: 
            // After receiving 3 notes with 0 deltaTicks, we force a position movement equal to the last one's duration

            int maxNotesPerTick = 2;
            int notesThisTick = 0;

            int modulusRounding = 50;

            do
            {
                var result = neural.FeedForward(inputs);

                //int noteNum = GetPentatonicNote((int)Math.Floor(result[0] * 64f + 64));
                int noteNum = (int)Math.Floor(result[0] * 64f + 64);
                var durationTime = Math.Max(Math.Round((result[2] + 1)/1f),0.1f);
                
                int durationTicks = (int)(durationTime * ticksPerSecond);
                //ticksPerSecond = (float)(durationTicks / durationTime);

                var deltaTick = (int)(Math.Max((result[1] + 1), 0) * ticksPerSecond);

                deltaTick -= deltaTick % modulusRounding;

                position += deltaTick;

                if (deltaTick == 0)
                    notesThisTick++;
                if (notesThisTick > maxNotesPerTick)
                {
                    deltaTick = Math.Max(durationTicks + (durationTicks - durationTicks % modulusRounding), modulusRounding);//durationTicks;//
                    position += deltaTick;
                    notesThisTick = 0;
                    totalLength += Math.Max(durationTicks + (durationTicks - durationTicks % modulusRounding), modulusRounding) / ticksPerSecond; //(float)durationTime;
                }
                else if (deltaTick > 0)
                {
                    totalLength += deltaTick / ticksPerSecond;
                    notesThisTick = 0;
                }
                
                //position = Math.Max(0, position - position % 20);// rounding...?

                track.Insert(position, new Sanford.Multimedia.Midi.ChannelMessage(Sanford.Multimedia.Midi.ChannelCommand.NoteOn, 0, noteNum, 100));
                track.Insert(position + durationTicks, new Sanford.Multimedia.Midi.ChannelMessage(Sanford.Multimedia.Midi.ChannelCommand.NoteOff, 0, noteNum));
                //totalLength += (float)durationTime;

                result[0] = (noteNum - 64) / 64f;
                result[1] = Math.Max(Math.Min(((deltaTick / ticksPerSecond) - 1f), 1), -1);
                result[2] = (float)Math.Max(Math.Min((Math.Max(durationTime, 0.005f) - 1f), 1), -1);


                inputs = (inputs.Skip(3).Take(inputs.Length-3-1)
                    .Concat(result)).Select((i,index) => (index-1)%3 == 0 ? Math.Max(i-inputs[1],0) : i)
                    .Concat(new float[] { totalLength/ targetSecondsLength }).ToArray();
            }
            while (totalLength < targetSecondsLength);

            tsm.Player.sequence.Clear();
            tsm.Player.sequence.Add(track);

            if (File.Exists("Testmidi.mid"))
                File.Delete("Testmidi.mid");

            tsm.Player.sequence.Save("Testmidi.mid");

            Console.WriteLine("Done");
        }


        private async Task TrainNetwork(int numPerfect, float percentForSuccess, int parallelism, params int[] sizes)
        {
            try
            {
                int numParamsPerChannel = Extensions.numParamsPerChannel;

                // We can softmax all the outputs once we have them for each channel
                // This is great, btw.  This is 'channelNeural', the only issue is that the way I handle the output gives values <0 and > 100% sometimes, but rarely...

                int numNeurons = sizes.Length + 2; // They don't hand us the input or output layer

                string[] activation = new string[numNeurons - 1]; // And the input layer doesn't have an activation
                for (int n = 0; n < activation.Length; n++) // I decided not to use softmax because it gave more varied values
                    activation[n] = "tanh";
                int[] parameters = new int[numNeurons];
                parameters[0] = numParamsPerChannel;
                for (int n = 1; n < parameters.Length - 1; n++)
                    parameters[n] = sizes[n - 1];
                parameters[parameters.Length - 1] = 1;
                tsm.neural = new NeuralNetwork(parameters, activation);

                // These below work great and are the settings for 'v2Neural'
                //string[] activation = new string[] { "tanh", "softmax" };
                //tsm.neural = new NeuralNetwork(new int[] { 16 * numParamsPerChannel, 64, 16 }, activation);

                // This is no longer necessary but, the iteration over them helps us build the other candidates and this takes basically no time so, oh well
                tsm.simpleML = new SimpleML<MidiChannelItem>(
                    (c => c.avgNoteLength),
                    (c => c.maxChordSize),
                    (c => c.totalNoteLength),
                    (c => c.highestNote),
                    (c => c.lowestNote),
                    (c => c.numNotes),
                    (c => c.Id)
                    );

                var candidates = new List<TrainingCandidate<MidiChannelItem>>();

                var neuralCandidates = new List<MidiChannelItem[]>(); // It can accept multiple correct answers

                foreach (var part in pf.index.PartitionNames)
                {
                    string midiPath = Path.Combine(PartitionsForm.partitionMidiPath, part + ".mid");
                    if (File.Exists(midiPath))
                    {
                        await LuteBotForm.luteBotForm.LoadHelperAsync(midiPath, true);
                        // To prevent it from trying to train on re-generated midis, which have incorrect lengths
                        // Require at least 3 channels
                        if (tsm.MidiChannels.Count() > 2 && tsm.DataDictionary.ContainsKey(1)) // To keep the percentages from getting weird in training, at least 4 channels?
                        {
                            if (tsm.MidiTracks.All(t => t.Value.Active))
                            {
                                // Actually, I think the flute track may not have the appropriate data...
                                var tempNeuralCandidates = tsm.MidiChannels.Values.Where(c => c.Id != 9).ToArray();
                                foreach (var ca in tempNeuralCandidates)
                                {
                                    ca.Active = tsm.DataDictionary[1].MidiChannels.Where(c => c.Id == ca.Id).Single().Active;
                                }
                                neuralCandidates.Add(tempNeuralCandidates);
                                var fluteChannels = tsm.DataDictionary[1].MidiChannels.Where(c => c.Active);
                                if (fluteChannels.Count() == 1)
                                {
                                    var fluteChannel = fluteChannels.First();
                                    // We have what we need, one flute track and more than one total track
                                    // We need to make sure our passed target exactly matches the one in our passed list
                                    var candidate = new TrainingCandidate<MidiChannelItem>(tsm.MidiChannels.Values, fluteChannel);
                                    candidates.Add(candidate);
                                }
                            }
                            else if (tsm.MidiChannels.All(c => c.Value.Active))
                            {
                                // If some tracks are disabled and all channels are enabled, we'll take the tracks as data
                                // Track notes should already have discarded anything with channel 9
                                MidiChannelItem[] tempNeuralCandidates = tsm.MidiTracks.Values.ToArray();
                                foreach (var ca in tempNeuralCandidates)
                                { // This may not be necessary; I could probably just take the MidiTracks from DataDictionary[1] but, it seemed to give odd results like it didn't have everything
                                    ca.Active = tsm.DataDictionary[1].MidiTracks.Where(c => c.Id == ca.Id).Single().Active;
                                }
                                neuralCandidates.Add(tempNeuralCandidates);
                            }
                        }
                    }
                }
                Console.WriteLine($"Training with {candidates.Count} candidates...");
                var trainingTarget = new TrainingTarget<MidiChannelItem>(candidates, ((a, b) => a.Id == b.Id));

                tsm.simpleML.Train(trainingTarget);
                Console.WriteLine("Trained - Final Values:");
                int count = 0;
                foreach (var p in tsm.simpleML.Parameters)
                    Console.WriteLine($"Parameter {count++}: Target={p.Target}; Weight={p.Weight}");

                // Now train the neural one

                // So after 100k trainings, all the channels give a negative answer which is pretty weird
                // And they seem completely wrong or arbitrary, nothing useful.  

                // Probably need to make the network deeper... 
                //int trainCount = 2000;
                float costTotal = 999f;

                //Console.WriteLine($"Training neural network {trainCount} times");

                // Partition candidates into test sets, probably 70/30%?
                // So, first shuffle them

                var random = new Random();

                var orderedCandidates = neuralCandidates.OrderBy(c => random.Next());
                var numTestCandidates = (int)(orderedCandidates.Count() * 0.3f);
                var neuralTrainingCandidates = orderedCandidates.Skip(numTestCandidates);
                var neuralTestCandidates = orderedCandidates.Take(numTestCandidates);

                int numTestsCorrect = 0;
                int numActualTestsCorrect = 0;


#if DEBUG
                TrainMusicMaker(neuralCandidates);
                return;
#endif


                BeginInvoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText($"\n\nTraining neural network until all {numTestCandidates} non-training candidates are correct");
                    progressBarTraining.Maximum = numTestCandidates;
                    progressBarTraining.Value = 0;
                });

                int i = 0;
                int numSuccesses = 0;
                while (numSuccesses < numPerfect)
                //while(costTotal > 0.1f && i < trainCount)
                //for (int i = 0; i < trainCount; i++)
                {
                    costTotal = 0;
                    //foreach (var song in neuralTrainingCandidates)
                    Parallel.ForEach(neuralTrainingCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        float maxTickNumber = song.SelectMany(c => c.tickNotes).SelectMany(kvp => kvp.Value).Max(n => n.tickNumber);

                        foreach (var channel in song)
                        {
                            // So for the 'recurrent memory' implementation, we have memory_count*noteParams*2 inputs; 16 notes, each time we process another note, we push the others up the chain
                            // and pop off the oldest.  Then we have memory_count*noteParams in the final hidden layer, which should be fed back into the last memory_count*noteParams*2 inputs next time

                            // We need an alternate backPropagation that doesn't feedforward, or rather, does all of this before it tries correcting
                            //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);



                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            var expected = new float[1];

                            if (channel.Active)
                                expected[0] = 0.5f;
                            else
                                expected[0] = -0.5f;

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
                            tsm.neural.BackPropagate(inputs, expected);
                            costTotal += tsm.neural.cost;
                        }
                    });
                    numTestsCorrect = 0;
                    numActualTestsCorrect = 0;
                    Parallel.ForEach(neuralTrainingCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        float maxTickNumber = song.SelectMany(c => c.tickNotes).SelectMany(kvp => kvp.Value).Max(n => n.tickNumber);

                        Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                        foreach (var channel in song)
                        {
                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);
                            //var neuralResults = tsm.neural.FeedForwardRecurrent(inputs);
                            var neuralResults = tsm.neural.FeedForward(inputs);
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

                        //await Task.Delay(0); // Let the form live between iterations
                    });
                    Parallel.ForEach(neuralTrainingCandidates.OrderBy(n => random.NextDouble()), new ParallelOptions() { MaxDegreeOfParallelism = parallelism }, song =>
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        float maxTickNumber = song.SelectMany(c => c.tickNotes).SelectMany(kvp => kvp.Value).Max(n => n.tickNumber);

                        Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                        foreach (var channel in song)
                        {
                            //var inputs = channel.GetRecurrentInput(noteParams, maxTickNumber);
                            //var neuralResults = tsm.neural.FeedForwardRecurrent(inputs);
                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            var neuralResults = tsm.neural.FeedForward(inputs);
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
                        var numFlute = song.Where(s => s.Active).Count();
                        bool correct = true;

                        for (int j = 0; j < numFlute; j++)
                        {
                            bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Active;
                            if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                                correct = false;
                        }

                        if (correct)
                        {
                            numTestsCorrect++;
                            numActualTestsCorrect++;
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
                    });
                    BeginInvoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText($"\n{numActualTestsCorrect}/{neuralTestCandidates.Count()} ({(float)numActualTestsCorrect / neuralTestCandidates.Count() * 100}%) tests correct; {percentForSuccess}% {numPerfect} times in a row to finish.  Training Set: {numTestsCorrect}/{neuralCandidates.Count()}\nTraining #{i++} - TotalCost: {costTotal} (This number should go down eventually)");
                        richTextBox1.ScrollToCaret();
                        progressBarTraining.Value = numActualTestsCorrect;
                    });

                    //orderedCandidates = neuralCandidates.OrderBy(c => random.Next());
                    //neuralTrainingCandidates = orderedCandidates.Take(numTrainingCandidates);
                    //neuralTestCandidates = orderedCandidates.Skip(numTrainingCandidates);

                    if ((float)numActualTestsCorrect / neuralTestCandidates.Count() < (percentForSuccess / 100f))
                        numSuccesses = 0;
                    else
                        numSuccesses++;

                    await Task.Delay(1); // Let the form live between iterations
                }

                BeginInvoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText($"\n\n----Training Complete----\n\nNetwork saved to CustomNetwork file to {savePath}\nYou may now load midis and check it for yourself, and this will be your new default network. \n\nYou can revert at any time by deleting this file, or train again to replace it");
                    richTextBox1.ScrollToCaret();
                    textBoxSizes.Enabled = true;
                    buttonTrain.Enabled = true;
                    textBox1.Enabled = true;
                    textBoxParallel.Enabled = true;
                    textBox2.Enabled = true;
                });
                if (File.Exists(savePath))
                    File.Delete(savePath);
                tsm.neural.Save(savePath);
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
                await TestMusicMaker();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
