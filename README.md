# CmdPlay
Lets you play videos in command line using ASCII-Art.

This program uses ffmpeg for video decoding and NAudio for audio playback.
Thus it needs a reference to NAudio and a copy of ffmpeg.exe in the working directory.

https://github.com/mariiaan/CmdPlay/releases

# Known issues / weaknesses
No buffering, long load times

# Compiling
Create a new .NET framework (or .NET core) console application. Add a reference to NAudio (dotnet package cli or nuget package manager). Use the generated project to compile CmdPlay.cs
