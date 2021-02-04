using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NAudio;
using NAudio.Wave;

namespace CmdPlay
{
    class Program
    {
        const string brightnessLevels = " .-+*wGHM#&%";

        static void Main(string[] args)
        {
            string inputFilename;
            if (args.Length == 0) /* Ask user manually if no parameters specified */
            {
                Console.Write("Input File: ");
                inputFilename = Console.ReadLine().Replace("\"", "");
            }
            else /* Otherwise use first argument */
            {
                inputFilename = args[0];
            }

            Console.WriteLine(  "------------------------------\n" +
                                "            Controls          \n" +
                                "      Space - Play / Pause    \n" +
                                "           Esc - Exit         \n" +
                                "------------------------------\n");
            ConsoleColor originalForegroundColor = Console.ForegroundColor; /* Preserve the old colours to print warning message */
            ConsoleColor originalBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black; /* Contrast: Red on black */
            Console.WriteLine("NOTE: Do not resize the window starting from now! (Resize before program init)");
            Console.ForegroundColor = originalForegroundColor; /* Reset old colours */
            Console.BackgroundColor = originalBackgroundColor;

            Console.WriteLine("[INFO] Please wait.. Processing..");
            Console.WriteLine("[INFO] Step 1 / 4: Cleaning up...");

            if(Directory.Exists("tmp"))
            {
                if(Directory.Exists("tmp\\frames\\"))
                {
                    Directory.Delete("tmp\\frames\\", true);
                }
                Directory.CreateDirectory("tmp\\frames\\");
                if(File.Exists("tmp\\audio.wav"))
                {
                    File.Delete("tmp\\audio.wav");
                }
            }
            else
            {
                Directory.CreateDirectory("tmp\\");
                Directory.CreateDirectory("tmp\\frames\\");
            }

            int targetFrameWidth = Console.WindowWidth - 1;
            int targetFrameHeight = Console.WindowHeight - 2;

            Console.WriteLine("[INFO] Step 2 / 4: Extracting frames...");
            Process ffmpegProcess = new Process(); /* Launch ffmpeg process to extract the frames */
            ffmpegProcess.StartInfo.FileName = "ffmpeg.exe";
            ffmpegProcess.StartInfo.Arguments = "-i \"" + inputFilename + "\" -vf scale=" + 
                                    targetFrameWidth + ":" + targetFrameHeight + " tmp\\frames\\%0d.bmp";

            ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpegProcess.Start();
            Console.WriteLine("[INFO] Waiting for ffmpeg.exe to finish...");
            ffmpegProcess.WaitForExit();

            Console.WriteLine("[INFO] Step 3 / 4: Extracting audio...");
            ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = "ffmpeg.exe";
            ffmpegProcess.StartInfo.Arguments = "-i \"" + inputFilename + "\" tmp\\audio.wav";
            ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpegProcess.Start();
            Console.WriteLine("[INFO] Waiting for ffmpeg.exe to finish...");
            ffmpegProcess.WaitForExit();

            Console.WriteLine("[INFO] Step 4 / 4: Converting to ascii... (This can take some time!)");
            Console.Write("-> [PROGRESS] [0  %] [                    ]");
            int currentCursorHeight = Console.CursorTop;
            List<string> frames = new List<string>();

            int frameCount = Directory.GetFiles("tmp\\frames", "*.bmp").Length;
            int frameIndex = 1;
            while(true)
            {
                string filename = "tmp\\frames\\" + frameIndex.ToString() + ".bmp";
                if(!File.Exists(filename))
                {
                    break;
                }
                StringBuilder frameBuilder = new StringBuilder();
                using (Bitmap b = new Bitmap(filename))
                {
                    for(int y = 0; y < b.Height; y++)
                    {
                        for(int x = 0; x < b.Width; x++)
                        {
                            int dIndex = (int)(b.GetPixel(x, y).GetBrightness() * brightnessLevels.Length);
                            if(dIndex < 0)
                            {
                                dIndex = 0;
                            }
                            else if(dIndex >= brightnessLevels.Length)
                            {
                                dIndex = brightnessLevels.Length - 1;
                            }
                            frameBuilder.Append(brightnessLevels[dIndex]);
                        }
                        frameBuilder.Append("\n");
                    }
                }
                frames.Add(frameBuilder.ToString());
                frameIndex++;

                int percentage = (int)(frameIndex / (float)frameCount * 100);
                Console.SetCursorPosition(15, currentCursorHeight);
                Console.Write(percentage.ToString());
                Console.SetCursorPosition(22, currentCursorHeight);
                for(int i = 0; i < percentage / 5; i++)
                {
                    Console.Write("#");
                }
            }

            AudioFileReader reader = new AudioFileReader("tmp\\audio.wav");
            WaveOutEvent woe = new WaveOutEvent();
            woe.Init(reader);
            Console.WriteLine("\n\nPress return to play!");
            Console.ReadLine();
            woe.Play();

            while(true)
            {
                float percentage = woe.GetPosition() / (float)reader.Length;
                int frame = (int)(percentage * frameCount);
                if(frame >= frames.Count)
                    break;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine(frames[frame]);

                if(Console.KeyAvailable)
                {
                    ConsoleKey pressed = Console.ReadKey().Key;
                    switch(pressed)
                    {
                        case ConsoleKey.Spacebar:
                            {
                                if (woe.PlaybackState == PlaybackState.Playing)
                                    woe.Pause();
                                else woe.Play();

                                break;
                            }
                        case ConsoleKey.Escape:
                            {
                                Console.WriteLine("Done. Press any key to close");
                                Console.ReadKey();

                                return;
                            }
                    }
                }
            }
            Console.WriteLine("Done. Press any key to close");
            Console.ReadKey();
        }
    }
}
