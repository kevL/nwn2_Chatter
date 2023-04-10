Chatter
1.0.2.0 - 2023 april 9
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

The data is laid out in a table with cols
1. Voice
2. Resref
3. Strref
4. Talk-entry

Rightclick on the resref or strref slots opens a text input. The resref shall be
ASCII alphanumeric or underscore characters only, or blank. The strref shall be
0 or a positive integer less than 33,554,432 (0..16,777,215 points into
Dialog.Tlk while 16,777,216..33,554,431 points into a custom .Tlk file). A blank
strref will be interpreted as -1 or no strref.

[Ctrl]+Leftclick on a resref slot opens a dialog to browse to and play an
audiofile.

[Ctrl]+Rightclick on a resref slot opens a dialog to browse to and open an NwN2
zipped data file, from which an audiofile can be played and, if desired, its
name inserted into the current slot.

Playing audio likely works on Windows machines only.

On the menu is an option to load a Talkfile - if loaded the text of the strrefs
should appear in the table.

Included in this Chatter package is a LAME 3.100 executable for decoding
MPEG-audio layer 3 (MP3) to PCM-audio (WAV) playback.

Chatter uses a parsed down version of SharpZipLib for decompressing zipped files
in Neverwinter Nights 2's installation/data folder. See the sourcecode for
licensing and attribution.
