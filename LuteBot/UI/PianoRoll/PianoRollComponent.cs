using LuteBot.TrackSelection;
using LuteBot.UI.ScrollPanel;

namespace LuteBot.UI.PianoRoll;

public class PianoRollComponent : ZoomableScrollPanel
{
    public TrackSelectionManager TrackSelectionManager { get; set; }
    private List<NoteCollectionPanel> noteCollections = new List<NoteCollectionPanel>();
    public PianoRollDisplaySettings PianoRollSettings { get; set; }

    public PianoRollComponent(TrackSelectionManager trackSelectionManager, bool showChannels = true) : base()
    {
        PianoRollSettings = new PianoRollDisplaySettings { ShowChannels = showChannels };
        TrackSelectionManager = trackSelectionManager;
        foreach (var track in TrackSelectionManager.MidiTracks.Values)
            noteCollections.Add(new NoteCollectionPanel(track, PianoRollSettings));
        foreach (var channel in TrackSelectionManager.MidiChannels.Values)
            noteCollections.Add(new NoteCollectionPanel(channel, PianoRollSettings));
        // Components are created, now add them to this panel
        // ... requires us to have a width and height, which isn't necessarily set yet
    }

    // I think I need to do this almost entirely at a note level...
        // Adding collections for all channels and tracks is just duplicating the same notes
        // The position of the notes is determined by both, and doesn't change depending on which is being displayed

        // The only thing that changes is the color, and the names and tooltips
        //  and which config thing is affected when you shift it


            // Sigh... what if... all of lutebot became at the note level
              // When they disable a track or channel, it just disables/enables all notes on it.  When they shift, it shifts the note
              // Each note knows whether it plays for lute or flute or both (still want to get rid of the 'both' concept...)

            // It'd probably take a lot of work to keep things performant unless I keep the constructs, but just move the settings

                // ... Other adjustments I should make while I'm rewriting
                // - Don't store notes by tick; in order by tick maybe, which should still allow culling and whatnot, but no need for this dictionary I never use
                //   - Just a list of notes should be fine.  
                // - Massively simplify track and channel structures, if I can.  The notes have the important data.  

            // Hmm... but.  Part of the idea of channel/track encapsulation is that when someone changes a track offset, I don't have to iterate every note, it's ref
            //   So it really does make sense to have a structure that has shared settings like that

            // In which case it also makes sense to have a parent panel that contains the child panels, ie the track or channel, and that parent panel can be moved up/down 



    public void AddComponents()
    {
        this.SuspendLayout();
        foreach(var collection in noteCollections)
        {
            collection.Height = this.Height;
            collection.Width = this.Width;
            collection.Location = new Point(0, 0);
            collection.Visible = true;
            this.Controls.Add(collection);
        }
        this.ResumeLayout();
    }
}
