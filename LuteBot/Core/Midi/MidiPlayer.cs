using LuteBot.Config;
using LuteBot.TrackSelection;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Core.Midi
{
    public class MidiPlayer : Player
    {

        private OutputDevice outDevice;
        private Sequence sequence;
        private Sequencer sequencer;
        private MordhauOutDevice mordhauOutDevice;
        private TrackSelectionManager trackSelectionManager;

        private bool isPlaying;

        public MidiPlayer(TrackSelectionManager trackSelectionManager)
        {
            isPlaying = false;
            mordhauOutDevice = new MordhauOutDevice();
            this.trackSelectionManager = trackSelectionManager;
            sequence = new Sequence
            {
                Format = 1
            };

            sequencer = new Sequencer
            {
                Position = 0,
                Sequence = this.sequence
            };
            sequencer.PlayingCompleted += new System.EventHandler(this.HandlePlayingCompleted);
            sequencer.ChannelMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.ChannelMessageEventArgs>(this.HandleChannelMessagePlayed);
            sequencer.SysExMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.SysExMessageEventArgs>(this.HandleSysExMessagePlayed);
            sequencer.Chased += new System.EventHandler<Sanford.Multimedia.Midi.ChasedEventArgs>(this.HandleChased);
            sequencer.Stopped += new System.EventHandler<Sanford.Multimedia.Midi.StoppedEventArgs>(this.HandleStopped);

            if (!(OutputDevice.DeviceCount == 0))
            {
                outDevice = new OutputDevice(0);
                sequence.LoadCompleted += HandleLoadCompleted;
            }
        }

        public void ResetDevice()
        {
            outDevice.Reset();
        }

        public void UpdateMutedTracks(TrackItem item)
        {
            if (isPlaying)
            {
                sequencer.Stop();
            }
            sequence.UpdateMutedTracks(item.Id, item.Active);
            if (isPlaying)
            {
                sequencer.Continue();
            }
            outDevice.Reset();
        }

        public List<int> GetMidiChannels()
        {
            return sequence.Channels;
        }

        public List<string> GetMidiTracks()
        {
            return sequence.TrackNames();
        }

        public override int GetLenght()
        {
            return sequence.GetLength();
        }

        public override string GetFormattedPosition()
        {
            TimeSpan time = TimeSpan.FromSeconds(0);
            time = TimeSpan.FromSeconds((int)((sequencer.Position / sequence.Division) * this.sequence.FirstTempo / 1000000f));
            return time.ToString(@"mm\:ss");
        }

        public override string GetFormattedLength()
        {
            TimeSpan time = TimeSpan.FromSeconds(0);
            time = TimeSpan.FromSeconds((int)((sequence.GetLength() / sequence.Division) * this.sequence.FirstTempo / 1000000f));
            return time.ToString(@"mm\:ss");
        }

        public override void SetPosition(int position)
        {
            sequencer.Position = position;
        }

        public override int GetPosition()
        {
            return sequencer.Position;
        }

        public override void ResetSoundEffects()
        {
            outDevice.Reset();
        }

        public override void LoadFile(string filename)
        {
            sequence.LoadAsync(filename);
        }

        public override void Stop()
        {
            isPlaying = false;
            sequencer.Stop();
            sequencer.Position = 0;
            outDevice.Reset();
        }

        public override void Play()
        {
            isPlaying = true;
            if (sequencer.Position > 0)
            {
                sequencer.Continue();
            }
            else
            {
                sequencer.Start();
            }
        }

        public override void Pause()
        {
            isPlaying = false;
            sequencer.Stop();
            outDevice.Reset();
        }

        public override void Dispose()
        {
            this.disposed = true;
            sequence.Dispose();

            if (outDevice != null)
            {
                outDevice.Dispose();
            }
        }

        //------------- Handlers -------------

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                base.LoadCompleted(this, e);

                mordhauOutDevice.HighMidiNoteId = sequence.MaxNoteId;
                mordhauOutDevice.LowMidiNoteId = sequence.MinNoteId;
            }
        }

        private void HandleStopped(object sender, StoppedEventArgs e)
        {
            foreach (ChannelMessage message in e.Messages)
            {
                if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
                {
                    outDevice.Send(message);
                }
            }
        }

        private void HandleChased(object sender, ChasedEventArgs e)
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
            {
                foreach (ChannelMessage message in e.Messages)
                {
                    outDevice.Send(message);
                }
            }
        }

        private void HandleSysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
        }

        private void HandleChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            if (ConfigManager.GetProperty(PropertyItem.NoteConversionMode) != "1")
            {
                if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
                {
                    outDevice.Send(mordhauOutDevice.FilterNote(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId)));
                }
                else
                {
                    mordhauOutDevice.SendNote(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId));
                }
            }
            else
            {
                outDevice.Send(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId));
            }
        }

        private void HandlePlayingCompleted(object sender, EventArgs e)
        {
            isPlaying = false;
            outDevice.Reset();
        }
    }
}

