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
        OutputDevice,
        Instrument,

        //keybinds
        Play,
        Next,
        Previous,
        Ready,
        OpenConsole,
        SynchronizedPlay,

        //misc
        MordhauInputIniLocation,
        UserSavedConsoleKey,
        LastPlaylistLocation,
        LastSoundBoardLocation,
        LastMidiDeviceUsed,
        LastMidiLowBoundUsed,
        Server,

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
