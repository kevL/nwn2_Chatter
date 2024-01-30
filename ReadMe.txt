Chatter
1.2.1.0 - 2024 january 30
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
1. Voice (static)
2. Resref (editable)
3. Strref (editable)
4. Talk-entry in talktable per 3. Strref

A resref shall be ASCII alphanumeric or underscore characters only, or blank. A
strref shall be 0 or a positive integer less than 33,554,432 (0..16,777,215
points into Dialog.Tlk while 16,777,216..33,554,431 points into a custom .Tlk
file). A blank strref will be interpreted as -1 or no strref.

-----------
RESREF Slots

[Ctrl]+leftclick opens a resref input box.
[Shift]+leftclick opens a dialog to browse for and insert a resref.
[Ctrl+Shift]+leftclick opens a dialog to browse for and open an NwN2 zipped data
file, from which a resource (audio) can be played and, if desired, its resref
inserted into the current slot.

rightclick opens a context menu with available actions:
- input resref
- cut
- copy
- paste
- delete
- browse for file
- browse /Data zipfile
- browse and Play file

[Ctrl]+rightclick opens a dialog to browse for and play a resource (audio).

-----------
STRREF Slots

[Ctrl]+leftclick opens a strref input box.

rightclick opens a context menu with available actions:
- input strref
- cut
- copy
- paste
- delete

----
The edit functions use the Windows Clipboard. Paste will be disabled if the
contents of the clipboard are invalid for the slot-type.

Playing audio is supported only for a few standards of PCM-WAV, ADPCM, and
BMU/MP3 formats and likely works on Windows machines only. Generally the
standard should be 44.1 kHz 16 bit Mono; sample rate and bit depth for BMU/MP3
is more relaxed but at present requires an MP3 decoder installed on your
machine (usually there is one).

On the menu is an option to load a Talkfile - if loaded the text of any strrefs
should appear at the right of the table.

----
Encode to BMU

Under "encode" on the menubar is a tool that allows the user to encode (or
re-encode) an audiofile to BMU/MP3. Input can be BMU/MP3 or Wave PCM (44.1 kHz
16 bit mono) or ADPCM (44.1 kHz 4 bit mono). Output should be compatible with
NwN Voice audio. I found several voicefiles that appear to be okay but throw an
exception in the toolset when played - use this tool to fix them or to compress
original audiofiles for NwN ...

The output file will be written to the same directory as the input file, with
incremental digits appended to the filename if necessary to prevent overwriting
an already existing file.

----
CREDITS

Chatter uses a parsed down version of NAudio for decoding BMU/MP3 and ADPCM
audiofiles to PCM-audio (WAV) playback. See the sourcecode for licensing and
attribution.

Chatter uses a parsed down version of SharpZipLib for decompressing zipped files
in Neverwinter Nights 2's installation/Data folder. See the sourcecode for
licensing and attribution.
