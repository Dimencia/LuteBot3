# LuteBot 3
Originally forked from https://github.com/MontyLeGueux/Lutebot-2
I had some issues with Git and ended up making a new repo, but this is still mostly his code


## Usage Instructions
Download the latest Release from https://github.com/Dimencia/LuteBot3/releases

Change your Mordhau Console keybinding to PageDown (inside the game).  Load a song, and press + while Mordhau is focused and it should start/pause playing

Further support is provided at the [Bard's Guild Discord](https://discord.gg/4xnJVuz)

![Track Filtering Example](https://github.com/Dimencia/LuteBot3/blob/master/LutebotExample1.PNG)

![Guild Library Example](https://github.com/Dimencia/LuteBot3/blob/master/LutebotExample2.PNG)

## New Features v Lutebot-2
* Rust compatibility
* Searching/filtering/downloading of any song in the Bards Guild Midi Library - Bard's Guild: https://discord.gg/4xnJVuz
* Automatic search/download of BitMidi when searching for a song not in the Library
* Ability to visually align songs or individual channels to match your instrument
* Song preferences are now injected into the midi file, so the mid can be distributed easily with settings intact
* Automatic drum removal for Mordhau
* TimeSync option using an NTP server to attempt to synchronize playing with a friend
* QOL tweaks, such as a stop hotkey, not unloading song on stop, song reload button

## Rust Features v MidiPlayer
* Drum mapping so any midi with a glockenspiel track is automatically converted (usually)
* Note Duplicate Filtering - cleans up tracks to play with Rust's instrument limitations
* Note conversion - like Mordhau, moves all notes into the instrument's range
* Song/channel filtering and alignment


### Guild Library
You can find this new button at the top of the screen in yellow

This allows you to search the entire Bard's Guild Library, courtesy of the Official [Bard's Guild](https://discord.gg/4xnJVuz)

Songs are automatically downloaded when selected from this library

### Track Alignment
New feature lets you align channels individually to the instrument range, as shown in the image

This helps you accentuate or soften parts of the song based on where you put them on the instrument range

These changes are all saved with the midi

### Midi Embed
Starting with v2.41, Track Filtering selections and track alignment data are all stored inside the .mid file itself

Previously it was stored in an xml file, and when distributing mids, you had to either send two files or tell them how to filter it

Now, once you find the perfect settings for a mid, you can send that adjusted mid to someone and have them load it directly

All mids with embedded data are still compatible with any other midi players

### Rust
Using this with Rust requires you to download and install [LoopMidi](https://www.tobias-erichsen.de/software/loopmidi.html) or equivalent midi loopback device.  Simply create a port in LoopMidi with any name, and then in Lutebot, select that port as the Output device.  Then select your instrument and make sure Rust Mode is enabled in Settings

Rust is interesting because there already exists a good solution for playing mids there - LoopMidi and MidiPlayer.  Unfortunately, depending on the octaves, some songs don't translate well, and Rust doesn't do any remapping to make the song fit.  It also tends to send delayed-duplicate notes when the midi has notes on top of eachother.  

So, this now solves those problems, as well as providing the same customization options and Guild Library to Mordhau and Rust users

All instruments are supported and available to select from a dropdown box
