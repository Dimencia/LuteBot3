namespace LuteBot.Config
{
    /// <summary>
    /// The enum listing the names of properties stored in the configuration.
    /// </summary>
    /// 
    public enum PropertyItem
    {
        None,
        //window auto open features
        SoundBoard,
        PlayList,
        TrackSelection,
        OnlineSync,
        LiveMidi,

        //Settings
        SoundEffects,
        ConsoleOpenMode,
        NoteConversionMode,
        LowestNoteId,
        AvaliableNoteCount,
        NoteCooldown,
        DebugMode,

        //keybinds
        Play,
        Next,
        Previous,
        Ready,
        OpenConsole,

        //misc
        MordhauInputIniLocation,
        UserSavedConsoleKey,
        LastPlaylistLocation,
        LastSoundBoardLocation,
        LastMidiDeviceUsed,
        LastMidiLowBoundUsed,

        //window positions
        MainWindowPos,
        SoundBoardPos,
        PlayListPos,
        TrackSelectionPos,
        LiveMidiPos,
        VirtualKeyboardBinds,
        LiveMidiListen
    }
}
