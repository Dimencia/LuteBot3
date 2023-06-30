namespace LuteBot.UI.ScrollPanel;

public class ZoomableScrollPanel : Panel
{
    // Provides reference to zoom levels, and probably scrollbar info and maybe handling/drawing

    public float ZoomX { get; set; } = 1f;
    public float ZoomY { get; set;} = 1f;
}
