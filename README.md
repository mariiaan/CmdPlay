# CmdPlay
Lets you play videos in command line using ASCII-Art.

This program uses ffmpeg for video decoding and NAudio for audio playback.
Thus it needs a reference to NAudio and a copy of ffmpeg.exe in the working directory.

Compiled binary for windows: https://github.com/mariiaan/CmdPlay/releases

# Known issues / weaknesses
No buffering, long load times

# License
The program code including all cs files and the CmdPlay binary are licensed under the MIT license. This does NOT apply to ffmpeg or NAudio.
FFmpeg, including its source code and binaries are licensed under the GNU Lesser General Public License (LGPL) version 2.1.
NAudio, including its source code and binaries are licensed under the Microsoft Public License (Ms-PL).

# Compiling
Create a new .NET framework (or .NET core) console application. Add a reference to NAudio (dotnet package cli or nuget package manager). Use the generated project to compile CmdPlay.cs
