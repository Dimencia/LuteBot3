using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;

using Sanford.Multimedia.Midi;

using SimpleML;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class TrackSelectionManager
    {
        private Dictionary<int, MidiChannelItem> midiChannels;
        private Dictionary<int, TrackItem> midiTracks;

        private MidiChannelItem PredictedFluteChannel = null;

        public Dictionary<int, MidiChannelItem> MidiChannels { get => midiChannels; set { midiChannels = value; ResetRequest(); } }
        public Dictionary<int, TrackItem> MidiTracks { get => midiTracks; set { midiTracks = value; ResetRequest(); } }
        public bool ActivateAllChannels { get => activateAllChannels; set { activateAllChannels = value; ResetRequest(); } }
        public bool ActivateAllTracks { get => activateAllTracks; set { activateAllTracks = value; ResetRequest(); } }
        public int NoteOffset { get => noteOffset; set { noteOffset = value; ResetRequest(); } }
        public int NumChords { get => numChords; set { numChords = value; ResetRequest(); } }
        public Dictionary<int, TrackSelectionData> DataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();

        private int numChords;
        private int noteOffset;
        private bool activateAllChannels;
        private bool activateAllTracks;

        public bool autoLoadProfile;
        public string FileName { get; set; }

        public event EventHandler TrackChanged;
        public event EventHandler<TrackItem> ToggleTrackRequest;
        public event EventHandler OutDeviceResetRequest;

        public MidiPlayer Player { get; set; }

        public TrackSelectionManager()
        {
            midiChannels = new Dictionary<int, MidiChannelItem>();
            midiTracks = new Dictionary<int, TrackItem>();
            activateAllChannels = false;
            activateAllTracks = false;
            autoLoadProfile = true;
            numChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            UpdateTrackSelectionForInstrument(0); // Saves defaults, and also updates us in case lute wasn't the one selected
        }

        public void ToggleChannel(int index, bool active)
        {
            if (index >= 0 && index < midiChannels.Count)
            {
                midiChannels[index].Active = active;
                ResetRequest();
            }
        }

        public void SetTrackSelectionData(TrackSelectionData data)
        {
            // So... Channels and tracks might have changed
            // I think we need to compare current channels and remove any values that aren't in them before storing
            //Dictionary<int, int> newMidiOffsets = new Dictionary<int, int>();
            //foreach (var channel in MidiChannels)
            //{
            //    var newChannel = data.MidiChannels.Where(c => c.Id == channel.Id).FirstOrDefault();
            //    if (newChannel != null)
            //    {
            //        channel.Active = newChannel.Active;
            //        if (data.MidiChannelOffsets.ContainsKey(channel.Id))
            //            newMidiOffsets.Add(channel.Id, data.MidiChannelOffsets[channel.Id]);
            //    }
            //}
            //foreach (var track in MidiTracks)
            //{
            //    var newTrack = data.MidiTracks.Where(t => t.Id == track.Id).FirstOrDefault();
            //    if (newTrack != null)
            //        track.Active = newTrack.Active;
            //}
            //
            //NoteOffset = data.Offset;
            //NumChords = data.NumChords;

            // Aha.  So, we don't want to overwrite because old ones might not have some data, and we already have populated data
            // We also need new objects so we don't have refernce issues.  So start by duplicating all existing ones
            foreach (var c in data.MidiChannels)
            {
                if (MidiChannels.ContainsKey(c.Id))
                {
                    MidiChannels[c.Id] = new MidiChannelItem(MidiChannels[c.Id]);
                    MidiChannels[c.Id].Active = c.Active;
                    MidiChannels[c.Id].offset = c.offset;
                }
                // If we don't have it, we discarded it cuz it was dumb, so don't do anything.  
            }
            foreach (var t in data.MidiTracks)
            {
                if (MidiTracks.ContainsKey(t.Id))
                {
                    MidiTracks[t.Id] = new TrackItem(MidiTracks[t.Id]);
                    MidiTracks[t.Id].Active = t.Active;
                }
            }

            //this.MidiChannels = data.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel)).ToDictionary(channel => channel.Id);
            //this.MidiTracks = data.MidiTracks.ConvertAll(track => new TrackItem(track)).ToDictionary(track => track.Id);
            this.NoteOffset = data.Offset;
            this.NumChords = data.NumChords;

            if (data.MidiChannelOffsets != null)
                foreach(var kvp in data.MidiChannelOffsets)
                {
                    if (MidiChannels.ContainsKey(kvp.Key)) // If not, we don't care. 
                        MidiChannels[kvp.Key].offset = kvp.Value;
                }

            // Restore any channel names that might be null or incorrect, from older data that got saved
            foreach (var c in midiChannels.Values)
                c.Name = Player.GetChannelName(c.Id);

            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            DataDictionary[instrumentId] = new TrackSelectionData(data, instrumentId);
        }


        public void UpdateTrackSelectionForInstrument(int oldInstrument)
        {
            DataDictionary[oldInstrument] = GetTrackSelectionData();
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (DataDictionary.ContainsKey(instrumentId))
                SetTrackSelectionData(DataDictionary[instrumentId]);
            else if (instrumentId == 3 && DataDictionary.ContainsKey(1))
            {
                // If it's duet flute but there is no data for it, and we also have a flute track, copy the flute settings
                SetTrackSelectionData(DataDictionary[1]);
            }
            else if (instrumentId == 0 && DataDictionary.ContainsKey(1))
            {
                // If it's lute with no data, and there is a flute track, copy the flute track and disable whatever the flute has active
                SetTrackSelectionData(DataDictionary[1]);
                var fluteData = DataDictionary[1];

                foreach (var channel in MidiChannels.Values)
                {
                    // There's no real situation where they should have any disparity between their channels
                    if (fluteData.MidiChannels.Where(d => d.Id == channel.Id).Single().Active)
                        channel.Active = false;
                }
            }
            else if (instrumentId == 1 && DataDictionary.ContainsKey(0))
            {
                // Load default from lute...
                SetTrackSelectionData(DataDictionary[0]);
                if (PredictedFluteChannel != null)
                {
                    if (PredictedFluteChannel is TrackItem)
                    {
                        foreach (var channel in midiTracks.Values)
                        {
                            if (channel.Id != PredictedFluteChannel.Id)
                                channel.Active = false;
                            else
                            {
                                //channel.Name += " (Flute?)";
                                channel.Active = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var channel in midiChannels.Values)
                        {
                            if (channel.Id != PredictedFluteChannel.Id)
                                channel.Active = false;
                            else
                            {
                                //channel.Name += " (Flute?)";
                                channel.Active = true;
                            }
                        }
                    }
                }
            }
            else if (DataDictionary.ContainsKey(0))
                SetTrackSelectionData(DataDictionary[0]);
        }

        public TrackSelectionData GetTrackSelectionData()
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = new List<MidiChannelItem>(MidiChannels.Values);
            data.MidiTracks = new List<TrackItem>(MidiTracks.Values);
            data.Offset = NoteOffset;
            data.NumChords = NumChords;
            return data;
        }

        public void SaveTrackManager(string filename = null)
        {
            DataDictionary[ConfigManager.GetIntegerProperty(PropertyItem.Instrument)] = GetTrackSelectionData();
            SaveManager.SaveTrackSelectionData(DataDictionary, FileName, filename);
        }

        public void LoadTrackManager()
        {
            var dataDictionary = SaveManager.LoadTrackSelectionData(FileName);
            if (dataDictionary != null)
            {
                DataDictionary = dataDictionary;
                TrackSelectionData data = null;
                int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                if (DataDictionary.ContainsKey(instrumentId))
                    data = DataDictionary[instrumentId];
                else if (DataDictionary.ContainsKey(0))
                    data = DataDictionary[0];

                if (data != null)
                {
                    SetTrackSelectionData(data);
                }
                else
                { // Reset these if there's no settings for something
                    DataDictionary.Clear();
                    this.NoteOffset = 0;
                    //this.midiChannels.Clear();
                    //this.midiTracks.Clear();
                    this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);

                    // There was no saved data... So, for lute, disable the PredictedFluteChannel, and enable it for flute
                    if (PredictedFluteChannel != null)
                    {
                        if (PredictedFluteChannel is TrackItem)
                        {
                            midiTracks[PredictedFluteChannel.Id].Active = false;
                            if (!midiTracks[PredictedFluteChannel.Id].Name.Contains("Shawm"))
                                midiTracks[PredictedFluteChannel.Id].Name += " (Shawm?)";
                        }
                        else
                        {
                            midiChannels[PredictedFluteChannel.Id].Active = false;
                            if (!midiChannels[PredictedFluteChannel.Id].Name.Contains("Shawm"))
                                midiChannels[PredictedFluteChannel.Id].Name += " (Shawm?)";
                        }
                    }

                    UpdateTrackSelectionForInstrument(0); // Force the settings into both instrument 0 and the current one

                    // Setup the flute and make that happen too
                    if (PredictedFluteChannel != null)
                    {
                        if (PredictedFluteChannel is TrackItem)
                        {
                            foreach (var channel in midiTracks.Values)
                            {
                                if (channel.Id != PredictedFluteChannel.Id)
                                    channel.Active = false;
                                else
                                {
                                    //channel.Name += " (Flute?)";
                                    channel.Active = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var channel in midiChannels.Values)
                            {
                                if (channel.Id != PredictedFluteChannel.Id)
                                    channel.Active = false;
                                else
                                {
                                    //channel.Name += " (Flute?)";
                                    channel.Active = true;
                                }
                            }
                        }
                    }
                    UpdateTrackSelectionForInstrument(1);
                }
                EventHelper();
            }
            else
            {
                DataDictionary.Clear();
                this.NoteOffset = 0;
                //this.midiChannels.Clear();
                //this.midiTracks.Clear();
                this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);

                // There was no saved data... So, for lute, disable the PredictedFluteChannel, and enable it for flute
                if (PredictedFluteChannel != null)
                {
                    if (PredictedFluteChannel is TrackItem)
                    {
                        midiTracks[PredictedFluteChannel.Id].Active = false;
                        if (!midiTracks[PredictedFluteChannel.Id].Name.Contains("Shawm"))
                            midiTracks[PredictedFluteChannel.Id].Name += " (Shawm?)";
                    }
                    else
                    {
                        midiChannels[PredictedFluteChannel.Id].Active = false;
                        if (!midiChannels[PredictedFluteChannel.Id].Name.Contains("Shawm"))
                            midiChannels[PredictedFluteChannel.Id].Name += " (Shawm?)";
                    }
                }

                UpdateTrackSelectionForInstrument(0); // Force the settings into both instrument 0 and the current one

                // Setup the flute and make that happen too
                if (PredictedFluteChannel != null)
                {
                    if (PredictedFluteChannel is TrackItem)
                    {
                        foreach (var channel in midiTracks.Values)
                        {
                            if (channel.Id != PredictedFluteChannel.Id)
                                channel.Active = false;
                            else
                            {
                                //channel.Name += " (Flute?)";
                                channel.Active = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var channel in midiChannels.Values)
                        {
                            if (channel.Id != PredictedFluteChannel.Id)
                                channel.Active = false;
                            else
                            {
                                //channel.Name += " (Flute?)";
                                channel.Active = true;
                            }
                        }
                    }
                }
                UpdateTrackSelectionForInstrument(1);
                EventHelper();
            }
        }

        public void LoadTracks(Dictionary<int, MidiChannelItem> channels, Dictionary<int, TrackItem> tracks, TrackSelectionManager tsm)
        {
            //midiChannels.Clear();
            //midiTracks.Clear();
            //
            //foreach (var kvp in channels)
            //    if(kvp.Key != 9)
            //        midiChannels.Add(new MidiChannelItem() { Id = kvp.Key, Active = true, Name = kvp.Value });
            //    else // Automatically disable glockenspiel channel
            //        midiChannels.Add(new MidiChannelItem() { Id = kvp.Key, Active = false, Name = kvp.Value });
            //foreach (var kvp in tracks)
            //    midiTracks.Add(new TrackItem() { Id = kvp.Key, Name = kvp.Value, Active = true });

            // We don't want to directly copy it... I think we already have some at this point?
            // We do not, yet... 
            midiChannels = channels;
            midiTracks = tracks;

            NoteOffset = tsm.NoteOffset;
            NumChords = tsm.NumChords;

            // Now is a good time to predict a flute channel - PredictedFluteChannel
            PredictedFluteChannel = GetFlutePrediction();

            ResetRequest();
            //EventHelper();
        }


        public SimpleML<MidiChannelItem> simpleML = null;
        public NeuralNetwork neural = null;

        // Returns the channel ID of the channel most likely to be good for flute
        private MidiChannelItem GetFlutePrediction()
        {
            var activeChannels = midiChannels.Values;//.Where(c => c.Active); // Wait ... how...?  
            // I don't understand how anything was even getting labeled if we were using this

            if (neural == null)
            {
                //string[] activation = new string[] { "tanh", "leakyrelu", "softmax" }; // TODO make this an enum, what kind of madman made them strings... 
                //neural = new NeuralNetwork(new int[] { 96, 96, 48, 16 }, activation);
                // neural.Load("TestML");

                //string[] activation = new string[] { "tanh", "softmax" };
                //
                //int numParamsPerChannel = 8;
                //neural = new NeuralNetwork(new int[] { 16 * numParamsPerChannel, 64, 16 }, activation);
                //
                //neural.Load("v2Weights");

                string[] activation = new string[] { "tanh", "tanh", "tanh" };
                neural = new NeuralNetwork(new int[] { Extensions.numParamsPerChannel, 64, 32, 1 }, activation);
                neural.Load("channelNeural");
            }

            if (activeChannels.Count() < 2)
                return null;
            else// if(simpleML.Trained)
            {
                Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();

                float maxAvgNoteLength = activeChannels.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                float maxNoteLength = activeChannels.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                float maxNumNotes = activeChannels.Max(c => c.Id == 9 ? 0 : c.numNotes);

                foreach (var channel in activeChannels)
                {
                    var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                    var neuralResults = neural.FeedForward(inputs);
                    channelResults[channel] = neuralResults[0];
                }
                foreach (var channel in MidiTracks.Values)
                {
                    var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                    var neuralResults = neural.FeedForward(inputs);
                    channelResults[channel] = neuralResults[0]; // The tracks are MidiChannels and can work in this way; later we just check if they are a MidiTrack
                }

                var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);
                // Get softmaxed values... maybe? No.  We don't want that, in a large song that leaves many at low percent
                // We just want to scale the value between either -1 and 1, or -0.5 and 0.5
                // Just adding 0.5 gets us from 0 to 1 for most purposes, let's try that
                // It's funny when they're above 100% or below 0%, but messy
                // 
                orderedResults = orderedResults.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value + 0.5f)).OrderByDescending(kvp => kvp.Value);


                /*
                float[] inputs = activeChannels.ToArray().GetNeuralInput();

                for (int j = 0; j < 16; j++)
                {
                    var channel = activeChannels.Where(c => c.Id == j && c.Id != 9).SingleOrDefault();
                    //var channel = song.Values[j];

                }

                var neuralResults = neural.FeedForward(inputs);
                // The output here is a channel ID to confidence map...
                // I need to find the ID of the best one... and rank the rest...
                // So let's just build a quick dictionary I guess
                var results = new Dictionary<int, float>();

                for(int i = 0; i < neuralResults.Length; i++)
                {
                    results[i] = neuralResults[i];
                }

                var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                */

                int count = 0; // Separately track count for rank... 
                int trackCount = 0;
                for(int i = 0; i < orderedResults.Count(); i++)
                //foreach (var channel in activeChannels)
                {
                    //var channel = activeChannels.Where(c => c.Id == orderedResults.ElementAt(i).Key).SingleOrDefault();
                    var channel = orderedResults.ElementAt(i).Key;
                    if (channel != null) {
                        string ident = "Channel";
                        if (channel is TrackItem)
                            ident = "Track";
                        Console.WriteLine($"{ident} {channel.Name} ({channel.Id}) - Neural Score: {channelResults[channel]}");
                        //Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {neuralResults[channel.Id]}");
                        if (channel is TrackItem)
                            channel.Name += $"(Flute Rank {++trackCount} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                        else
                            channel.Name += $"(Flute Rank {++count} - {Math.Round(orderedResults.ElementAt(i).Value * 100,2)}%)";
                    }
                }

                return orderedResults.First().Key;
            }
            
            //return -1;
            /*
            if (simpleML != null && simpleML.Trained)
            {
                var results = simpleML.GetWeights(activeChannels.ToArray()).OrderBy(r => r.Weight).ToArray(); // Lower values are better 


                
            }
            else
            {
                return -1;
                // A good flute track has long notes, many notes, and the notes cover a wide octave area
                // So let's get all of those on a scale of 0 to 1
                // That means finding the highest and lowest avgNoteLength, numNotes, and highest-lowest

                // If there is only 1 channel, don't mark it, so that it plays on both lute and flute
                

                var lowestAvgLength = int.MaxValue;
                var highestAvgLength = 0;
                var lowestNumNotes = int.MaxValue;
                var highestNumNotes = 0;
                var lowestRange = int.MaxValue;
                var highestRange = 0;

                // First, parse channels for the highest and lowest values of: average note length, total number of notes, range of notes 

                foreach (var c in activeChannels)
                {
                    if (c.avgNoteLength < lowestAvgLength)
                        lowestAvgLength = c.avgNoteLength;
                    if (c.avgNoteLength > highestAvgLength)
                        highestAvgLength = c.avgNoteLength;

                    if (c.numNotes < lowestNumNotes)
                        lowestNumNotes = c.numNotes;
                    if (c.numNotes > highestNumNotes)
                        highestNumNotes = c.numNotes;

                    var range = c.highestNote - c.lowestNote;
                    if (range < lowestRange)
                        lowestRange = range;
                    if (range > highestRange)
                        highestRange = range;
                }

                int lengthRange = highestAvgLength - lowestAvgLength;
                int numNoteRange = highestNumNotes - lowestNumNotes;
                int rangeRange = highestRange - lowestRange;

                // Also for each channel, store the total number of ticks covered by all notes in the channel (i.e. total duration that notes are actually playing, later compared to song duration)

                // And then apply some weights and targets; the weights are all negative because the ideal track gets a 0 on the percent differences
                // Targets and weights set by some manual testing... 
                float lengthWeight = -1.5f;
                float targetALen = 0.2f; // Vocals aren't the longest, but not the shortest... 

                float rangeWeight = -1f;
                float targetRange = 0.5f; // They cover a very average amount of range

                float numNotesWeight = -0.5f;
                float targetNumNotes = 0.2f; // And usually only have like 20% of total notes for a song

                float targetLengthPercent = 0.6f; // Vocals are about half of the song usually...
                float targetLengthWeight = -2f; // At least 2 weight, because we're looking at values from 0 to 0.5, probably more than that

                float numChordsWeight = -0.25f; // Prefer fewer chords... 

                float targetMaxOctave = 84; // Prefer notes closer to our flute max
                float octaveWeight = -3;

                var totalTrackLength = Player.GetLength();

                var channels = activeChannels.OrderByDescending(c =>
                {
                    float weight = 0;

                    if (lengthRange > 0)
                        weight += Math.Abs(((c.avgNoteLength - lowestAvgLength) / (float)lengthRange) - targetALen) * lengthWeight;

                    if (numNoteRange > 0)
                        weight += Math.Abs(((c.numNotes - lowestNumNotes) / (float)numNoteRange) - targetNumNotes) * numNotesWeight;

                    if (rangeRange > 0)
                        weight += Math.Abs((((c.highestNote - c.lowestNote) - lowestRange) / (float)rangeRange) - targetRange) * rangeWeight;

                    weight += c.maxChordSize * numChordsWeight;

                    weight += Math.Abs((c.highestNote - targetMaxOctave) / 64) * octaveWeight; // This /64 is a bit arbitrary, but should ensure values stay below 1 like everything else
                                                                                               // They can't be more than 84 away, and assumedly they'll have a max note of at least a C1 (C-1 is at 0 in our implementation)

                var lengthPercent = c.totalNoteLength / (float)totalTrackLength;
                    var targetLengthDiff = Math.Abs(lengthPercent - targetLengthPercent);

                    weight += targetLengthDiff * targetLengthWeight;

                    return weight;
                });

                // Let's adjust their names with their scores
                // They tend to be like... up to -5?  I should really sum all the weights and find out but
                // Good enough otherwise
                for (int i = 0; i < channels.Count(); i++)
                    channels.ElementAt(i).Name += " (Flute Rank: " + (i + 1) + ")";

                var channel = channels.FirstOrDefault();
                if (channel != null)
                    return channel.Id;
                return -1;
            }
            */
        }

        public ChannelMessage FilterMidiEvent(ChannelMessage message, int trackId)
        {
            ChannelMessage newMessage = message;
            if (midiTracks.ContainsKey(trackId))
            {
                TrackItem track = midiTracks[trackId];
                if (track != null && track.Active)
                {
                    if (message.Command == ChannelCommand.NoteOn)
                    {
                        if (midiChannels.ContainsKey(message.MidiChannel))
                        {
                            var channelItem = midiChannels[message.MidiChannel];
                            if (!(channelItem.Active || activateAllChannels))
                            {
                                newMessage = new ChannelMessage(ChannelCommand.NoteOn, message.MidiChannel, message.Data1, 0);
                            }
                        }
                    }
                }
                else
                {
                    newMessage = new ChannelMessage(ChannelCommand.NoteOn, message.MidiChannel, message.Data1, 0);
                }
            }
            return newMessage;
        }

        public void UnloadTracks()
        {
            midiChannels.Clear();
            midiTracks.Clear();
            NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            //EventHelper();
        }

        public void ToggleTrackActivation(bool active, int index)
        {
            if (index >= 0 && index < midiTracks.Count)
            {
                midiTracks[index].Active = active;
                ToggleTrackRequestHelper(midiTracks[index]);
            }
        }
        public void ToggleChannelActivation(bool active, int index)
        {
            if (midiChannels.ContainsKey(index))
            {
                MidiChannels[index].Active = active;
                ResetRequest();
            }
        }

        public void ResetRequest()
        {
            EventHandler handler = OutDeviceResetRequest;
            handler?.Invoke(this, new EventArgs());
        }

        private void ToggleTrackRequestHelper(TrackItem item)
        {
            EventHandler<TrackItem> handler = ToggleTrackRequest;
            handler?.Invoke(this, item);
        }

        private void EventHelper()
        {
            EventHandler handler = TrackChanged;
            handler?.Invoke(this, new EventArgs());
        }
    }
}
