using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;
using LuteBot.UI;
using Newtonsoft.Json;
using Sanford.Multimedia.Midi;
using System.Linq;
using SimpleML;
using LuteBot.UI.Utils;

namespace LuteBot.TrackSelection
{
    public class TrackSelectionManager
    {
        public Dictionary<int, MidiChannelItem> MidiChannels { get; private set; } = new Dictionary<int, MidiChannelItem>();
        public Dictionary<int, MidiChannelItem> MidiTracks { get; private set; } = new Dictionary<int, MidiChannelItem>();

        public MidiPlayer Player { get; set; }

        public TrackSelectionManager(MidiPlayer player)
        {
            Player = player;
        }

        public List<MidiChannelItem> GetTracksAndChannels()
        {
            return MidiChannels.Values.Concat(MidiTracks.Values).ToList();
        }

        // TODO: This can go anywhere else
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
                neural.Load(Path.Combine(AppContext.BaseDirectory, "channelNeural"));
            }
            return neural;
        }


        public async Task SaveTrackManager(string filename = null)
        {
            // Let's just save a collection of each channel and track's Settings, Id, and IsTrack
            // Deal with trimming it later

            // ... I guess I could make simpleDataDictionary for now
            var simpleDataDictionary = new Dictionary<int, SimpleTrackSelectionData>();
            foreach (var channel in GetTracksAndChannels())
            {
                foreach (var setting in channel.Settings.Values)
                {
                    if (!setting.Active || setting.Offset != 0 || setting.Rank.HasValue)
                    {
                        if (!simpleDataDictionary.ContainsKey(setting.InstrumentId))
                            simpleDataDictionary[setting.InstrumentId] = new SimpleTrackSelectionData() { InstrumentID = setting.InstrumentId };
                        var simpleChannel = new SimpleMidiChannelItem() { Active = setting.Active, Id = channel.Id, Rank = setting.Rank, offset = setting.Offset == 0 ? null : setting.Offset };
                        if (channel.IsTrack)
                            simpleDataDictionary[setting.InstrumentId].MidiTracks.Add(simpleChannel);
                        else
                            simpleDataDictionary[setting.InstrumentId].MidiChannels.Add(simpleChannel);
                    }
                }
            }
            await SaveManager.SaveTrackSelectionData(simpleDataDictionary, Player.FileName, filename).ConfigureAwait(false);
        }

        public void LoadTrackManager(bool reorderTracks = false, bool autoEnableFlutes = false, bool clearOffsets = false)
        {
            // Meant to be called after the file has been loaded, to load settings from the file
            var simpleDataDict = SaveManager.LoadTrackSelectionData(Player.FileName);

            if (simpleDataDict != null && !autoEnableFlutes) // This is effectively a full reset, don't bother loading settings
            {
                // This is a dictionary of instrumentId to settings, and we need to convert it to our new style of storage
                // To build a ConfiguredMidiChannelItem, I need a dictionary of all of each given MidiChannelItem, for each InstrumentId

                // They've just got MidiChannels and MidiTracks, which are SimpleMidiChannelItems with just the basic settings and info on them

                // So, we need one of these dictionaries for each track and channel
                var oldChannels = new Dictionary<int, Dictionary<int, SimpleMidiChannelItem>>();
                var oldTracks = new Dictionary<int, Dictionary<int, SimpleMidiChannelItem>>();

                foreach (var kvp in simpleDataDict)
                {
                    if (kvp.Value.MidiChannels != null)
                        foreach (var c in kvp.Value.MidiChannels)
                        {
                            if (!oldChannels.ContainsKey(c.Id))
                                oldChannels[c.Id] = new Dictionary<int, SimpleMidiChannelItem>();
                            oldChannels[c.Id][kvp.Value.InstrumentID] = c;
                        }
                    if (kvp.Value.MidiTracks != null)
                        foreach (var c in kvp.Value.MidiTracks)
                        {
                            if (!oldTracks.ContainsKey(c.Id))
                                oldTracks[c.Id] = new Dictionary<int, SimpleMidiChannelItem>();
                            oldTracks[c.Id][kvp.Value.InstrumentID] = c;
                        }
                }

                foreach (var kvp in oldChannels)
                {
                    if (!MidiChannels.ContainsKey(kvp.Key))
                        MidiChannels[kvp.Key] = new MidiChannelItem(kvp.Key, false).WithOldSettings(kvp.Value);
                    else
                        MidiChannels[kvp.Key] = MidiChannels[kvp.Key].WithOldSettings(kvp.Value);
                }
                foreach (var kvp in oldTracks)
                {
                    if (!MidiTracks.ContainsKey(kvp.Key))
                        MidiTracks[kvp.Key] = new MidiChannelItem(kvp.Key, true).WithOldSettings(kvp.Value);
                    else
                        MidiTracks[kvp.Key] = MidiTracks[kvp.Key].WithOldSettings(kvp.Value);
                }

                SetFlutePredictions();
                ApplyAutomaticFixes(reorderTracks, autoEnableFlutes, clearOffsets);
            }
            else
            {
                SetFlutePredictions();
                ApplyAutomaticFixes(true, true);
            }

        }

        public static bool ShouldUseChannels(MidiChannelItem[] activeChannels, MidiChannelItem[] activeTracks)
        {
            return activeTracks.Length < 2 || (activeChannels.Max(c => c.FluteRating) > activeTracks.Max(c => c.FluteRating) && activeChannels.Any(c => c.FluteRating < 0.50f) && activeChannels.Any(c => c.FluteRating >= 0.50f));
        }

        public void ApplyAutomaticFixes(bool reorderTracks = false, bool autoEnable = false, bool clearOffsets = false)
        {
            int rank = 0;
            var activeChannels = GetValidChannels();
            var activeTracks = GetValidTracks();
            // This is a bit wasteful, but it helps ensure data isn't skewed by empty stuff
            // And hopefully at least channels or tracks, one or the other, will have at least one good instrument for each
            bool useChannels = ShouldUseChannels(activeChannels, activeTracks);
            foreach (var channel in MidiChannels.Values.Concat(MidiTracks.Values).OrderByDescending(c => c.FluteRating))
            {
                // First, make sure we have a setting for each instrument... though I think we already do...?

                foreach (var instrument in Instrument.Prefabs)
                {
                    var instrumentId = instrument.Key;
                    if (!channel.Settings.ContainsKey(instrumentId))
                        channel.Settings[instrumentId] = new ChannelSettings() { InstrumentId = instrumentId };

                    if (clearOffsets)
                        channel.Settings[instrumentId].Offset = 0;

                    if (channel.IsTrack && !activeTracks.Any(t => t.Id == channel.Id))
                        channel.Settings[instrument.Value.Id].Active = false;
                    else if (!channel.IsTrack && !activeChannels.Any(t => t.Id == channel.Id))
                        channel.Settings[instrument.Value.Id].Active = false;
                    else
                    {
                        if (reorderTracks || channel.Settings[instrumentId].Rank == null)
                            channel.Settings[instrumentId].Rank = rank; // TODO: Will this interfere with other existing ranks?

                        if (autoEnable)
                        {
                            if ((useChannels && channel.IsTrack) || (!useChannels && !channel.IsTrack))
                            {
                                channel.Settings[instrumentId].Active = true;
                            }
                            else
                            {
                                if (channel.FluteRating >= instrument.Value.AIMinimum && channel.FluteRating <= instrument.Value.AIMaximum)
                                    channel.Settings[instrumentId].Active = true;
                                else
                                    channel.Settings[instrumentId].Active = false;
                            }
                        }
                    }
                }

                rank++;
            }
        }

        public void LoadTracks(Dictionary<int, MidiChannelItem> midiChannels, Dictionary<int, MidiChannelItem> midiTracks)
        {
            MidiChannels = midiChannels ?? new Dictionary<int, MidiChannelItem>();
            MidiTracks = midiTracks ?? new Dictionary<int, MidiChannelItem>();
        }

        public MidiChannelItem[] GetValidChannels()
        {
            return MidiChannels.Values.Where(c => c.Id != 9 && c.MidiInstrument != 9 && c.TickNotes.Any(tn => tn.Value.Any())).OrderBy(c => c.Id).ToArray();
        }
        public MidiChannelItem[] GetValidTracks()
        {
            return MidiTracks.Values.Where(c => c.TickNotes.Any(tn => tn.Value.Any(n => n.channel != 9 && !MidiChannels.Values.Any(mc => mc.Id == n.channel && mc.MidiInstrument == 9)))).OrderBy(c => c.Id).ToArray();
        }

        // Sets data on each Channel and Track about its predicted ranking
        private void SetFlutePredictions()
        {
            // TODO: Load the neural settings only once from file, but still make one network per prediction for threadsafety
            var neural = SetupNeuralNetwork();

            var activeChannels = GetValidChannels();
            var activeTracks = GetValidTracks();

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
            // Scale them all up to get the highest to 100%
            var scale = 1f / channelResults.Values.Max(v => (v + 1) / 2);
            foreach (var kvp in channelResults)
            {
                if (kvp.Key.IsTrack)
                {
                    if (MidiTracks.TryGetValue(kvp.Key.Id, out var channel))
                    {
                        channel.FluteRating = (kvp.Value + 1) / 2 * scale;
                        channel.Name += $" (Flute: {Math.Round((kvp.Value + 1) / 2 * 100 * scale, 1)}%)";
                    }
                }
                else
                {
                    if (MidiChannels.TryGetValue(kvp.Key.Id, out var channel))
                    {
                        channel.FluteRating = (kvp.Value + 1) / 2 * scale;
                        channel.Name += $" (Flute: {Math.Round((kvp.Value + 1) / 2 * 100 * scale, 1)}%)";
                    }
                }
            }
        }


    }
}
