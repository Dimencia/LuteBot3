using LuteBot.TrackSelection;
using System.Windows.Forms;

namespace LuteBot.UI.PianoRoll;

public class NoteComponent : Panel
{
    public MidiNote Note { get; set; }
    public NoteDisplaySettings DisplaySettings { get; set; }
    private bool isVisible { get; set; }


    public NoteComponent(MidiNote note, NoteDisplaySettings displaySettings)
    {
        Note = note;
        DisplaySettings = displaySettings;
        this.Paint += new PaintEventHandler(onPaint);
        isVisible = displaySettings.Visible;
    }

    // OnDraw, fill with our track or channel's color
    private void onPaint(object sender, PaintEventArgs e)
    {
        // Setting these now might end up a frame behind, and maybe could cause perf issues with having to update every tick because it thinks it moved, we'll see
        this.Width = (int)Math.Min(1, DisplaySettings.PianoRollSettings.PixelsPerSecond * Note.timeLength); // Don't make any notes invisible
        this.Height = DisplaySettings.PianoRollSettings.NoteHeight;
        this.Location = new Point((int)(DisplaySettings.PianoRollSettings.PixelsPerSecond * Note.startTime.TotalSeconds), DisplaySettings.PianoRollSettings.NoteHeight * (128 - Note.note));

        var g = e.Graphics;
        if (DisplaySettings.Visible)
        {
            g.Clear(DisplaySettings.NoteColor);
        }
        else if (isVisible) // If we were visible, but now we're not
        {
            // If we shouldn't display, set a transparent image as our background maybe
            var bitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bitmap.MakeTransparent();
            g.DrawImage(bitmap, 0, 0);
            isVisible = false;
        }
        // Otherwise do nothing if we weren't previously visible, and we're still not
    }
}
