using LuteBot.Config;
using LuteBot.Core;
using LuteBot.TrackSelection;
using LuteBot.UI.Utils;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

            this.SizeChanged += TrackSelectionForm_SizeChanged;

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

            ChannelsListBox.ContextMenuStrip = contextMenuStrip1;
            TrackListBox.ContextMenuStrip = contextMenuStrip2;

        }

        private void TrackSelectionForm_SizeChanged(object sender, EventArgs e)
        {
            // Resize/move the two listboxes
            // Minx and max x are available via OffsetPanel, I think.  Or maybe, probably, panel1
            // Some arbitrary gap between them; 8 is what we have at start, that seems fine
            ChannelsListBox.Size = new Size(panel1.Width / 2 - 4, ChannelsListBox.Size.Height);
            TrackListBox.Size = new Size(panel1.Width / 2 - 4, TrackListBox.Size.Height);
            TrackListBox.Location = new Point(panel1.Location.X + panel1.Width / 2 + 4, TrackListBox.Location.Y);
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



        private Panel pianoPanel = new Panel();

        private void TrackSelectionForm_Load(object sender, EventArgs e)
        {
            //panel1.AutoScroll = false;
            //panel1.VerticalScroll.Enabled = true;
            //panel1.VerticalScroll.Visible = true;
            //panel1.AutoScroll = true;
            //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
            //panel1.AutoScroll = false;
            panel1.VerticalScroll.Enabled = true;
            panel1.VerticalScroll.Visible = true;
            panel1.HorizontalScroll.Enabled = true;
            panel1.HorizontalScroll.Visible = true;
            //panel1.AutoScroll = false;
            //this.MouseMove += new MouseEventHandler((o, ev) => OffsetPanel.Invalidate());
            panel1.Resize += panel1_Resize;

            //pianoPanel = new Panel();
            pianoPanel.AutoSize = false;
            pianoPanel.Height = panel1.Height - 2;
            pianoPanel.Width = pianoWidth + 1;
            this.Controls.Add(pianoPanel);
            pianoPanel.Location = new Point(panel1.Location.X + 1, panel1.Location.Y + 1);
            pianoPanel.BringToFront();

            pianoPanel.Paint += new PaintEventHandler((o, ev) => DrawPiano(ev.Graphics));

            var scrollHandler = new ScrollEventHandler((o, ev) =>
            {
                if (ev.ScrollOrientation == ScrollOrientation.VerticalScroll)
                {
                    DrawPiano(pianoPanel.CreateGraphics());
                    pianoPanel.Update();
                }
                //DrawPianoRoll(OffsetPanel.CreateGraphics());
                //OffsetPanel.Update();

                // If it was a horizontal scroll, we should trim the channelRects we use for mouse checks

                // Realistically, I should sort channelRects when I'm done making it, and then I can do some ez logic to avoid reiterating
                // Like find the largest one in our culled set, find it in the original set, and only check ones after that
                // Which means it should be flattened?  That's awkward though... 

                // Hmm.  It's really the drawing that spikes it, though this took us from 500mb to like 400
                // We should really be invalidating only the visible section when we update it probably
                // If they horizontal scroll while a hoverChannel is up, then we'd want to blank them and redraw, but otherwise it'd be fine to do
                else
                {
                    SetVisibleNotes();
                    OffsetPanel.Refresh();
                }
            });

            OffsetPanel.MouseWheel += new MouseEventHandler((o, ev) =>
            {
                if (hoverChannels.Count > 1)
                {
                    if (ev.Delta >= 120)
                    {
                        // Take whatever's at spot 0 and put it at the end
                        var first = hoverChannels.First();
                        hoverChannels.RemoveAt(0);
                        hoverChannels.Add(first);
                    }
                    else if (ev.Delta <= -120)
                    {
                        var first = hoverChannels.Last();
                        hoverChannels.RemoveAt(hoverChannels.Count - 1);
                        hoverChannels.Insert(0, first);
                    }

                    // Wish we could make it not scroll... docs say the delta is cumulative, and should scroll when it's +/-120
                    // It then should scroll SystemInformation.MouseWheelScrollLines per 120 deltas of movement
                    (ev as HandledMouseEventArgs).Handled = true; // Does that do it? Yep
                    OffsetPanel.Refresh();
                }
                else
                {
                    if (ModifierKeys == Keys.Control)
                    {
                        // Zoom in/out on time scale
                        if (ev.Delta >= 120)
                        {
                            tickLength *= 2;
                        }
                        else if (ev.Delta <= -120)
                        {
                            tickLength /= 2;
                            if (tickLength <= 0)
                                tickLength = 0.01f;
                        }
                        // TODO: Scroll horizontally to where the mouse was when 'zooming'
                        // TODO: Apply upper and lower limits
                        ResetRowSize();
                        SetVisibleNotes(true);
                        (ev as HandledMouseEventArgs).Handled = true;
                        OffsetPanel.Refresh();
                        // Something is still not right here; we get notes cut off at both left and right side
                        // Considering how we build them, they can't have an X < 0 so that doesn't add up unless it's just
                        // visible notes not being correct; maybe the new scroll value isn't updated yet...?  But should be as soon as they scroll...

                        // I guess, at least on the left, we're not accounting for pianoWidth
                    }
                    else
                    {
                        if (panel1.VerticalScroll.Visible && panel1.VerticalScroll.Enabled)
                        {
                            var args = new ScrollEventArgs(ScrollEventType.SmallIncrement, 0, ScrollOrientation.VerticalScroll); // We aren't really gonna use these values...
                            scrollHandler(null, args);
                        }
                        else
                        {
                            var args = new ScrollEventArgs(ScrollEventType.SmallIncrement, 0, ScrollOrientation.HorizontalScroll); // We aren't really gonna use these values...
                            scrollHandler(null, args);
                        }
                    }
                }
            }); // The OffsetPanel lets us override the scroll, and the panel1 triggers after it occurs 
            panel1.MouseWheel += new MouseEventHandler((o, ev) =>
            {
                DrawPiano(pianoPanel.CreateGraphics());
                pianoPanel.Update();
            });
            panel1.Scroll += scrollHandler;


            //OffsetPanel.Left = pianoWidth;

            //OffsetPanel.HorizontalScroll.Enabled = true;
            OffsetPanel.AutoScroll = false;

            OffsetPanel.Paint += new PaintEventHandler(DrawGraphics);
            OffsetPanel.Resize += OffsetPanel_Resize;
            OffsetPanel.MouseDown += OffsetPanel_MouseDown;
            OffsetPanel.MouseUp += OffsetPanel_MouseUp;
            OffsetPanel.MouseLeave += OffsetPanel_MouseLeave;
            OffsetPanel.MouseCaptureChanged += OffsetPanel_MouseLeave; // These should do the same thing
            OffsetPanel.MouseMove += OffsetPanel_MouseMove;
            OffsetPanel.MouseEnter += OffsetPanel_MouseEnter;

            TrackSelectionForm_SizeChanged(null, null); // Make it resize the listboxes on load
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                foreach (var note in selectedNotes)
                    note.active = false;

                selectedNotes.Clear();
                OffsetPanel.Refresh();
            }
            else if(keyData == Keys.Insert)
            {
                foreach (var note in selectedNotes)
                    note.active = true;

                selectedNotes.Clear();
                OffsetPanel.Refresh();
            }
            return base.ProcessCmdKey(ref msg, keyData); 
        }

        // This mouse stuff is going to suck
        private Point dragStart;
        private bool dragging;
        private Rectangle draggableRect;
        private int startOffset;
        private Dictionary<int, Color> channelColors = new Dictionary<int, Color>();
        private Dictionary<int, Color> originalChannelColors = new Dictionary<int, Color>();
        private MidiChannelItem dragTarget;
        private List<int> hoverChannels = new List<int>();

        private Point lastMousePosition = new Point(0, 0);
        private int lastScrollAmount = 0;

        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        private void OffsetPanel_MouseEnter(object sender, EventArgs e) => OffsetPanel.Refresh();
        private void OffsetPanel_MouseMove(object sender, MouseEventArgs e)
        {
            lastScrollAmount = panel1.VerticalScroll.Value;
            if (dragging)
            {
                // Set offset to (mousePos - dragStart)/columnWidth/12 (round to octaves...?)
                int multiplier = (e.Location.Y < dragStart.Y ? 1 : -1); // Preserve sign

                //if (isAdvanced)
                //{
                if (trackSelectionManager.MidiChannels.ContainsKey(dragTarget.Id))
                {
                    var channel = trackSelectionManager.MidiChannels[dragTarget.Id];


                    int oldOffset = channel.offset;
                    channel.offset = startOffset + (int)Math.Round((double)(Math.Abs(dragStart.Y - e.Location.Y) * multiplier / pianoRowHeight) / 12) * 12;
                    if (channel.offset != oldOffset)
                    {
                        // Simulate a mouse position at where it should now draw...?
                        // If we're scrolled all the way up or down, we don't want it to move though

                        // Nah.  If the highest note changed is the only time we don't do this
                        int oldHighest = maxNote;
                        ReloadNotes(true);
                        if (maxNote == oldHighest)
                            lastMousePosition = new Point(dragStart.X, dragStart.Y - (channel.offset - startOffset) * pianoRowHeight);
                        //OffsetPanel.Refresh();
                        //pianoPanel.Refresh();
                    }
                }
                //}
                //else
                //{
                //    int oldOffset = trackSelectionManager.NoteOffset;
                //    trackSelectionManager.NoteOffset = startOffset + (int)Math.Round(((GetDistance(dragStart, e.Location) * multiplier / columnWidth)) / 12) * 12;
                //    if (trackSelectionManager.NoteOffset != oldOffset)
                //        OffsetPanel.Refresh();
                //}
            }
            else if(rectDragging)
            {
                lastMousePosition = e.Location;
                if (panel1.HorizontalScroll.Value > Int16.MaxValue || e.X < 0) // It's a uint16, can overflow easily, this gives us about double the range but could be a problem still... 
                {
                    lastMousePosition = new Point((int)e.X & 0xffff, e.Y);
                }
                OffsetPanel.Refresh();
            }
            else
            {
                lastMousePosition = e.Location;
                if (panel1.HorizontalScroll.Value > Int16.MaxValue || e.X < 0) // It's a uint16, can overflow easily, this gives us about double the range but could be a problem still... 
                {
                    lastMousePosition = new Point( (int)e.X & 0xffff, e.Y);
                }
                List<int> hoveringChannels = new List<int>();
                // Can I really iterate every note rect for collisions every move?  I can try anyway

                // Well, answer is kinda, it works but spikes the memory usage while moving mouse
                // We will cull them elsewhere

                foreach (var note in visibleNotes)
                {
                    if (note.pianoRect.Contains(lastMousePosition))
                    {
                        hoveringChannels.Add(note.channel);
                    }
                }
                // Problem: I want to draw them based on the order of hoverChannels so the one they have selected is drawn on top
                // But doing so makes it flip the order everytime it finds them again
                // What's the best workaround?  

                // Obvious answer is to flip from using first, use last instead somewhere... 
                // or if they were already hovering last tick, flip it?
                var refresh = false;
                //if (hoverChannels.Count > 0)
                    hoveringChannels.Reverse();
                if (hoveringChannels.Count != hoverChannels.Count || !hoveringChannels.SequenceEqual(hoverChannels))
                {
                    refresh = true;
                }
                hoverChannels = hoveringChannels;
                if (refresh)
                    OffsetPanel.Refresh();

            }

            

            //OffsetPanel.Refresh(); // See if this is laggy
            // Yeah a little

        }
        private void OffsetPanel_MouseLeave(object sender, EventArgs e)
        {
            dragging = false;
            rectDragging = false;
        }
        private void OffsetPanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;

            if (rectDragging)
            {
                rectDragging = false;
                // TODO: If we stopped at negative sizes, do I need to handle that?
                Rectangle selectionArea = new Rectangle(Math.Min(rectDragStart.X, e.Location.X), Math.Min(rectDragStart.Y, e.Location.Y), Math.Abs(e.Location.X - rectDragStart.X), Math.Abs(e.Location.Y - rectDragStart.Y));
                foreach(var note in visibleNotes)
                {
                    if (selectionArea.LooseContains(note.pianoRect))
                    {
                        // Damn, we don't have a way to access the Note
                        // Well, I could and probably should make channelRects and culledChannelRects be a List of some new structure that links it all together
                        // Or array, or something that's more enumerable than a dict

                        // Really it'd just be a MidiNote, and a Rectangle.  What if I added the rectangle... right onto the notes?  and we just use allNotes?  That seems smart
                        // Then when we draw notes or scroll, we just keep track of which ones are currently visible..?
                        // Scrolling may end up slow then, if we iterate them all... 
                        // In order to consider partial updates, they should be sorted by tick and then channel
                        // We can then use a modified binary search to find ones within our tick range

                        // Unless I plan to draw each channel on its own graphics and overlay them all... but I would still need that logic there too

                        // Alternatively I could find out the tick and channel via reversing positioning logic, and notes are made to lookup via tick

                        // Well anyway now we have one
                        selectedNotes.Add(note);
                    }
                }
                OffsetPanel.Refresh();
            }

            rectDragging = false;
        }
        List<MidiNote> selectedNotes = new List<MidiNote>();
        bool rectDragging = false;
        Point rectDragStart = new Point(0, 0);
        private void OffsetPanel_MouseDown(object sender, MouseEventArgs e)
        {
            //if (isAdvanced)
            //{
            //foreach (var kvp in channelRects)
            //{
            //    //foreach (var rect in kvp.Value)
            //    for (int i = kvp.Value.Count - 1; i >= 0; i--) // Iterate backwards cuz that's the order they are visible in
            //    {
            //        var rect = kvp.Value[i];
            //        if (rect.Contains(e.Location))
            //        {
            //            
            //        }
            //    }
            //}
            //}
            //// Check if their mouse is inside the draggableRect
            //else if (draggableRect.Contains(e.Location))
            //{
            //    dragging = true;
            //    dragStart = e.Location;
            //    startOffset = trackSelectionManager.NoteOffset;
            //}

            // We already checked all these collisions
            if (hoverChannels.Count > 0)
            {
                var targetChannel = hoverChannels.First();
                if (trackSelectionManager.MidiChannels.ContainsKey(targetChannel))
                {
                    var channel = trackSelectionManager.MidiChannels[targetChannel];
                    dragging = true;
                    dragStart = e.Location;
                    //if (isAdvanced)
                    startOffset = channel.offset;
                    dragTarget = channel;
                    hoverChannels.Clear();
                    hoverChannels.Add(targetChannel);
                    return;
                }
            }
            else
            {
                selectedNotes.Clear();
                rectDragging = true;
                rectDragStart = e.Location;
                OffsetPanel.Refresh();
            }
        }

        public void RefreshOffsetPanel() => OffsetPanel.Refresh();

        internal class ChannelData
        {
            internal string name { get; set; }
            internal Rectangle bgLabelRect { get; set; }
            internal Rectangle labelRect { get; set; }
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            if (panel1.HorizontalScroll.Visible)
                pianoPanel.Height = panel1.Height - 2 - SystemInformation.HorizontalScrollBarHeight;
            else
                pianoPanel.Height = panel1.Height - 2;
            SetVisibleNotes();
            pianoPanel.Refresh();
            OffsetPanel.Refresh();
            this.Refresh();
        }

        private void OffsetPanel_Resize(object sender, EventArgs e)
        {
            //OffsetPanel.Refresh();
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



        // So this is really annoying because I'm gonna need floating things at the left and bottom
        // But they flicker like mad and look stupid when scrolling
        //... which means I really need to implement my own scrollbars... 

        // Or maybe I could have the left and bottom regions not inside the scroll panel, and manually redraw them based on that scroll...
        // Might look awkward with the bottom one being below the bar, but, we could put it ontop but not inside... 

        private float tickLength = 0.25f; // IDK, 1px per tick sounds ok to me
        private int pianoWidth = 60;
        private int pianoRowHeight = 10;

        private int minNote = 0;
        private int maxNote = 127;
        private int numNotes = 128;
        private MidiNote[] allNotes = new MidiNote[0];
        private MidiNote[] visibleNotes = new MidiNote[0];
        int maxHeight = 128 * 10;
        float maxWidth = 0f;

        int numNotesBelow = 12; // We display a full octave above and below the highest/lowest notes
        // And need to know this to calculate where to put them

        Brush selectedNotesBrush = new SolidBrush(Color.Blue);

        private void DrawPianoRoll(Graphics g)
        {
            // So for a piano roll, the height is always static, 127 notes worth
            // The width may change depending on length

            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            g.Clear(OffsetPanel.BackColor);

            // Then... draw every note.  For now we don't even care about colors, just see if I even can




            // Draw grid lines behind
            for (int i = 0; i < numNotes; i++)
            {
                int y = maxHeight - pianoRowHeight * (i + 1); // Since we draw down, we bring it up one for the rect
                int translatedNote = minNote + i - numNotesBelow;
                // Draw piano line across the entire content - but the highlight should be at the bottom of a note, not top... 
                if (translatedNote % 12 == 0)
                {
                    g.DrawLine(gridHighlightPen, 0, y + pianoRowHeight, maxWidth, y + pianoRowHeight);
                }
                else
                {
                    g.DrawLine(gridPen, 0, y + pianoRowHeight, maxWidth, y + pianoRowHeight);
                }
            }

            // I guess draw allNotes for now, maybe later only draw visibleNotes but that may get flickery and maybe more expensive to redraw anytime we scroll?
            // Let's find out.
            //foreach (var note in allNotes.OrderBy(n => n.channel).OrderBy(n => hoverChannels.Contains(n.channel) ? hoverChannels.Count-hoverChannels.IndexOf(n.channel) : -1))

            // TODO: find out why in one rare case, we don't have a channel that it says we should have.  I think it's related to loading old files
            foreach(var note in visibleNotes.OrderBy(n => hoverChannels.Contains(n.channel) ? hoverChannels.Count - hoverChannels.IndexOf(n.channel) : -1).Where(n => trackSelectionManager.MidiChannels.ContainsKey(n.channel)))
            {

                var channel = trackSelectionManager.MidiChannels[note.channel];
                var channelRect = note.pianoRect;
                if (channelRect == Rectangle.Empty)
                    channelRect = GetNoteRect(note);
                    

                if (!channelColors.ContainsKey(channel.Id))
                {
                    channelColors[channel.Id] = Color.FromArgb(random.Next(0, 200), random.Next(0, 200), random.Next(0, 200));
                    originalChannelColors[channel.Id] = channelColors[channel.Id];
                }

                if (selectedNotes.Contains(note))
                {
                    g.FillRectangle(selectedNotesBrush, channelRect);
                    g.DrawRectangle(Pens.White, channelRect);
                }
                // Draw outline, black if not hovered
                else if (hoverChannels.Contains(note.channel))
                {
                    var baseColor = channelColors[channel.Id];
                    if (note.active)
                    {
                        g.FillRectangle(new SolidBrush(channelColors[channel.Id]), channelRect);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(50, baseColor.R, baseColor.G, baseColor.B)), channelRect);
                    }
                    g.DrawRectangle(Pens.White, channelRect);
                }
                else if (note.active)
                {
                    g.FillRectangle(new SolidBrush(channelColors[channel.Id]), channelRect);
                    g.DrawRectangle(Pens.Black, channelRect);
                }
                else
                {
                    // Draw it with alpha
                    var baseColor = channelColors[channel.Id];
                    g.FillRectangle(new SolidBrush(Color.FromArgb(50, baseColor.R, baseColor.G, baseColor.B)), channelRect);
                    // And no outline
                    //g.DrawRectangle(Pens.Black, channelRect);
                }

            }

            //int numHovers = 0;
            // Now for each hoverChannel, draw a label for it near lastMousePosition
            //foreach(var hoverChannel in hoverChannels)
            for (int numHovers = 0; numHovers < hoverChannels.Count; numHovers++)
            {
                // Get them in reverse order though so the top one is the one that gets selected
                var hoverChannel = hoverChannels[numHovers];

                var channel = trackSelectionManager.MidiChannels[hoverChannel];
                var labelSize = g.MeasureString(channel.Name, labelFont);

                Rectangle channelLabelRect = new Rectangle((int)(lastMousePosition.X - labelSize.Width/2 - 2), (int)(lastMousePosition.Y - (pianoRowHeight*1.5f) - ((labelSize.Height+10)*(numHovers+1))), (int)(labelSize.Width+4), (int)(labelSize.Height + 4));
                Rectangle channelLabelBgRect = new Rectangle(channelLabelRect.X + 1, channelLabelRect.Y + 1, channelLabelRect.Width + 1, channelLabelRect.Height + 1);

                g.FillRectangle(shadowBrush, channelLabelBgRect);
                g.FillRectangle(labelBgBrush, channelLabelRect);
                // If this is the first one of multiple, give it an outline or something
                if (numHovers == 0 && hoverChannels.Count > 1)
                    g.DrawRectangle(new Pen(channelColors[channel.Id], 4f), channelLabelRect);
                else
                    g.DrawRectangle(new Pen(channelColors[channel.Id], 2f), channelLabelRect);
                g.DrawString(channel.Name, labelFont, labelBrush, channelLabelRect);
            }


            if (rectDragging)
            {
                // Draw rect
                g.DrawRectangle(Pens.Blue, Math.Min(rectDragStart.X, lastMousePosition.X), Math.Min(rectDragStart.Y, lastMousePosition.Y), Math.Abs(lastMousePosition.X - rectDragStart.X), Math.Abs(lastMousePosition.Y - rectDragStart.Y));
            }

            //DrawPiano(g);
        }



        private Brush instrumentPianoBrush = new SolidBrush(Color.FromArgb(225, 255, 225));
        // Because of issues with buffering, we may have OffsetPanel contain a left docked panel for drawing the piano on
        private void DrawPiano(Graphics g)
        {
            // The piano roll should always float the left side, which means we need to know how far we're scrolled
            // This seems to be in pixels which is great
            //int pianoX = panel1.HorizontalScroll.Value;
            int pianoX = 0;

            g.Clear(Color.White);

            //float maxWidth = pianoWidth + trackSelectionManager.Player.GetLength() * tickLength;
            float maxWidth = pianoWidth;

            // Fill in a white rectangle where the piano goes
            //g.FillRectangle(Brushes.White, pianoX, 0, pianoWidth, maxHeight);

            // Fill in a greenish rectangle where the instrument goes
            
            int noteCount = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount); //-1 because if noteCount is 60 and start is 0, the high note is 59
            int highest = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId) + ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote) + noteCount - 1;

            // Convert that highest note to a y using the same logic as everything else...
            //int translatedHighest = minNote + highest - numNotesBelow;
            int translatedi = highest - minNote + numNotesBelow;
            int instrumentY = maxHeight - pianoRowHeight * (translatedi + 1) - panel1.VerticalScroll.Value;
            int instrumentHeight = noteCount * pianoRowHeight;
            g.FillRectangle(instrumentPianoBrush, pianoX, instrumentY, pianoWidth, instrumentHeight);

            // A line on the right, then one for each note
            g.DrawLine(gridPen, pianoX + pianoWidth, maxHeight, pianoX + pianoWidth, 0);

            for (int i = 0; i < numNotes; i++)
            {
                int translatedNote = minNote + i - numNotesBelow;

                int y = maxHeight - pianoRowHeight * (i + 1);

                if (y + pianoRowHeight > panel1.VerticalScroll.Value && y < panel1.VerticalScroll.Value + pianoPanel.Height)
                {
                    y = y - panel1.VerticalScroll.Value;
                    // Draw piano line across the entire content - but the highlight should be at the bottom of a note, not top... 
                    if (translatedNote % 12 == 0)
                    {
                        g.DrawLine(gridHighlightPen, pianoX, y + pianoRowHeight, maxWidth, y + pianoRowHeight);
                        var text = $"C{translatedNote / 12}";
                        var measured = g.MeasureString(text, labelFont);
                        g.DrawString(text, labelFont, labelBrush, pianoX + pianoWidth - measured.Width, y + pianoRowHeight / 2 - measured.Height / 2);
                    }
                    else
                    {
                        g.DrawLine(gridPen, pianoX, y + pianoRowHeight, pianoX + pianoWidth, y + pianoRowHeight);
                    }
                }
            }
        }

        // And a bottom-docked panel for a timeline




        private void DrawGraphics(object sender, PaintEventArgs e)
        {
            DrawPianoRoll(e.Graphics);
            return;
            /*
            List<ChannelData> data = new List<ChannelData>();

            int numRows = 2; // Default
            if (isAdvanced)
                numRows = 1 + trackSelectionManager.MidiChannels.Values.Where((c) => c.Active).Count();
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
            var activeMidiChannels = trackSelectionManager.MidiChannels.Values.Where(c => c.Active);
            if (activeMidiChannels.Count() > 0)
            {
                minNote = activeMidiChannels.Min((c) => c.lowestNote + c.offset + trackSelectionManager.NoteOffset);
                maxNote = activeMidiChannels.Max((c) => c.highestNote + c.offset + trackSelectionManager.NoteOffset);
            }

            if (isAdvanced)
            {
                channelRects = new Dictionary<MidiChannelItem, List<Rectangle>>();
                // Draw each MidiChannel as a row with its own offset
                int currentY = rowHeight;
                foreach (var channel in trackSelectionManager.MidiChannels.Values)
                {
                    if (channel.Active)
                    {
                        var offset = trackSelectionManager.NoteOffset + channel.offset;
                        Rectangle channelRect = new Rectangle(xPad + offset * columnWidth + channel.lowestNote * columnWidth, currentY + 1,
                            (channel.highestNote - channel.lowestNote) * columnWidth, rowHeight - 1);
                        if (!channelRects.ContainsKey(channel))
                            channelRects[channel] = new List<Rectangle>();
                        channelRects[channel].Add(channelRect);

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
                int note = ((x - xPad) / columnWidth) - centerOffset * 12;

                if (isAdvanced)
                { // For brevity and cuz I think it's safe here, assume the dicts are populated
                    int labelY = height + padding + labelHeight;
                    foreach (var kvp in channelRects)
                    {
                        if (trackSelectionManager.MidiChannels.ContainsKey(kvp.Key.Id))
                        {
                            var channel = trackSelectionManager.MidiChannels[kvp.Key.Id];
                            int offset = trackSelectionManager.NoteOffset + channel.offset;
                            int midiLowest = channel.lowestNote + offset;
                            int midiHighest = channel.highestNote + offset;
                            if (note >= midiLowest && note <= midiHighest)
                            {
                                // We can consider showing labels...
                                // So then, (note - midiLowest) is how many notes out we are from midiLowest
                                // So midiLowest + (note - midiLowest) is our effective note here
                                int effectiveNote = channel.lowestNote + (note - midiLowest);
                                if (effectiveNote % 12 == 0)
                                {
                                    g.DrawString($"C{effectiveNote / 12}", gridFont, new SolidBrush(channelColors[kvp.Key.Id]), x - xPad, labelY);
                                }
                            }
                            labelY += labelHeight;
                        }
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
                    foreach (var rect in kvp.Value)
                    {
                        //dpi/96 should give us a scalar for making the boxes wider.  Unsure if it should be X or Y, but one of those

                        // We're wildly guessing here... But it's about 7px per char.  Need 8 to prevent trimming
                        //int channelLabelWidth = (int)((8 * kvp.Key.Name.Length)*(g.DpiX / 96f));
                        int channelLabelWidth = (int)g.MeasureString(kvp.Key.Name, labelFont).Width + 4;

                        Rectangle channelLabelRect = new Rectangle(rect.X + rect.Width / 2 - channelLabelWidth / 2, rect.Y + rowHeight / 2 - 9, channelLabelWidth, 18);
                        Rectangle channelLabelBgRect = new Rectangle(channelLabelRect.X + 1, channelLabelRect.Y + 1, channelLabelRect.Width + 1, channelLabelRect.Height + 1);

                        g.FillRectangle(shadowBrush, channelLabelBgRect);
                        g.FillRectangle(labelBgBrush, channelLabelRect);
                        g.DrawString(kvp.Key.Name, labelFont, labelBrush, channelLabelRect);
                    }
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
            */
        }

        private void ResetRowSize()
        {
            minNote = 0;
            maxNote = 127;

            if (allNotes.Length > 0)
            {
                minNote = allNotes.Min(n => n.note + trackSelectionManager.NoteOffset + trackSelectionManager.MidiChannels[n.channel].offset);
                maxNote = allNotes.Max(n => n.note + trackSelectionManager.NoteOffset + trackSelectionManager.MidiChannels[n.channel].offset);
            }

            numNotes = maxNote - minNote + 1 + numNotesBelow * 2;// To make it inclusive?  i.e. if min 1 and max 2, 2-1 = 1, but there's really 2 notes... 
            // And then also give some leeway of one octave in either direction...

            maxHeight = pianoRowHeight * numNotes;
            maxWidth = pianoWidth + trackSelectionManager.Player.GetLength() * tickLength;
            //private float tickLength = 0.1f; // IDK, 1px per tick sounds ok to me
            //private int pianoWidth = 60;
            //private int pianoRowHeight = 20;
            OffsetPanel.Size = new Size(Math.Max(originalPanelSize.Width, (int)(trackSelectionManager.Player.GetLength() * tickLength + pianoWidth + xPad)), pianoRowHeight * numNotes);
            if (panel1.HorizontalScroll.Visible)
                pianoPanel.Height = panel1.Height - 2 - SystemInformation.HorizontalScrollBarHeight;
            else
                pianoPanel.Height = panel1.Height - 2;
            return;
            if (isAdvanced) // Reset size for this new track
            {
                int numRows = 2; // Default
                if (isAdvanced)
                    numRows = 1 + trackSelectionManager.MidiChannels.Values.Where((c) => c.Active).Count();

                OffsetPanel.Size = new Size(originalPanelSize.Width, originalPanelSize.Height + defaultRowHeight * (numRows - 2));
                rowHeight = (OffsetPanel.Height - labelHeight * numRows - padding) / numRows;
                //ScrollBarForcer.Location = new Point(OffsetPanel.Location.X, OffsetPanel.Location.Y + OffsetPanel.Height);
            }
        }

        private void SetVisibleNotes(bool forceRegenerate = false)
        {
            // A note is visible if its tickNumber*tickLength+min(length+1) > horizontalScroll, and tickNumber*tickLength < horizontalScroll+panel1.Width
            // Errrr.  I mean if it's rect.X+rect.Width > horizontalScroll and rect.X < horizontalScroll+panel1.Width

            // allNotes is sorted by tickNumber, which determines the X as shown above
            // So we should binary search to find an index that is within our window, then iterate up and down until we get to ones that aren't

            // Alright, define a targetRect to get notes within.  
            //var notes = GetNotesWithin(panel1.DisplayRectangle); // This probably works? Nope

            var targetRect = new Rectangle(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value, panel1.Width, panel1.Height);
            var notes = GetNotesWithin(targetRect, forceRegenerate);

            // It raises a question though.  I probably want to get notes a screen or two ahead and behind, so scrolling isn't flickery
            // But I probably don't want to put them in visibleNotes for the sake of checking collisions... well, we'll see how this looks when doing just strictly visible ones
            visibleNotes = notes.OrderBy(n => n.channel).ToArray();
        }





        // This is getting really fucking annoying
        // I can't fucking get this binary fucking search to fucking do anything right
        // It always fucking quits early but, in a way that makes me think the sort is wrong

        // Basically, esp when zoomed out, you can find horizontal scroll values that make some channels disappear some of their notes
        // Not all of them - the highest and lowest seem to stay.  Which means it must not be sorted by X value like I need it to be
        // Though I have no fucking clue how that could happen, I very explicitly sort by tickNumber which directly translates to X value and has more precision
        // And I fucking explicitly never use fucking note length, always max length, to prevent issues

        // It's really fucking annoying right now.  I don't fucking know, and I can't fucking use the debug to even fucking look at the list
        // Because the fucking values we need are thousands of entries in


        private IEnumerable<MidiNote> GetNotesWithin(Rectangle targetRect, bool forceRegenerate)
        {
            
            if (allNotes.Length == 0)
                return new MidiNote[0];
            /*
            int validIndex = -1;
            int startPosition = (int)(allNotes.Length / 2f);
            int adder = (int)Math.Ceiling(startPosition / 2f);//Ceil so we always have at least 1
            do
            {
                var note = allNotes[startPosition];

                if (forceRegenerate || note.pianoRect == Rectangle.Empty)
                    note.pianoRect = GetNoteRect(note); // Unsure if we should do this here but why not

                if (note.pianoRect.X <= targetRect.X + targetRect.Width)
                {
                    if (note.pianoRect.X + (int)Math.Max(maxNoteLength*tickLength,1) >= targetRect.X)
                    //if (targetRect.LooseContains(note.pianoRect))
                    {
                        validIndex = startPosition;
                    }
                    else
                    {
                        // the note, if it had max length, is below our minimum X - go up
                        int oldStart = startPosition;
                        startPosition += adder;
                        adder = (int)(adder / 2f); // Ceiling, round, or floor?  Floor, because 1/2 would return 0 and then we stop looking if we just moved one and didn't find anything.  Anything else will do fine
                        if (oldStart == startPosition || startPosition >= allNotes.Length) // Found nothing, we're at 0 or out of bounds
                        {
                            return new MidiNote[0];
                        }
                    }
                }
                else
                {
                    // Note starts above our maximum X - go down
                    int oldStart = startPosition;
                    startPosition -= adder;
                    adder = (int)(adder / 2f); // Ceiling, round, or floor?  Floor, because 1/2 would return 0 and then we stop looking if we just moved one and didn't find anything.  Anything else will do fine
                    if (oldStart == startPosition || startPosition >= allNotes.Length) // Found nothing, we're at 0 or out of bounds
                    {
                        return new MidiNote[0];
                    }


                    //int differenceMin = targetRect.X + targetRect.Width - note.pianoRect.X;// Tells us what direction go 
                    // We either divide startPosition by 2, or multiply it by 1.5... which is kinda weird but ok
                    //int oldStart = startPosition;
                    // If it's the same as we have now, or out of bounds, we abort
                    // I guess we have to round, otherwise a length of 3, starts at 1, *1.5 is still 1.  /2 is still 1 with rounding though... 
                    // So, if going up, take ceiling, and if down just truncate to int, which guarantees it changes unless it was 0 to begin with

                    // I think our fundamental issue is comparing to something that we're not sorting on, i.e. length
                    // so if for example, something is left of our X but not long enough to be shown
                    // we assume we need to go toward more positive tick values... or... wait... 
                    // We split it in half, and check the middle.  If it's above target, we check the lower half, and below we check the upper half
                    // Each time we split that also in half, checking the middle, and doing the same... sounds right I think?
                    //
                    // But no, that fundamental problem is real.  If that happens, yet all the notes that should display are lower tick, then we get nothing
                    // Maybe our sort should be based on minimum distance from current scroll

                    // Maybe the answer is to do it twice, find the one that's above our minX, and then find one below our maxX?
                    // Or maybe we only pick a direction based on whether or not the note X, which is what we sorted on, does a thing...
                    // If the x of our note is above our rectX, we know it's half-valid
                    // If it then is outside our width, that doesn't mean we know anything yet... do we?
                    
                    // well we do as long as it's just the X we're dealing with... 
                    // Or rather.  If the x of our note is below our rect's max, it's half valid
                    // If that x + maxWidth is below our minX, we know to go up
                    // If it's not, we have no idea.   And tbh probably just mark it valid and let the other logic iterate

                    // If the x of our not is not below our rect's max, we know to go down


                    //if (differenceMin >= 0)
                    //    startPosition += adder;
                    //else
                    //    startPosition -= adder;
                    //adder = (int)(adder / 2f); // Ceiling, round, or floor?  Floor, because 1/2 would return 0 and then we stop looking if we just moved one and didn't find anything.  Anything else will do fine
                    //if (oldStart == startPosition || startPosition >= allNotes.Length) // Found nothing, we're at 0 or out of bounds
                    //{
                    //    return new MidiNote[0];
                    //}
                }
                
            }
            while (validIndex == -1);

            // If we got here, we have a validIndex, and need to iterate every note above and below, adding until we get to ones that aren't on screen
            List<MidiNote> result = new List<MidiNote>();
            result.Add(allNotes[validIndex]);
            // Generate NoteRects for them all if they don't exist
            for(int i = validIndex+1; i < allNotes.Length; i++)
            {
                var note = allNotes[i];
                if (forceRegenerate || note.pianoRect == Rectangle.Empty)
                    note.pianoRect = GetNoteRect(note);
                if (note.pianoRect.X <= targetRect.X + targetRect.Width)
                {
                    if (note.pianoRect.X + (int)Math.Max(maxNoteLength * tickLength, 1) >= targetRect.X)
                    {
                        result.Add(note);
                    } // If this isn't true, we keep going anyway
                }
                //else
                //    break;
            }
            for (int i = validIndex - 1; i >= 0; i--)
            {
                var note = allNotes[i];
                if (forceRegenerate || note.pianoRect == Rectangle.Empty)
                    note.pianoRect = GetNoteRect(note);
                if (note.pianoRect.X <= targetRect.X + targetRect.Width)
                {
                    if (note.pianoRect.X + (int)Math.Max(maxNoteLength * tickLength, 1) >= targetRect.X)
                    {
                        result.Add(note);
                    }
                    //else
                    //    break; // If maxLength couldn't get into the frame, we stop from going down further
                }
            }
            */

            // This was fucking annoying and isn't even that bad at performance if I do a straight linear one
            List<MidiNote> result = new List<MidiNote>();
            foreach(var note in allNotes)
            {
                if (forceRegenerate || note.pianoRect == Rectangle.Empty)
                    note.pianoRect = GetNoteRect(note);
                if (targetRect.LooseContainsX(note.pianoRect))
                    result.Add(note);
            }
            return result;
        }

        

        private Rectangle GetNoteRect(MidiNote note)
        {
            int x = (int)(note.tickNumber * tickLength) + pianoWidth;
            int width = (int)Math.Max((note.length * tickLength), 1);
            int finalNote = note.note + trackSelectionManager.MidiChannels[note.channel].offset + trackSelectionManager.NoteOffset;
            int y = maxHeight - ((finalNote - minNote + 1 + numNotesBelow) * pianoRowHeight);
            int height = pianoRowHeight;

            var rect = new Rectangle(x, y, width, height);
            return rect;
        }

        private int maxNoteLength = 0;
        private void ReloadNotes(bool forceRefresh = false)
        {
            allNotes = trackSelectionManager.MidiChannels.Values.Where(c => c.Active).SelectMany(c => c.tickNotes.Values).SelectMany(c => c).Where(n => trackSelectionManager.MidiTracks[n.track].Active).OrderBy(n => n.tickNumber).ThenBy(n => n.channel)
                .ToArray();
            // Give it an arbitrary order to make things consistent for draw order

            // And we need the maximum length for a 'binary' sort algo...
            if (allNotes.Length > 0)
                maxNoteLength = allNotes.Max(n => n.length);
            else
                maxNoteLength = 0;
            ResetRowSize();
            SetVisibleNotes(forceRefresh);
            pianoPanel.Refresh();
            OffsetPanel.Refresh();
        }

        public void InitLists()
        {
            SuspendLayout();
            textBoxNotesForChords.Text = trackSelectionManager.NumChords.ToString();
            textBoxNotesForChords.Enabled = !ConfigManager.GetBooleanProperty(PropertyItem.ForbidsChords);

            TrackListBox.Items.Clear();
            ChannelsListBox.Items.Clear();

            foreach (MidiChannelItem channel in trackSelectionManager.MidiChannels.Values.OrderBy(c => c.Id))
            {
                ChannelsListBox.Items.Add(channel, channel.Active);
            }
            foreach (TrackItem track in trackSelectionManager.MidiTracks.Values.OrderBy(c => c.Id))
            {
                TrackListBox.Items.Add(track, track.Active);
            }

            ChannelsListBox.DisplayMember = "Name";
            TrackListBox.DisplayMember = "Name";

            instrumentsBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);

            ReloadNotes(true);

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
            pianoPanel.Refresh();
            OffsetPanel.Refresh();
        }

        private void TrackChangedHandler(object sender, EventArgs e)
        {
            if(!LuteBotForm.skipUI)
                InitLists();
        }

        private void TrackSelectionForm_Closing(object sender, FormClosingEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.TrackSelectionPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
        }

        private void ChannelListBox_ItemChecked(object sender, ItemCheckEventArgs e)
        {
            trackSelectionManager.ToggleChannelActivation(!(e.CurrentValue == CheckState.Checked), (ChannelsListBox.Items[e.Index] as MidiChannelItem).Id);
            //Invalidate();
            //OffsetPanel.Refresh();
            //ResetRowSize();
            //InitLists();
            ReloadNotes();
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

        private void TrackListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            trackSelectionManager.ToggleTrackActivation(!(e.CurrentValue == CheckState.Checked), (TrackListBox.Items[e.Index] as TrackItem).Id);
            //InitLists();
            //ResetRowSize();
            //Invalidate();
            //OffsetPanel.Refresh();
            ReloadNotes();
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
                    numRows = 1 + trackSelectionManager.MidiChannels.Values.Where((c) => c.Active).Count();
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

            pianoPanel.Refresh();
            OffsetPanel.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackSelectionManager.NoteOffset = 0;
            foreach (var channel in trackSelectionManager.MidiChannels.Values)
            {
                channel.offset = 0;
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

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuStrip = ((sender as ToolStripMenuItem).Owner) as ContextMenuStrip;
            if (menuStrip == contextMenuStrip1)
            {
                foreach (var channel in trackSelectionManager.MidiChannels.Values)
                {
                    if (channel.Id != 9)
                        channel.Active = true;
                }
            }
            else if (menuStrip == contextMenuStrip2)
            {
                foreach (var track in trackSelectionManager.MidiTracks.Values)
                    track.Active = true;
            }
            trackSelectionManager.ResetRequest();
            InitLists();
            //ResetRowSize();
            //Invalidate();
            //OffsetPanel.Refresh();

        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuStrip = ((sender as ToolStripMenuItem).Owner) as ContextMenuStrip;
            if (menuStrip == contextMenuStrip1)
            {
                foreach (var channel in trackSelectionManager.MidiChannels.Values)
                    channel.Active = false;
            }
            else if (menuStrip == contextMenuStrip2)
            {
                foreach (var track in trackSelectionManager.MidiTracks.Values)
                    track.Active = false;
            }
            trackSelectionManager.ResetRequest();
            InitLists();
            //ResetRowSize();
            //Invalidate();
            //OffsetPanel.Refresh();
        }

        private void selectInverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuStrip = ((sender as ToolStripMenuItem).Owner) as ContextMenuStrip;
            if (menuStrip == contextMenuStrip1)
            {
                foreach (var channel in trackSelectionManager.MidiChannels.Values)
                    if (channel.Id != 9)
                        channel.Active = !channel.Active;
            }
            else if (menuStrip == contextMenuStrip2)
            {
                foreach (var track in trackSelectionManager.MidiTracks.Values)
                    track.Active = !track.Active;
            }
            trackSelectionManager.ResetRequest();
            InitLists();
            //ResetRowSize();
            //Invalidate();
            //OffsetPanel.Refresh();
        }
    }

    public class CustomBufferedPanel : Panel
    {

        public CustomBufferedPanel() : base()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint &
                          ControlStyles.UserPaint &
                          ControlStyles.OptimizedDoubleBuffer,
                          true);
        }
    }
}
