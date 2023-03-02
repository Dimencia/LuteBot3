# LuteBot 3
Originally forked from https://github.com/MontyLeGueux/Lutebot-2
I had some issues with Git and ended up making a new repo, but this is still forked from his code

Now installs lutemod and the mod loader upon prompt

Big thanks to cswic and his modloader, which he allowed me to package here https://mordhau.mod.io/clientside-mod-autoloader

And Monty, for LuteBot2, and LuteMod which he allowed me to package here https://mordhau.mod.io/lutemod


## Usage Instructions
Download the latest Release from https://github.com/Dimencia/LuteBot3/releases

If prompted to install LuteMod, click yes.

Open game, verify modloader menu showing LuteMod is loaded.  If not, try options -> Install LuteMod

In-game with an instrument in hand, press Kick to open the partition list, press number keys to select a partition, and press feint to pause/play

Further support is provided at the [Bard's Guild Discord](https://discord.gg/4xnJVuz)

## LuteMod Controls

Lutemod's base controls are based on Mordhau controls, so that you can change them

* **Kick**: Open Menu, next page when Menu is open
* **Arrow Keys Left/Right**: Change menu page
* **Ctrl+Arrow Keys Left/Right**: Change menu page 5 pages at a time
* **Equipment Select 0-9**: Select song or mirror target from Menu
* **Feint (not Parry/Feint)**: Play/Pause
* **Numpad 1**: Toggle Voice (Warning: Use sparingly.  Mods may ban you for using it in a way that makes it hard to hear combat)

Other controls are unfortunately hardcoded (including Numpad1) because there was no more Mordhau controls that aren't being used for important things

* **Arrow Key Up**: Pitch Up (Currently held instrument)
* **Arrow Key Down**: Pitch Down (Currently held instrument)
* **Arrow Key Left**: FluteCutting -1 Octave (Flute ignores the lowest {FluteCutting} octaves when copying from lute)
* **Arrow Key Right**: FluteCutting +1 Octave (Flute ignores the lowest {FluteCutting} octaves when copying from lute)
* **Ctrl+Arrow Key Up**: Toggle 'Duplication' (Currently held instrument) (Doubles all notes played on this instrument, these notes play on other instruments)
* **Ctrl+Arrow Key Down**: Toggle 'Copying' (Currently held instrument type) (Copies all notes from other instrument types to the equipped instrument type)


## New Features v Lutebot-2

LuteMod

* Plays all equipped instruments at once
* Notes first try to go to the currently held instrument.  A held flute will also usually get longer notes than not-held flutes
* If an instrument is busy when it needs to play a note, another equipped instrument of the same type (lute/flute) may be used
* Can play extremely complex songs, extremely quickly
* Offers the ability to 'Mirror' other bards (play the same thing they're playing)
  * Note: If your ping, or the target's ping, is higher than 100, avoid doing this - it just sounds bad
    * The person doing the mirroring does not hear any delay, but everyone else on the server does, including the person being mirrored
  * Press 'Kick' to open the LuteMod menu
  * Mirror-able bards should be at the beginning of the list
  * Press 'Feint' to pause/play as normal
* Offers hotkeys for modifying songs on the fly: See 'LuteMod Controls', above (all of these persist after death but reset on a new game)
* Allows you to define specific tracks for each of Lute and Flute (LuteBot handles this)
* Performs transposition for out of range notes (so LuteBot doesn't have to)

LuteBot

* LuteMod Installer
* Export MIDI to LuteMod Partitions
* Searching/filtering/downloading of any song in the Bards Guild Midi Library - Bard's Guild: https://discord.gg/4xnJVuz
* Ability to visually align songs, channels, or tracks to fit the instrument
* Piano roll and ability to view and add/remove individual notes in Track Selection
* Song preferences are now injected into the midi file, so the mid can be distributed easily with settings intact
* Automatic drum removal for Mordhau
* Partition editing, import, export
* Self update detection and install


### Guild Library
You can find this new button at the top of the screen in yellow

This allows you to search the entire Bard's Guild Library, courtesy of the Official [Bard's Guild](https://discord.gg/4xnJVuz)

Songs are automatically downloaded when selected from this library

![Guild Library Example](https://github.com/Dimencia/LuteBot3/blob/master/LutebotExample2.PNG)


### Midi Embed
Starting with v2.41, Track Filtering selections and track alignment data are all stored inside the .mid file itself

Previously it was stored in an xml file, and when distributing mids, you had to either send two files or tell them how to filter it

Now, once you find the perfect settings for a mid, you can send that adjusted mid to someone and have them load it directly

All mids with embedded data are still compatible with any other midi players

### Rust
Rust is no longer supported, but should still work, though it may require older versions.  Requires LoopMidi

LuteBot offers transposition to the Rust instrument, and mapping from MIDI Drums to Rust Drums.  But for most purposes, any midi player should work fine for Rust


### Like my work?
If you want to show appreciation for LuteBot and/or LuteMod, by request I'm now accepting donations at https://www.paypal.com/donate/?hosted_button_id=3PSW26CRK3CKQ 

But, I am an employed software dev, I'm not struggling, and just a thanks in Discord is fine, really.  

Also note that Bardlord is hosting the Guild Library, which has server costs, so he deserves your money more than I do - but doesn't yet have a donation link or I'd put it here.  That's `SpaceBardlord LaserLutemaster#0048` on Discord

And also note that both LuteBot and LuteMod were first created by Monty; I've only been updating them after he stopped working on them.  He deserves some love too - `Monty#2962` on Discord, or his github is linked at the top of the readme
