using LuteBot.TrackSelection;
using LuteBot.UI.ScrollPanel;

namespace LuteBot.UI.PianoRoll;

public class NoteCollectionPanel : Panel
{
    public MidiChannelItem MidiChannel { get; set; }
    public NoteDisplaySettings DisplaySettings { get; set; }

    // If we assume that our panel's size is the entire length of the song
    // then the parent can just resize us and we're dynamic
    // ... but we need the TSM to know what that is
    // So let's just get the PianoRollComponent

    public NoteCollectionPanel(MidiChannelItem midiChannel, PianoRollDisplaySettings displaySettings)
    {
        MidiChannel = midiChannel;
        DisplaySettings = new NoteDisplaySettings { PianoRollSettings = displaySettings, Visible = (displaySettings.ShowChannels && !midiChannel.IsTrack) || (!displaySettings.ShowChannels && midiChannel.IsTrack) };
        
        DisplaySettings.NoteColor = Color.FromArgb(BitConverter.ToInt32(BitConverter.GetBytes(colorIndex[midiChannel.Id % colorIndex.Length])));

        //Internet pretty much says there are 30 or so colors that people can differentiate.
        //  and that I should just hardcode that list.  That's valid.
        //  We'll just do %30, shouldn't be that many tracks anyway

        // Now create NoteComponents, and add them to ourselves at the right position...
        this.SuspendLayout();
        foreach(var kvp in midiChannel.TickNotes)
        {
            foreach (var note in kvp.Value)
                AddNote(new NoteComponent(note, DisplaySettings));
        }
        this.ResumeLayout();
    }

    private void AddNote(NoteComponent note)
    {
        note.Visible = true;
        this.Controls.Add(note);
    }


    private static readonly uint[] colorIndex = new uint[]
    {
        0xFF00FF00,
        0xFF0000FF,
        0xFFFF0000,
        0xFF01FFFE,
        0xFFFFA6FE,
        0xFFFFDB66,
        0xFF006401,
        0xFF010067,
        0xFF95003A,
        0xFF007DB5,
        0xFFFF00F6,
        0xFF774D00,
        0xFF90FB92,
        0xFF0076FF,
        0xFFD5FF00,
        0xFFFF937E,
        0xFF6A826C,
        0xFFFF029D,
        0xFFFE8900,
        0xFF7A4782,
        0xFF7E2DD2,
        0xFF85A900,
        0xFFFF0056,
        0xFFA42400,
        0xFF00AE7E,
        0xFF683D3B,
        0xFFBDC6FF,
        0xFF263400,
        0xFFBDD393,
        0xFF00B917,
        0xFF9E008E,
        0xFF001544,
        0xFFC28C9F,
        0xFFFF74A3,
        0xFF01D0FF,
        0xFF004754,
        0xFFE56FFE,
        0xFF788231,
        0xFF0E4CA1,
        0xFF91D0CB,
        0xFFBE9970,
        0xFF968AE8,
        0xFFBB8800,
        0xFF43002C,
        0xFFDEFF74,
        0xFF00FFC6,
        0xFFFFE502,
        0xFF620E00,
        0xFF008F9C,
        0xFF98FF52,
        0xFF7544B1,
        0xFFB500FF,
        0xFF00FF78,
        0xFFFF6E41,
        0xFF005F39,
        0xFF6B6882,
        0xFF5FAD4E,
        0xFFA75740,
        0xFFA5FFD2,
        0xFFFFB167,
        0xFF009BFF,
        0xFFE85EBE
    };
}
