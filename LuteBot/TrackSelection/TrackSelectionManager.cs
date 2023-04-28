using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;
using LuteBot.UI;
using LuteBot.UI.Utils;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Sanford.Multimedia.Midi;

using SimpleML;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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

        public Dictionary<int, MidiChannelItem> MidiChannels { get => midiChannels; private set { midiChannels = value;  } }
        public Dictionary<int, TrackItem> MidiTracks { get => midiTracks; private set { midiTracks = value;  } }
        public bool ActivateAllChannels { get => activateAllChannels; set { activateAllChannels = value; } }
        public bool ActivateAllTracks { get => activateAllTracks; set { activateAllTracks = value; } }
        public int NoteOffset { get => noteOffset; set { noteOffset = value; } }
        public int NumChords { get => numChords; set { numChords = value; } }
        public Dictionary<int, TrackSelectionData> DataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();
        public Dictionary<int, TrackSelectionData> OriginalDataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();

        private int numChords;
        private int noteOffset;
        private bool activateAllChannels;
        private bool activateAllTracks;

        public bool autoLoadProfile = true;
        public string FileName { get; set; }

        public event EventHandler TrackChanged;
        public event EventHandler<MidiChannelItem> ToggleTrackRequest;
        public event EventHandler OutDeviceResetRequest;

        public bool autoEnableFlutes = false;
        public MidiPlayer Player { get; set; }

        private NeuralNetwork neural = null;

        public TrackSelectionManager()
        {
            midiChannels = new Dictionary<int, MidiChannelItem>();
            midiTracks = new Dictionary<int, TrackItem>();
            activateAllChannels = false;
            activateAllTracks = false;
            autoLoadProfile = true;
            numChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            Player = new MidiPlayer(this);
            UpdateTrackSelectionForInstrument(0); // Saves defaults, and also updates us in case lute wasn't the one selected
        }

        public void ToggleChannel(int index, bool active)
        {
            if (index >= 0 && index < midiChannels.Count)
            {
                midiChannels[index].Active = active;
            }
        }

        public static NeuralNetwork SetupNeuralNetwork(int[] sizes = null)
        {
            NeuralNetwork neural;
            bool customFile = false;
            if (sizes == null && File.Exists(NeuralNetworkForm.savePath) && File.Exists(NeuralNetworkForm.savePath + ".config"))
            {
                sizes = JsonConvert.DeserializeObject<int[]>(File.ReadAllText(NeuralNetworkForm.savePath + ".config"));
                customFile = true;
            }
            if (sizes != null)
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
                neural = new NeuralNetwork(parameters, activation);
                if (customFile)
                    neural.Load(NeuralNetworkForm.savePath);
            }
            else
            {
                string[] activation = new string[] { "tanh", "tanh", "tanh" };
                neural = new NeuralNetwork(new int[] { Extensions.numParamsPerChannel, 64, 32, 1 }, activation);
                neural.Load(Path.Combine(AppContext.BaseDirectory, "lib", "channelNeural"));
            }
            return neural;
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


        public void UpdateTrackSelectionForInstrument(int oldInstrument, bool reorderTracks = false, int? newInstrument = null)
        {
            DataDictionary[oldInstrument] = GetTrackSelectionData(oldInstrument);
            int instrumentId = newInstrument ?? ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
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

                foreach (var channel in MidiChannels.Values.OrderBy(t => t.Rank))
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

        public async Task SaveTrackManager(string filename = null)
        {
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            DataDictionary[instrumentId] = GetTrackSelectionData(instrumentId);
            var simpleDataDictionary = new Dictionary<int, SimpleTrackSelectionData>();

            foreach (var kvp in DataDictionary)
            {
                simpleDataDictionary[kvp.Key] = new SimpleTrackSelectionData(kvp.Value, kvp.Value.InstrumentID, OriginalDataDictionary[kvp.Key]);
            }

            await SaveManager.SaveTrackSelectionData(simpleDataDictionary, FileName, filename).ConfigureAwait(false);
        }

        public void LoadTrackManager(bool reorderTracks = false)
        {
            // Meant to be called after LoadTracks
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            var simpleDataDict = SaveManager.LoadTrackSelectionData(FileName);
            if (simpleDataDict != null)
            {
                
                // The only reason we need simple data is so we can pull out and handle the Notes from within the channels and tracks, if there are any
                //DataDictionary.Clear();
                var currentData = GetTrackSelectionData(instrumentId);
                foreach (var kvp in simpleDataDict)
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
                    PredictedFluteChannel = GetFlutePrediction(reorderTracks);
                    SetTrackSelectionData(data);
                }
                else
                { // Reset these if there's no settings for something
                    //DataDictionary.Clear();
                    this.NoteOffset = 0;
                    //this.midiChannels.Clear();
                    //this.midiTracks.Clear();
                    this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
                    // There was no saved data... So, for lute, disable the PredictedFluteChannel, and enable it for flute
                    autoEnableFlutes = true;
                    PredictedFluteChannel = GetFlutePrediction(true);
                    autoEnableFlutes = false;

                    UpdateTrackSelectionForInstrument(instrumentId);
                    foreach (var t in midiTracks.Values)
                        t.Active = !t.Active;
                    if (instrumentId == 0)
                        UpdateTrackSelectionForInstrument(1);
                    else
                        UpdateTrackSelectionForInstrument(0);
                }
                EventHelper();
            }
            else
            {
                this.NoteOffset = 0;
                //this.midiChannels.Clear();
                //this.midiTracks.Clear();
                this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
                autoEnableFlutes = true;
                PredictedFluteChannel = GetFlutePrediction(true); // This sets them as active or not with autoEnableFlutes on
                autoEnableFlutes = false;

                UpdateTrackSelectionForInstrument(instrumentId);
                foreach (var t in midiTracks.Values)
                    t.Active = !t.Active;
                if (instrumentId == 0)
                    UpdateTrackSelectionForInstrument(1);
                else
                    UpdateTrackSelectionForInstrument(0);

                EventHelper();
            }
            // Fix up the instrument IDs in case we didn't get them before... unsure why they're included at all tbh but, future compatibility I guess
            //foreach (var kvp in DataDictionary)
            //    kvp.Value.InstrumentID = kvp.Key;

        }

        public void LoadTracks(Dictionary<int, MidiChannelItem> channels, Dictionary<int, TrackItem> tracks)
        {
            UnloadTracks();
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
            DataDictionary.Clear();
            for (int i = 0; i < 16; i++)
            {
                var data = GetTrackSelectionData(i);
                data.InstrumentID = i;
                OriginalDataDictionary[i] = data;
            }

            //EventHelper();
        }

        // Returns the channel ID of the channel most likely to be good for flute
        private MidiChannelItem GetFlutePrediction(bool reorderTracks = false)
        {
            if (neural == null)
                neural = SetupNeuralNetwork();
            
            var activeChannels = midiChannels.Values.Where(c => c.Id != 9 && c.midiInstrument != 9 && c.tickNotes.Any(tn => tn.Value.Any())).OrderBy(c => c.Id).ToArray();
            var activeTracks = midiTracks.Values.Where(c => c.tickNotes.Any(tn => tn.Value.Any(n => n.channel != 9 && !midiChannels.Values.Any(mc => mc.Id == n.channel && mc.midiInstrument == 9)))).OrderBy(c => c.Id).ToArray();
            if (activeChannels.Length < 2)
                return null;
            else // if(simpleML != null && simpleML.Trained)
            {
                Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();

                int index = 0;
                foreach (var channel in activeChannels)
                {
                    var inputs = channel.GetNeuralInputs(index++, activeChannels.Length);
                    var neuralResults = neural.FeedForward(inputs);
                    channelResults[channel] = neuralResults[0];
                }
                index = 0;
                foreach (var channel in activeTracks)
                {
                    var inputs = channel.GetNeuralInputs(index++, activeTracks.Length); // When training, I gave it all of them; this time I'm trimming some out, but the IDs still need to be ok
                    var neuralResults = neural.FeedForward(inputs);
                    channelResults[channel] = neuralResults[0]; // The tracks are MidiChannels and can work in this way; later we just check if they are a MidiTrack
                }


                var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);

                // Softmax them...
                //var resultFloats = orderedResults.Select(c => c.Value).ToArray();
                //resultFloats = NeuralNetwork.Softmax(resultFloats);
                //for (int i = 0; i < resultFloats.Length; i++)
                //    channelResults[orderedResults.ElementAt(i).Key] = resultFloats[i];
                //orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);
                // Get softmaxed values... maybe? No.  We don't want that, in a large song that leaves many at low percent
                // We just want to scale the value between either -1 and 1, or -0.5 and 0.5
                // Just adding 0.5 gets us from 0 to 1 for most purposes, let's try that
                // It's funny when they're above 100% or below 0%, but messy
                // 
                orderedResults = orderedResults.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value + 1) / 2).OrderByDescending(kvp => kvp.Value);


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

                var numResults = orderedResults.Count();
                int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);

                //float autoThreshold = 0.50f;
                float autoThreshold = orderedResults.Average(kvp => kvp.Value);

                for (int i = 0; i < orderedResults.Count(); i++)
                //foreach (var channel in activeChannels)
                {
                    //var channel = activeChannels.Where(c => c.Id == orderedResults.ElementAt(i).Key).SingleOrDefault();
                    var channel = orderedResults.ElementAt(i).Key;
                    if (channel != null)
                    {
                        
                        if (!channel.Rank.HasValue || reorderTracks)
                        {
                            channel.Rank = i;
                        }
                        string ident = "Channel";
                        if (channel.isTrack)
                        {
                            if (instrumentId == 1)
                            {
                                if (autoEnableFlutes && orderedResults.ElementAt(i).Value > autoThreshold)
                                {
                                    channel.Active = true;
                                }
                                else if (autoEnableFlutes)
                                    channel.Active = false;
                            }
                            else if (autoEnableFlutes && instrumentId == 0)
                            {
                                channel.Active = orderedResults.ElementAt(i).Value <= autoThreshold;
                            }
                            ident = "Track";
                            channel.Name += $"{channel.Id}(Flute Rank {trackCount + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                            if (DataDictionary.ContainsKey(0))
                                if (DataDictionary[0].MidiTracks.Any(t => t.Id == channel.Id))
                                {
                                    if (!channel.Rank.HasValue || reorderTracks)
                                    {
                                        DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Rank = i;
                                    }
                                    DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Name += $"{channel.Id}(Flute Rank {trackCount + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                                    if (autoEnableFlutes)
                                        DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Active = orderedResults.ElementAt(i).Value <= autoThreshold;
                                }
                            if (DataDictionary.ContainsKey(1))
                                if (DataDictionary[1].MidiTracks.Any(t => t.Id == channel.Id))
                                {
                                    if (!channel.Rank.HasValue || reorderTracks)
                                        DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Rank = i;
                                    DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Name += $"{channel.Id}(Flute Rank {trackCount + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                                    if (autoEnableFlutes)
                                        DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Active = orderedResults.ElementAt(i).Value > autoThreshold;
                                }

                            trackCount++;
                        }
                        else
                        {
                            channel.Name += $"{channel.Id}(Flute Rank {count + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                            if (autoEnableFlutes)
                                channel.Active = true; // Auto-enable works on tracks, not channels, which all get activated
                            if (DataDictionary.ContainsKey(0))
                                if (DataDictionary[0].MidiChannels.Any(t => t.Id == channel.Id))
                                {
                                    if (!channel.Rank.HasValue || reorderTracks)
                                    {
                                        channel.Rank = i;
                                        DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Rank = i;
                                    }
                                    if (autoEnableFlutes)
                                        DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Active = true;
                                    DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Name += $"{channel.Id}(Flute Rank {count + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                                }
                            if (DataDictionary.ContainsKey(1))
                                if (DataDictionary[1].MidiChannels.Any(t => t.Id == channel.Id))
                                {
                                    if (!channel.Rank.HasValue || reorderTracks)
                                        DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Rank = i;
                                    DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Name += $"{channel.Id}(Flute Rank {count + 1} - {Math.Round(orderedResults.ElementAt(i).Value * 100, 2)}%)";
                                    if (autoEnableFlutes)
                                        DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Active = true;
                                }
                            count++;
                        }
                    }
                }
                var drums = midiChannels.Values.Where(c => c.Id == 9 || c.midiInstrument == 9).Concat(midiTracks.Values.Where(c => c.tickNotes.Any(tn => tn.Value.Any(n => n.channel == 9 || midiChannels.Values.Any(mc => mc.Id == n.channel && mc.midiInstrument == 9)))).Cast<MidiChannelItem>()).ToArray();
                var empty = midiChannels.Values.Where(c => !c.tickNotes.Any() || c.tickNotes.All(tn => !tn.Value.Any())).Concat(midiTracks.Values.Where(c => !c.tickNotes.Any() || c.tickNotes.All(tn => !tn.Value.Any())).Cast<MidiChannelItem>()).ToArray();
                foreach (var channel in drums)
                {
                    channel.Active = false;
                    channel.Name += " (Drums)";
                    if (!channel.isTrack)
                    {
                        if (DataDictionary.ContainsKey(0) && DataDictionary[0].MidiChannels.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                        if (DataDictionary.ContainsKey(1) && DataDictionary[1].MidiChannels.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                    }
                    else
                    {
                        if (DataDictionary.ContainsKey(0) && DataDictionary[0].MidiTracks.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                        if (DataDictionary.ContainsKey(1) && DataDictionary[1].MidiTracks.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                    }
                }
                foreach (var channel in empty)
                {
                    channel.Active = false;
                    channel.Name += " (Empty)";
                    if (!channel.isTrack)
                    {
                        if (DataDictionary.ContainsKey(0) && DataDictionary[0].MidiChannels.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[0].MidiChannels.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                        if (DataDictionary.ContainsKey(1) && DataDictionary[1].MidiChannels.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[1].MidiChannels.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                    }
                    else
                    {
                        if (DataDictionary.ContainsKey(0) && DataDictionary[0].MidiTracks.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[0].MidiTracks.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
                        if (DataDictionary.ContainsKey(1) && DataDictionary[1].MidiTracks.Any(t => t.Id == channel.Id))
                        {
                            DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Active = false;
                            DataDictionary[1].MidiTracks.Where(t => t.Id == channel.Id).First().Name = channel.Name;
                        }
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



        public ChannelMessage FilterMidiEvent(ChannelMessage message, int trackId)
        {
            ChannelMessage newMessage = message;
            if (midiTracks.ContainsKey(trackId))
            {
                var track = midiTracks[trackId];
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
            DataDictionary.Clear();
            NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            //EventHelper();
        }

        public void ToggleTrackActivation(bool active, int index)
        {
            if (midiTracks.ContainsKey(index))
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
            }
        }

        private void ToggleTrackRequestHelper(MidiChannelItem item)
        {
            EventHandler<MidiChannelItem> handler = ToggleTrackRequest;
            handler?.Invoke(this, item);
        }

        private void EventHelper()
        {
            EventHandler handler = TrackChanged;
            handler?.Invoke(this, new EventArgs());
        }
    }
}
