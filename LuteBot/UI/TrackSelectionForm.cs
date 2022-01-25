using LuteBot.Config;
using LuteBot.Core;
using LuteBot.TrackSelection;
using LuteBot.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class TrackSelectionForm : Form
    {
        TrackSelectionManager trackSelectionManager;
        MordhauOutDevice _mordhauOut;
        RustOutDevice _rustOut;
        LuteBotForm mainForm;

        // We need only one out device, might as well use rust, but take both for now cuz why not, feels unfair
        // Though they both get updated with the same values at the same time, for what we're doing
        public TrackSelectionForm(TrackSelectionManager trackSelectionManager, MordhauOutDevice mordhauOut, RustOutDevice rustOut, LuteBotForm mainForm)
        {
            _mordhauOut = mordhauOut;
            _rustOut = rustOut;
            this.mainForm = mainForm;
            this.trackSelectionManager = trackSelectionManager;
            trackSelectionManager.TrackChanged += new EventHandler(TrackChangedHandler);
            this.Load += TrackSelectionForm_Load;
            InitializeComponent();

            SuspendLayout();
            Instrument.Read();
            instrumentsBox.DisplayMember = "Name";
            foreach (Instrument i in Instrument.Prefabs)
                instrumentsBox.Items.Add(i);
            instrumentsBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            ResumeLayout();
            

            InitLists();
            trackSelectionManager.autoLoadProfile = AutoActivateCheckBox.Checked;
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
            | BindingFlags.Instance | BindingFlags.NonPublic, null,
            OffsetPanel, new object[] { true }); // Internet suggested this... 

            textBoxNotesForChords.Text = trackSelectionManager.NumChords.ToString();

            IO.KB.ActionManager.NotePlayed += _mordhauOut_notePlayed;

            
        }

        private void _mordhauOut_notePlayed(object sender, int channel)
        {
            // Mark the channel to flash and redraw the graphic (twice)
            if (channelColors.ContainsKey(channel)) // If they have not entered advanced mode on this midi at any point, there will not be a color
            { // And in that case there's no reason to do anything
                var original = channelColors[channel];
                channelColors[channel] = Color.FromArgb(Math.Min(original.R * 2, 255), Math.Min(original.G * 2, 255), Math.Min(original.B * 2, 255));
                System.Threading.Timer t;
                t = new System.Threading.Timer(T_Tick, channel, ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown), System.Threading.Timeout.Infinite);
                BeginInvoke((MethodInvoker)delegate
                {
                    OffsetPanel.Refresh();
                });
            }
        }

        private void T_Tick(object state)
        {
            int channel = (int)state;
            //var original = channelColors[channel];
            //channelColors[channel] = Color.FromArgb(Math.Min(original.R / 2, 255), Math.Min(original.G / 2, 255), Math.Min(original.B / 2, 255));
            channelColors[channel] = originalChannelColors[channel];
            BeginInvoke((MethodInvoker)delegate
            {
                OffsetPanel.Refresh();
            });
        }

        private void TrackSelectionForm_Load(object sender, EventArgs e)
        {
            //panel1.AutoScroll = false;
            //panel1.VerticalScroll.Enabled = true;
            //panel1.VerticalScroll.Visible = true;
            //panel1.AutoScroll = true;
            //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
            panel1.AutoScroll = false;
            panel1.VerticalScroll.Enabled = true;
            panel1.VerticalScroll.Visible = true;
            panel1.HorizontalScroll.Enabled = false;
            panel1.HorizontalScroll.Visible = false;
            panel1.AutoScroll = true;
            OffsetPanel.Paint += new PaintEventHandler(DrawGraphics);
            OffsetPanel.Resize += OffsetPanel_Resize;
            OffsetPanel.MouseDown += OffsetPanel_MouseDown;
            OffsetPanel.MouseUp += OffsetPanel_MouseUp;
            OffsetPanel.MouseLeave += OffsetPanel_MouseLeave;
            OffsetPanel.MouseCaptureChanged += OffsetPanel_MouseLeave; // These should do the same thing
            OffsetPanel.MouseMove += OffsetPanel_MouseMove;
            OffsetPanel.MouseEnter += OffsetPanel_MouseEnter;
        }

        // This mouse stuff is going to suck
        private Point dragStart;
        private bool dragging;
        private Rectangle draggableRect;
        private int startOffset;
        private Dictionary<MidiChannelItem, Rectangle> channelRects = new Dictionary<MidiChannelItem, Rectangle>();
        private Dictionary<int, Color> channelColors = new Dictionary<int, Color>();
        private Dictionary<int, Color> originalChannelColors = new Dictionary<int, Color>();
        private MidiChannelItem dragTarget;

        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        private void OffsetPanel_MouseEnter(object sender, EventArgs e) => OffsetPanel.Refresh();
        private void OffsetPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                // Set offset to (mousePos - dragStart)/columnWidth/12 (round to octaves...?)
                int multiplier = (e.Location.X < dragStart.X ? -1 : 1); // Preserve sign

                if (isAdvanced)
                {
                    int oldOffset = trackSelectionManager.MidiChannelOffsets.ContainsKey(dragTarget.Id) ? trackSelectionManager.MidiChannelOffsets[dragTarget.Id] : 0;
                    trackSelectionManager.MidiChannelOffsets[dragTarget.Id] = startOffset + (int)Math.Round(((GetDistance(dragStart, e.Location) * multiplier / columnWidth)) / 12) * 12;
                    if (trackSelectionManager.MidiChannelOffsets[dragTarget.Id] != oldOffset)
                        OffsetPanel.Refresh();
                }
                else
                {
                    int oldOffset = trackSelectionManager.NoteOffset;
                    trackSelectionManager.NoteOffset = startOffset + (int)Math.Round(((GetDistance(dragStart, e.Location) * multiplier / columnWidth)) / 12) * 12;
                    if (trackSelectionManager.NoteOffset != oldOffset)
                        OffsetPanel.Refresh();
                }
            }

            //OffsetPanel.Refresh(); // See if this is laggy
            // Yeah a little

        }
        private void OffsetPanel_MouseLeave(object sender, EventArgs e)
        {
            dragging = false;
        }
        private void OffsetPanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private void OffsetPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (isAdvanced)
            {
                foreach (var kvp in channelRects)
                {
                    if (kvp.Value.Contains(e.Location))
                    {
                        dragging = true;
                        dragStart = e.Location;
                        if (isAdvanced)
                            startOffset = trackSelectionManager.MidiChannelOffsets.ContainsKey(kvp.Key.Id) ? trackSelectionManager.MidiChannelOffsets[kvp.Key.Id] : 0;
                        dragTarget = kvp.Key;
                        return;
                    }
                }
            }
            // Check if their mouse is inside the draggableRect
            else if (draggableRect.Contains(e.Location))
            {
                dragging = true;
                dragStart = e.Location;
                startOffset = trackSelectionManager.NoteOffset;
            }
        }

        public void RefreshOffsetPanel() => OffsetPanel.Refresh();

        internal class ChannelData
        {
            internal string name { get; set; }
            internal Rectangle bgLabelRect { get; set; }
            internal Rectangle labelRect { get; set; }
        }

        private void OffsetPanel_Resize(object sender, EventArgs e)
        {
            OffsetPanel.Refresh();
        }

        private static readonly Color gridColor = Color.Gray;
        private static readonly Brush gridBrush = new SolidBrush(gridColor);
        private static readonly Pen gridPen = new Pen(gridBrush, 1);
        private static readonly Font gridFont = new Font(FontFamily.GenericSerif, 10f);
        private static readonly Font labelFont = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Bold);
        private static readonly Color gridHighlightColor = Color.DarkGoldenrod;
        private static readonly Brush gridHighlightBrush = new SolidBrush(gridHighlightColor);
        private static readonly Pen gridHighlightPen = new Pen(gridHighlightBrush, 2);
        private static readonly Color instrumentBarColor = Color.DarkSlateGray;
        private static readonly Brush instrumentBarBrush = new SolidBrush(instrumentBarColor);
        private static readonly Color draggableRectColor = Color.DarkGreen;
        private static readonly Brush draggableRectBrush = new SolidBrush(draggableRectColor);
        private static readonly Brush labelBrush = Brushes.Black;
        private static readonly Brush labelBgBrush = Brushes.Goldenrod;
        private static readonly Brush shadowBrush = Brushes.Black;

        int labelHeight = 20; // We have two labels, too, remember
        int padding = 6; // Seems to suck without this
        int xPad = 10;
        int rowHeight;
        int columnWidth;
        int defaultRowHeight;

        private Random random = new Random();

        private bool isAdvanced = false;
        private void DrawGraphics(object sender, PaintEventArgs e)
        {
            List<ChannelData> data = new List<ChannelData>();

            int numRows = 2; // Default
            if (isAdvanced)
                numRows = 1 + trackSelectionManager.MidiChannels.Where((c) => c.Active).Count();
            rowHeight = (OffsetPanel.Height - labelHeight * numRows - padding) / numRows;
            columnWidth = (int)Math.Round(((OffsetPanel.Width - padding) / 10f) / 12);

            Graphics g = e.Graphics;
            g.Clear(OffsetPanel.BackColor);

            // We need columnWidth to be a Note, not an octave
            int width = columnWidth * 10 * 12;
            int height = rowHeight * numRows;
            // Let's go ahead and get the instrument info

            int lowest = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId) + ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);
            int noteCount = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            // Try to find the center
            var center = lowest + noteCount / 2;
            // Round down I guess
            center -= center % 12;
            int centerLine = center / 12; // This gives us the octave, like 0, 1, 2 usually
            // Now just take 5 - centerLine
            //int centerOffset = xPad/2 - centerLine;
            int centerOffset = 0;
            // and do the label as lineNum - that

                // Later notes: I don't know what centerLine and centerOffset are for.  centerLine represents the octave at the center, usually 4 for C4
                // centerOffset is... idk?  How many octaves we need to offset from 0 on far left, I guess
                // No idea what xPad has to do with it.

                // Wait what
                // center is the midi note value for the center of the instrument range (rounded down if not even octave)
                // centerLine is the octave
                // centerOffset is ... I still don't know.  

            // We're going to iterate twice so the background grids are behind everything
            for (int x = xPad; x <= OffsetPanel.Width; x += columnWidth)
            {
                g.DrawLine(gridPen, x, 0, x, height);
            }

            // Draw the instrument bar first so the rest of the grid goes over it
            // We use -12 here because all logic and values like LowestPlayedNote assume MIDI0 = C0, but that's wrong, MIDI0=C-1
            int rectStartX = (lowest * columnWidth) + xPad;// - 12*columnWidth
            Rectangle instrumentRect = new Rectangle(rectStartX, 1, (noteCount * columnWidth), rowHeight - 1);
            g.FillRectangle(instrumentBarBrush, instrumentRect);

            int minNote = 0;
            int maxNote = 127;
            if (trackSelectionManager.MidiChannels.Count > 0)
            {
                // Sometimes, MinNoteByChannel or MaxNoteByChannel may not have value if the track never played any notes
                minNote = trackSelectionManager.MidiChannels.Min((c) => (trackSelectionManager.MinNoteByChannel.ContainsKey(c.Id) ? trackSelectionManager.MinNoteByChannel[c.Id] + trackSelectionManager.NoteOffset + (trackSelectionManager.MidiChannelOffsets.ContainsKey(c.Id) ? trackSelectionManager.MidiChannelOffsets[c.Id] : 0):127));
                maxNote = trackSelectionManager.MidiChannels.Max((c) => (trackSelectionManager.MaxNoteByChannel.ContainsKey(c.Id) ? trackSelectionManager.MaxNoteByChannel[c.Id] + trackSelectionManager.NoteOffset + (trackSelectionManager.MidiChannelOffsets.ContainsKey(c.Id) ? trackSelectionManager.MidiChannelOffsets[c.Id] : 0):0));
            }

            if (isAdvanced)
            {
                channelRects = new Dictionary<MidiChannelItem, Rectangle>();
                // Draw each MidiChannel as a row with its own offset
                int currentY = rowHeight;
                foreach (var channel in trackSelectionManager.MidiChannels)
                {
                    if (channel.Active)
                    {
                        var offset = trackSelectionManager.NoteOffset + (trackSelectionManager.MidiChannelOffsets.ContainsKey(channel.Id) ?
                            trackSelectionManager.MidiChannelOffsets[channel.Id] : 0);
                        Rectangle channelRect = new Rectangle(xPad + offset * columnWidth + trackSelectionManager.MinNoteByChannel[channel.Id] * columnWidth, currentY + 1,
                            (trackSelectionManager.MaxNoteByChannel[channel.Id] - trackSelectionManager.MinNoteByChannel[channel.Id]) * columnWidth, rowHeight - 1);
                        channelRects.Add(channel, channelRect);

                        if (!channelColors.ContainsKey(channel.Id))
                        {
                            channelColors[channel.Id] = Color.FromArgb(random.Next(0, 200), random.Next(0, 200), random.Next(0, 200));
                            originalChannelColors[channel.Id] = channelColors[channel.Id];
                        }

                        // K, draw
                        g.FillRectangle(new SolidBrush(channelColors[channel.Id]), channelRect);
                        // We draw the rest later, after drawing the overlying gold lines...
                        // So, store it all to calc later...


                        currentY += rowHeight;
                    }
                }
            }
            else
            {
                // We're going to need to figure out the note range using adjusted offsets
                // TODO: These might except if things aren't set for the channel.  Not sure
                // I could also combine them into one iteration, this is two unnecessarily


                // Draw draggableRect over the bg grid
                // So the width is the midi's note range, it starts at offset.  
                // Also, minNote already has all the offsets in it.
                draggableRect = new Rectangle(xPad + minNote * columnWidth, rowHeight + 1, (maxNote - minNote) * columnWidth, rowHeight - 1);
                g.FillRectangle(draggableRectBrush, draggableRect);
            }


            // We use straight-up width because we want this to continue as far as it can
            for (int x = xPad; x <= OffsetPanel.Width; x += columnWidth)
            {
                // Draw vertical line

                // Put labels at the bottom
                int note = ((x - xPad) / columnWidth) - centerOffset*12;

                if (isAdvanced)
                { // For brevity and cuz I think it's safe here, assume the dicts are populated
                    int labelY = height + padding + labelHeight;
                    foreach (var kvp in channelRects)
                    {
                        int offset = trackSelectionManager.NoteOffset + (trackSelectionManager.MidiChannelOffsets.ContainsKey(kvp.Key.Id) ? trackSelectionManager.MidiChannelOffsets[kvp.Key.Id] : 0);
                        int midiLowest = trackSelectionManager.MinNoteByChannel[kvp.Key.Id] + offset;
                        int midiHighest = trackSelectionManager.MaxNoteByChannel[kvp.Key.Id] + offset;
                        if (note >= midiLowest && note <= midiHighest)
                        {
                            // We can consider showing labels...
                            // So then, (note - midiLowest) is how many notes out we are from midiLowest
                            // So midiLowest + (note - midiLowest) is our effective note here
                            int effectiveNote = trackSelectionManager.MinNoteByChannel[kvp.Key.Id] + (note - midiLowest);
                            if (effectiveNote % 12 == 0)
                            {
                                g.DrawString($"C{effectiveNote / 12}", gridFont, new SolidBrush(channelColors[kvp.Key.Id]), x - xPad, labelY);
                            }
                        }
                        labelY += labelHeight;
                    }

                }
                else
                {
                    // We want to add labels of what the draggableRect is at too (and lock it when it drags)
                    // So we need to find our note relative to that... 
                    // So just get _rustOut.LowMidiNoteId + trackSelectionManager.NoteOffset - this is the 'note' it starts at
                    int midiLowest = _rustOut.LowMidiNoteId + trackSelectionManager.NoteOffset;
                    int midiHighest = _rustOut.HighMidiNoteId + trackSelectionManager.NoteOffset;
                    if (note >= midiLowest && note <= midiHighest)
                    {
                        // We can consider showing labels...
                        // So then, (note - midiLowest) is how many notes out we are from midiLowest
                        // So midiLowest + (note - midiLowest) is our effective note here

                        // Also both mordhauOut and rustOut should always have these values set for a loaded song
                        int effectiveNote = _rustOut.LowMidiNoteId + (note - midiLowest);
                        if (effectiveNote % 12 == 0)
                        {
                            g.DrawString($"C{effectiveNote / 12}", gridFont, draggableRectBrush, x - xPad, height + padding + labelHeight);
                        }
                    }
                }


                if (note % 12 == 0)
                {
                    g.DrawString($"C{note / 12}", gridFont, instrumentBarBrush, x - xPad, height + padding);
                    g.DrawLine(gridHighlightPen, x, 0, x, height);
                }

            }
            for (int y = 0; y <= height; y += rowHeight)
            {
                // Draw horizontal line
                g.DrawLine(gridHighlightPen, xPad, y, OffsetPanel.Width, y);
            }
            // Lastly, draw in white font above each of the squares to label them
            // instrumentRect and draggableRect
            // I'm too lazy to center the labels
            // But fine we need some rects behind them too.  
            //int instrumentLabelWidth = (int)(115 * (g.DpiX/96f));
            int instrumentLabelWidth = (int)g.MeasureString("Instrument Range", labelFont).Width + 4;
            Rectangle instrumentLabelRect = new Rectangle(instrumentRect.X + instrumentRect.Width / 2 - instrumentLabelWidth / 2, instrumentRect.Y + rowHeight / 2 - 9, instrumentLabelWidth, 18);
            Rectangle instrumentLabelBgRect = new Rectangle(instrumentLabelRect.X + 1, instrumentLabelRect.Y + 1, instrumentLabelRect.Width + 1, instrumentLabelRect.Height + 1);
            g.FillRectangle(shadowBrush, instrumentLabelBgRect);
            g.FillRectangle(labelBgBrush, instrumentLabelRect);
            
            g.DrawString("Instrument Range", labelFont, labelBrush, instrumentLabelRect);
            // Also our data, if we're on advanced
            if (isAdvanced)
                foreach (var kvp in channelRects)
                {
                    //dpi/96 should give us a scalar for making the boxes wider.  Unsure if it should be X or Y, but one of those
                    
                    // We're wildly guessing here... But it's about 7px per char.  Need 8 to prevent trimming
                    //int channelLabelWidth = (int)((8 * kvp.Key.Name.Length)*(g.DpiX / 96f));
                    int channelLabelWidth = (int)g.MeasureString(kvp.Key.Name, labelFont).Width + 4;

                    Rectangle channelLabelRect = new Rectangle(kvp.Value.X + kvp.Value.Width / 2 - channelLabelWidth / 2, kvp.Value.Y + rowHeight / 2 - 9, channelLabelWidth, 18);
                    Rectangle channelLabelBgRect = new Rectangle(channelLabelRect.X + 1, channelLabelRect.Y + 1, channelLabelRect.Width + 1, channelLabelRect.Height + 1);

                    g.FillRectangle(shadowBrush, channelLabelBgRect);
                    g.FillRectangle(labelBgBrush, channelLabelRect);
                    g.DrawString(kvp.Key.Name, labelFont, labelBrush, channelLabelRect);
                }
            else
            {
                // Guess at a width... this can be static
                //int draggableLabelWidth = (int)(190 * (g.DpiX/96f));
                int draggableLabelWidth = (int)g.MeasureString("Song Range (Click to Drag)", labelFont).Width + 4;

                Rectangle draggableLabelRect = new Rectangle(draggableRect.X + draggableRect.Width / 2 - draggableLabelWidth / 2, draggableRect.Y + rowHeight / 2 - 9, draggableLabelWidth, 18);
                Rectangle draggableLabelBgRect = new Rectangle(draggableLabelRect.X + 1, draggableLabelRect.Y + 1, draggableLabelRect.Width + 1, draggableLabelRect.Height + 1);
                g.FillRectangle(shadowBrush, draggableLabelBgRect);
                g.FillRectangle(labelBgBrush, draggableLabelRect);
                g.DrawString("Song Range (Click to Drag)", labelFont, labelBrush, draggableLabelRect);
            }
        }

        public void InitLists()
        {
            SuspendLayout();
            textBoxNotesForChords.Text = trackSelectionManager.NumChords.ToString();
            textBoxNotesForChords.Enabled = !ConfigManager.GetBooleanProperty(PropertyItem.ForbidsChords);
            foreach (var channel in trackSelectionManager.MidiChannels)
            { // This should help make sure we don't except if we're irresponsible with it
                if (!trackSelectionManager.MidiChannelOffsets.ContainsKey(channel.Id))
                    trackSelectionManager.MidiChannelOffsets[channel.Id] = 0;
            }
            if (isAdvanced) // Reset size for this new track
            {
                int numRows = 2; // Default
                if (isAdvanced)
                    numRows = 1 + trackSelectionManager.MidiChannels.Where((c) => c.Active).Count();

                OffsetPanel.Size = new Size(originalPanelSize.Width, originalPanelSize.Height + defaultRowHeight * (numRows - 2));
                rowHeight = (OffsetPanel.Height - labelHeight * numRows - padding) / numRows;
                //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
            }
            TrackListBox.Items.Clear();
            ChannelsListBox.Items.Clear();
            foreach (MidiChannelItem channel in trackSelectionManager.MidiChannels)
            {
                ChannelsListBox.Items.Add(channel.Name, channel.Active);
            }
            foreach (TrackItem track in trackSelectionManager.MidiTracks)
            {
                TrackListBox.Items.Add(track.Name, track.Active);
            }

            instrumentsBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);

            ResumeLayout();
            Invalidate();
            //Refresh();
            // This is a terrible thing to do, but, there's no easy way to hook the right events to make it wait properly
            // So after a timer, we're refreshing our OffsetPanel again
            //Timer t = new Timer();
            //t.Interval = 100;
            //t.Tick += (object sender, EventArgs e) =>
            //{
            //    if (this.IsHandleCreated && !this.IsDisposed)
            //        BeginInvoke((MethodInvoker)delegate
            //        {
            //            Refresh();
            //            t.Dispose();
            //        });
            //};
            //t.Start();
        }

        private void TrackChangedHandler(object sender, EventArgs e)
        {
            InitLists();
        }

        private void TrackSelectionForm_Closing(object sender, FormClosingEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.TrackSelectionPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
        }

        private void SelectAllChannelsTextBox_CheckedChanged(object sender, EventArgs e)
        {
            trackSelectionManager.ActivateAllChannels = SelectAllChannelsCheckBox.Checked;
            ChannelsListBox.Enabled = !SelectAllChannelsCheckBox.Checked;
            Invalidate();
        }

        private void ChannelListBox_ItemChecked(object sender, ItemCheckEventArgs e)
        {
            trackSelectionManager.ToggleChannelActivation(!(e.CurrentValue == CheckState.Checked), e.Index);
            Invalidate();
        }

        private void SongProfileSaveButton_Click(object sender, EventArgs e)
        {
            if (trackSelectionManager.FileName != null)
            {
                trackSelectionManager.SaveTrackManager();
            }
        }

        private void LoadProfileButton_Click(object sender, EventArgs e)
        {
            if (trackSelectionManager.FileName != null)
            {
                trackSelectionManager.LoadTrackManager();
                InitLists();
            }
        }

        private void AutoActivateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            trackSelectionManager.autoLoadProfile = AutoActivateCheckBox.Checked;
            Invalidate();
        }

        private void SelectAllTracksCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            trackSelectionManager.ActivateAllChannels = SelectAllTracksCheckBox.Checked;
            TrackListBox.Enabled = !SelectAllTracksCheckBox.Checked;
            Invalidate(); 
        }

        private void TrackListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            trackSelectionManager.ToggleTrackActivation(!(e.CurrentValue == CheckState.Checked), e.Index);
            Invalidate();
        }

        private Size originalPanelSize;

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            isAdvanced = !isAdvanced;
            if (isAdvanced)
            {
                // Stretch panel
                originalPanelSize = OffsetPanel.Size;
                defaultRowHeight = rowHeight;
                // We need to make it taller by: rowHeight * (MidiChannels.Length-1)
                // Ish.  A little more complicated than that.
                int numRows = 2; // Default
                if (isAdvanced)
                    numRows = 1 + trackSelectionManager.MidiChannels.Where((c) => c.Active).Count();
                OffsetPanel.Size = new Size(originalPanelSize.Width, originalPanelSize.Height + rowHeight * (numRows - 2));
                //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
                //ScrollBarForcer.Visible = true;
            }
            else
            {
                // Shrink it back
                // Uhhh it isn't setting
                // Let's try this
                OffsetPanel.Size = new Size(originalPanelSize.Width, originalPanelSize.Height);
                //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
                //ScrollBarForcer.Visible = false;
            }
            Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackSelectionManager.NoteOffset = 0;
            foreach (var channel in trackSelectionManager.MidiChannels)
            {
                trackSelectionManager.MidiChannelOffsets[channel.Id] = 0;
            }
            Invalidate();
        }

        private void textBoxNotesForChords_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxNotesForChords.Text, out int v))
            {
                trackSelectionManager.NumChords = v;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                 trackSelectionManager.SaveTrackManager(saveFileDialog1.FileName);
            }
        }

        private void instrumentsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If it's already the instrument we have as our property
            // Then don't re-set the values
            int currentInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (currentInstrument != instrumentsBox.SelectedIndex)
            {
                ConfigManager.SetProperty(PropertyItem.Instrument, instrumentsBox.SelectedIndex.ToString());
                Instrument target = (Instrument)instrumentsBox.SelectedItem;

                bool soundEffects = !target.Name.StartsWith("Mordhau", true, System.Globalization.CultureInfo.InvariantCulture);
                ConfigManager.SetProperty(PropertyItem.SoundEffects, soundEffects.ToString());

                ConfigManager.SetProperty(PropertyItem.LowestNoteId, target.LowestSentNote.ToString());

                ConfigManager.SetProperty(PropertyItem.AvaliableNoteCount, target.NoteCount.ToString());

                ConfigManager.SetProperty(PropertyItem.NoteCooldown, target.NoteCooldown.ToString());

                ConfigManager.SetProperty(PropertyItem.LowestPlayedNote, target.LowestPlayedNote.ToString());

                ConfigManager.SetProperty(PropertyItem.ForbidsChords, target.ForbidsChords.ToString());

                mainForm.OnInstrumentChanged(currentInstrument);
            }
        }
    }
}
