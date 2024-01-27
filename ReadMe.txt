Chatter
1.1.1.0 - 2024 january 26
kevL's

SoundSetFile (SSF) editor for Neverwinter Nights 2

source - C# w/ .NET 3.5
https://github.com/kevL/nwn2_Chatter


A SoundSetFile stores a list of resrefs and strrefs that make up a creature's
voiceset. The resrefs are pointers to audiofiles and the strrefs are pointers
into Dialog.Tlk (or a custom talkfile) for corresponding strings to display (if
relevant). There can be 49 entries (standard) or 51 entries (extended) - the
latter is used only by NwN2 as far as I'm aware.

Chatter supports both SSF version 1.0 (16-char resref) and 1.1 (32-char resref)
specifications.

The data is laid out in a table with four cols:
1. Voice
2. Resref
3. Strref
4. Talk-entry

A resref shall be ASCII alphanumeric or underscore characters only, or blank. A
strref shall be 0 or a positive integer less than 33,554,432 (0..16,777,215
points into Dialog.Tlk while 16,777,216..33,554,431 points into a custom .Tlk
file). A blank strref will be interpreted as -1 or no strref.

-----------
RESREF Slot

LMB opens a text input box.

RMB opens a context menu with available actions:
- input (text)
- cut
- copy
- paste
- delete
- browse for file
- browse /Data zipfile
- browse and Play file

LMB+[Ctrl] opens a dialog to browse for and insert a file.

LMB+[Shift] opens a dialog to browse for and play an audiofile.

RMB+[Ctrl] on a resref slot opens a dialog to browse to and open an NwN2 zipped
data file, from which an audiofile can be played and, if desired, inserted into
the current slot.

-----------
STRREF Slot

LMB opens an integer input box.

RMB opens a context menu with available actions:
- input (integer)
- cut
- copy
- paste
- delete

----
The edit functions use the Windows Clipboard. Paste will be disabled if the
contents of the clipboard are invalid for the slot-type.

Playing audio likely works on Windows machines only.

On the menu is an option to load a Talkfile - if loaded the text of any strrefs
should appear at the right of the table.

----
CREDITS

Chatter uses a parsed down version of NAudio for decoding BMU/MP3 and ADPCM
audiofiles to PCM-audio (WAV) playback. See the sourcecode for licensing and
attribution.

Chatter uses a parsed down version of SharpZipLib for decompressing zipped files
in Neverwinter Nights 2's installation/Data folder. See the sourcecode for
licensing and attribution.
