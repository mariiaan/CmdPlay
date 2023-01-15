# CmdPlay
Lets you play videos in command line using ASCII-Art.

This program uses ffmpeg for video decoding and NAudio for audio playback.
Thus it needs a reference to NAudio and a copy of ffmpeg.exe in the working directory.

[Compiled binary for windows](https://github.com/obvMellow/CmdPlay/releases)

[Demonstration video](https://youtu.be/6zs6S_I5gH8?t=16)

## Known issues / weaknesses
No buffering, long load times

## License
The program code including all cs files and the CmdPlay binary are licensed under the MIT license. This does NOT apply to FFmpeg or NAudio.
FFmpeg, including its source code and binaries are licensed under the GNU Lesser General Public License (LGPL) version 2.1.
NAudio, including its source code and binaries are licensed under the Microsoft Public License (Ms-PL).

## Installation

### Automatic installation
- Install the compiled binary from [here](https://github.com/obvMellow/CmdPlay/releases)
- Unzip the file
- Run CmdPlay.exe

### Manual Installation

- Install Visual Studio with .Net desktop development workload
- Clone this repository
- Open "CmdPlay.sln"
- Select the "Release" configuration and run

If you want to run it again without recompiling run the exe in the bin/Release.
