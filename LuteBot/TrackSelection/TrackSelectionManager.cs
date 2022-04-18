using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;
using LuteBot.UI.Utils;

using Sanford.Multimedia.Midi;

using SimpleML;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class TrackSelectionManager
    {
        private Dictionary<int, MidiChannelItem> midiChannels;
        private Dictionary<int, TrackItem> midiTracks;

        private MidiChannelItem PredictedFluteChannel = null;

        public Dictionary<int, MidiChannelItem> MidiChannels { get => midiChannels; private set { midiChannels = value; ResetRequest(); } }
        public Dictionary<int, TrackItem> MidiTracks { get => midiTracks; private set { midiTracks = value; ResetRequest(); } }
        public bool ActivateAllChannels { get => activateAllChannels; set { activateAllChannels = value; ResetRequest(); } }
        public bool ActivateAllTracks { get => activateAllTracks; set { activateAllTracks = value; ResetRequest(); } }
        public int NoteOffset { get => noteOffset; set { noteOffset = value; ResetRequest(); } }
        public int NumChords { get => numChords; set { numChords = value; ResetRequest(); } }
        public Dictionary<int, TrackSelectionData> DataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();
        public Dictionary<int, TrackSelectionData> OriginalDataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();

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

            /*
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
                foreach (var kvp in data.MidiChannelOffsets)
                {
                    if (MidiChannels.ContainsKey(kvp.Key)) // If not, we don't care. 
                        MidiChannels[kvp.Key].offset = kvp.Value;
                }
            */

            // Just copy the stuff straight over
            this.NoteOffset = data.Offset;
            this.NumChords = data.NumChords;
            this.MidiChannels = data.MidiChannels.ToDictionary(c => c.Id, c => c);
            this.MidiTracks = data.MidiTracks.ToDictionary(c => c.Id, c => c);

            // Restore any channel names that might be null or incorrect, from older data that got saved
            foreach (var c in midiChannels.Values)
                c.Name = Player.GetChannelName(c.Id);

            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            DataDictionary[instrumentId] = new TrackSelectionData(data, instrumentId);
        }


        public void UpdateTrackSelectionForInstrument(int oldInstrument)
        {
            DataDictionary[oldInstrument] = GetTrackSelectionData(oldInstrument);
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (DataDictionary.ContainsKey(instrumentId))
                SetTrackSelectionData(DataDictionary[instrumentId]);
            else if (instrumentId == 3 && DataDictionary.ContainsKey(1))
            {
                // If it's duet flute but there is no data for it, and we also have a flute track, copy the flute settings
                SetTrackSelectionData(new TrackSelectionData(DataDictionary[1], instrumentId));
            }
            else if (instrumentId == 0 && DataDictionary.ContainsKey(1))
            {
                // If it's lute with no data, and there is a flute track, copy the flute track and disable whatever the flute has active
                // I don't think this can ever happen since we populate a default lute immediately...
                SetTrackSelectionData(new TrackSelectionData(DataDictionary[1], instrumentId));
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
                SetTrackSelectionData(new TrackSelectionData(DataDictionary[0], instrumentId));
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
                SetTrackSelectionData(new TrackSelectionData(DataDictionary[0], instrumentId));
        }

        public TrackSelectionData GetTrackSelectionData(int instrumentId)
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = MidiChannels.Values.Select(c => new MidiChannelItem(c)).ToList();
            data.MidiTracks = MidiTracks.Values.Select(c => new TrackItem(c)).ToList();
            data.Offset = NoteOffset;
            data.NumChords = NumChords;
            data.InstrumentID = instrumentId;
            return data;
        }

        public void SaveTrackManager(string filename = null)
        {
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            DataDictionary[instrumentId] = GetTrackSelectionData(instrumentId);
            var simpleDataDictionary = new Dictionary<int, SimpleTrackSelectionData>();

            foreach(var kvp in DataDictionary)
            {
                simpleDataDictionary[kvp.Key] = new SimpleTrackSelectionData(kvp.Value, kvp.Value.InstrumentID, OriginalDataDictionary[kvp.Key]);
            }

            SaveManager.SaveTrackSelectionData(simpleDataDictionary, FileName, filename);
        }

        public void LoadTrackManager()
        {
            var simpleDataDict = SaveManager.LoadTrackSelectionData(FileName);
            if (simpleDataDict != null)
            {
                int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                // The only reason we need simple data is so we can pull out and handle the Notes from within the channels and tracks, if there are any
                DataDictionary.Clear();
                var currentData = GetTrackSelectionData(instrumentId);
                foreach(var kvp in simpleDataDict)
                {
                    DataDictionary[kvp.Key] = new TrackSelectionData(currentData, kvp.Key).WithData(kvp.Value);
                }
                
                TrackSelectionData data = null;
                
                if (DataDictionary.ContainsKey(instrumentId))
                    data = DataDictionary[instrumentId];
                else if (DataDictionary.ContainsKey(0))
                    data = new TrackSelectionData(DataDictionary[0], instrumentId);

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
            // Fix up the instrument IDs in case we didn't get them before... unsure why they're included at all tbh but, future compatibility I guess
            //foreach (var kvp in DataDictionary)
            //    kvp.Value.InstrumentID = kvp.Key;
        }

        public void LoadTracks(Dictionary<int, MidiChannelItem> channels, Dictionary<int, TrackItem> tracks)
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

            NoteOffset = 0;
            NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);

            // Before we screw with it by predicting flutes, we should store this current TrackSelectionData into our OriginalDataDictionary in every feasible slot
            OriginalDataDictionary.Clear();
            for (int i = 0; i < 16; i++)
            {
                var data = GetTrackSelectionData(i);
                data.InstrumentID = i;
                OriginalDataDictionary[i] = data;
            }

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
            else // if(simpleML != null && simpleML.Trained)
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
                for (int i = 0; i < orderedResults.Count(); i++)
                //foreach (var channel in activeChannels)
                {
                    //var channel = activeChannels.Where(c => c.Id == orderedResults.ElementAt(i).Key).SingleOrDefault();
                    var channel = orderedResults.ElementAt(i).Key;
                    if (channel != null)
                    {
                        string ident = "Channel";
                        if (channel is TrackItem)
                            ident = "Track";
                        Console.WriteLine($"{ident} {channel.Name} ({channel.Id}) - Neural Score: {channelResults[channel]}");
                        //Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {neuralResults[channel.Id]}");
                        if (channel is TrackItem)
                            channel.Name += $"(Flute Rank {++trackCount} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                        else
                            channel.Name += $"(Flute Rank {++count} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                    }
                }

                return orderedResults.First().Key;
            }

            return null;
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


        Regex savStartRegex = new Regex(@"\|\w*;[0-9]+\|");
        public void LoadSavFiles(string directory, string songname)
        {
            try
            {
                // Sav files have this format:

                //|name;tempo|{track 0}|{track 1}|

                // Where each track, is a list of notes, with the format:

                // tickNumber-note-NoteType

                // NoteType 1 is a NoteOn, 2 is a tempo event, Off is 0 - these are LuteMod.Sequencing.NoteType 


                // Annoyingly, at EOF we have some chars that may or may not be particular:     None    
                // Many of those are invis and won't paste right...
                // ENQ is 05... NUL is supposed to be 00
                // It's: NULENQNULNULNULNoneNULNULNULNUL
                // So tldr, it's NUL
                var eofDelim = (char)0;
                // There are of course, NULs before that too.  

                // OK, it's easy, here's what we do
                // We start with the 0 file, and find our signature, |name;tempo|.  Then go backwards until we find SavedPartitions, and store that as the start delim for this song
                // Then we iterate all of the files, cutting out after our start delim, to the next NUL, and appending to one long string
                // Then we easily parse that string

                string startDelim;
                var firstContent = File.ReadAllText(Path.Combine(directory, songname + "[0].sav"));
                Match firstMatch = savStartRegex.Match(firstContent);
                if (!firstMatch.Success)
                    throw new Exception("Sav file was in unrecognized format");
                var delimEndIndex = firstMatch.Index;

                // Remember that substring's second param is a length
                int startDelimStartIndex = firstContent.IndexOf("StrProperty"); // 'SavdPartition' occurs multiple times before we actually want it
                startDelim = firstContent.Substring(startDelimStartIndex, delimEndIndex - startDelimStartIndex);

                StringBuilder contentBuilder = new StringBuilder();

                var bottomContent = firstContent.Substring(delimEndIndex);
                contentBuilder.Append(bottomContent.Substring(0, bottomContent.IndexOf(eofDelim)));

                int i = 1;
                while (File.Exists(Path.Combine(directory, $"{songname}[{i}].sav")))
                {
                    firstContent = File.ReadAllText(Path.Combine(directory, $"{songname}[{i}].sav"));
                    delimEndIndex = firstContent.IndexOf(startDelim) + startDelim.Length;
                    bottomContent = firstContent.Substring(delimEndIndex);
                    int captureLength = bottomContent.IndexOf(eofDelim);
                    int atSymbol = bottomContent.IndexOf("@");
                    if (atSymbol > -1 && (atSymbol < captureLength || captureLength == -1))
                        captureLength = atSymbol;
                    if (captureLength == -1)
                        captureLength = bottomContent.Length;
                    contentBuilder.Append(bottomContent.Substring(0, captureLength));
                    i++;
                }

                string fullContent = contentBuilder.ToString();
                // Beautiful

                var trackSplit = fullContent.Split('|');
                // Tempo is in 1, the tracks are then each in 2+, 0 is blank or stuff we don't want if it isn't
                // The last one is also empty
                // I guess we'll build these into MidiChannelItems, then into a TrackSelectionData, and load it

                // The problem is, many things rely on the player and its tempo, which we have as our first bit of data
                // But it's probably not a good idea to brute-force... better if we can make it into a midi and actually load it on the player

                // I guess we can make a new sequencer maybe?
                var sequence = new Sequence(120) { Format = 1 };
                int tempo = int.Parse(trackSplit[1].Split(';')[1]) / 120;
                sequence.FirstTempo = tempo;

                var firstTempoBuilder = new TempoChangeBuilder();
                firstTempoBuilder.Tempo = tempo;
                firstTempoBuilder.Build();

                trackSplit = trackSplit.Skip(2).Take(trackSplit.Length - 3).ToArray();

                var metaTrack = new Track();
                metaTrack.Insert(0, firstTempoBuilder.Result);
                sequence.Add(metaTrack);

                //var tsb = new TimeSignatureBuilder();
                //tsb.Numerator = 4;
                //tsb.Denominator = 4;
                //tsb.Build();
                //metaTrack.Insert(0, tsb.Result);
                //
                //var ksb = new KeySignatureBuilder();
                //ksb.Key = Sanford.Multimedia.Key.CMajor;
                //ksb.Build();
                //metaTrack.Insert(0, ksb.Result);

                int trackNum = 1;
                int maxTick = 0;
                foreach (var trackString in trackSplit)
                {
                    var track = new Track();
                    sequence.Channels.Add(trackNum);

                    var noteSplit = trackString.Split(';');

                    int lastTick = 0;

                    foreach (var noteString in noteSplit)
                    {


                        var noteValueSplit = noteString.Split('-');
                        // 0 is the tick number, 1 is the value, 2 is the type

                        int noteTrackNum = trackNum - 1;
                        if (trackNum > 1)
                            noteTrackNum = 0;

                        IMidiMessage message;
                        lastTick = int.Parse(noteValueSplit[0]);
                        if (noteValueSplit[2] == "1")
                        {
                            message = new ChannelMessage(ChannelCommand.NoteOn, trackNum, int.Parse(noteValueSplit[1]) + Instrument.Prefabs[noteTrackNum].LowestPlayedNote, 100);
                            track.Insert(lastTick, message);
                            lastTick += 10; // IDK some arbitrary length
                            track.Insert(lastTick, new ChannelMessage(ChannelCommand.NoteOff, trackNum, int.Parse(noteValueSplit[1]) + Instrument.Prefabs[noteTrackNum].LowestPlayedNote));
                        }
                        else // This should be 7 bits per byte, little endian
                        { // Each byte is high except the last one...?
                            var tcb = new TempoChangeBuilder();
                            tcb.Tempo = int.Parse(noteValueSplit[1]) / 120;
                            tcb.Build();
                            message = tcb.Result;
                            // Well this is awkward.  The tempo we read is divided by the division
                            // But we don't know what the division is/was
                            metaTrack.Insert(lastTick, message);
                        }

                        if (lastTick > maxTick)
                            maxTick = lastTick;
                    }

                    track.Insert(lastTick + 100, new MetaMessage(MetaType.EndOfTrack, new byte[0]));

                    string trackName = null;
                    int instrumentId = 0;
                    if (trackNum == 1)
                        trackName = "Lute";
                    else if (trackNum == 2)
                    {
                        trackName = "Flute";
                        instrumentId = 73;
                    }
                    if (trackName != null)
                    {
                        track.Insert(0, new MetaMessage(MetaType.TrackName, Encoding.ASCII.GetBytes(trackName)));
                    }
                    if (instrumentId > 0)
                        track.Insert(0, new ChannelMessage(ChannelCommand.ProgramChange, trackNum, instrumentId));
                    
                    sequence.Add(track);
                    //sequence.Tracks.Add(track);
                    

                    trackNum++;
                }
                metaTrack.Insert(maxTick + 100, new MetaMessage(MetaType.EndOfTrack, new byte[0]));
                // This is fucking annoying
                // There's no way to get the division back out, and I can't set it even if I could
                // Ahhh, we can set it in the constructor... 
                // What if it's just, 1?  

                string midiPath = Path.Combine(PartitionsForm.partitionMidiPath, songname + ".mid");
                sequence.Save(midiPath);
                
                LuteBotForm.luteBotForm.LoadHelper(midiPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
