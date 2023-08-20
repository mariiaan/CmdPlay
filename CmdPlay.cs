using FFMpegCore;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CmdPlay
{



    class CmdPlay
    {

        const string brightnessLevels0 = " .-+*wGHM#&%@";
        const string brightnessLevels1 = "          `.-':_,^=;><+!rc*/z?sLTv)J7(|F{C}fI31tlu[neoZ5Yxya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@██████████████";

        private static readonly object Lock = new object();

        static void Main(string[] args)
        {

        string inputFilename;

            Console.WriteLine("Remember, Window size will affect the resolution of the video!!");
            Console.Write("Choose if you are using high or low resolution, 1. low(suggested)  2.high:");

            int choose = int.Parse(Console.ReadLine());
            string brightnessLevels = brightnessLevels0;

            if (choose == 2)
            {
                brightnessLevels = brightnessLevels1;
            }


            if (args.Length == 0)
            {
                Console.Write("Input File \"Path\" or name if its in the folder(you also have to write the extension down):");
                inputFilename = Console.ReadLine().Replace("\"", "");
            }
            else
            {
                inputFilename = args[0];
            }

            FileInfo file = new FileInfo(Path.GetFullPath(inputFilename));

            FFOptions options = new FFOptions();
            options.BinaryFolder = Path.GetDirectoryName("ffprobe.exe");

            var matadata = FFProbe.AnalyseAsync(file.FullName, options).Result;

            int vidW = matadata.VideoStreams[0].Width;
            int vidH = matadata.VideoStreams[0].Height;


            int targetFrameWidth = Console.WindowWidth - 1;
            int targetFrameHeight = Console.WindowHeight - 2;


            Console.WriteLine($"video resolution : {vidW} X {vidH}");

            double ratio = vidW / (double)vidH;


            targetFrameWidth = (int)Math.Round(targetFrameHeight * ratio * 2);

            Console.WriteLine($"your resolution: {targetFrameWidth} X {targetFrameHeight}");



            Console.WriteLine("------------------------------\n" +
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
            Console.CursorVisible = false;

            if (Directory.Exists("tmp"))
            {
                if (Directory.Exists("tmp\\frames\\"))
                {
                    Directory.Delete("tmp\\frames\\", true);
                }
                Directory.CreateDirectory("tmp\\frames\\");
                if (File.Exists("tmp\\audio.wav"))
                {
                    File.Delete("tmp\\audio.wav");
                }
            }
            else
            {
                Directory.CreateDirectory("tmp\\");
                Directory.CreateDirectory("tmp\\frames\\");
            }



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

            int frameCount = Directory.GetFiles("tmp\\frames", "*.bmp").Length;

            Bitmap[] b = new Bitmap[frameCount];
            string[] filename = new string[frameCount];
            List<string> frames = new List<string>();
            StringBuilder[] frameBuilder = new StringBuilder[frameCount];
            for (int a = 0; a < frameCount; a++)
            {
                frames.Add("");
                frameBuilder[a] = new StringBuilder("");
            }
            int frameIndex = 1;
            int percentage;
            int[,,] dIndex = new int[frameCount, targetFrameHeight, targetFrameWidth];

            Parallel.For(0, frameCount, a =>
            {
                filename[a] = "tmp\\frames\\" + (a + 1).ToString() + ".bmp";
                b[a] = new Bitmap(filename[a]);
                int H = b[a].Height;
                int W = b[a].Width;
                for (int i = 0; i < H * W + H; i++)
                {
                    frameBuilder[a].Append('x');
                }
                for (int y = 0; y < H; y++)
                {
                    for (int x = 0; x < W; x++)
                    {
                        dIndex[a, y, x] = (int)(b[a].GetPixel(x, y).GetBrightness() * brightnessLevels.Length);
                    }
                }
                Task height = Task.Run(() =>
                {
                    Parallel.For(0, H, y =>
                    {
                        Task width = Task.Run(() =>
                        {
                            Parallel.For(0, W, x =>
                            {
                                if (dIndex[a, y, x] < 0)
                                {
                                    dIndex[a, y, x] = 0;
                                }
                                else if (dIndex[a, y, x] >= brightnessLevels.Length)
                                {
                                    dIndex[a, y, x] = brightnessLevels.Length - 1;
                                }
                                frameBuilder[a].Replace('x', brightnessLevels[dIndex[a, y, x]], x + y * (W + 1), 1);
                            });
                        });
                        width.Wait();
                        frameBuilder[a].Replace('x', '\n', W * (y + 1) + y, 1);
                    });
                });
                height.Wait();
                frames[a] = frameBuilder[a].ToString();

                lock (Lock)
                {
                    frameIndex++;
                    percentage = (int)(frameIndex / (float)frameCount * 100);
                    Console.SetCursorPosition(15, currentCursorHeight);
                    Console.Write(percentage.ToString());
                    Console.SetCursorPosition(21 + percentage / 5, currentCursorHeight);
                    if (percentage % 5 == 0 && percentage != 0)
                    {
                        Console.Write("#");
                    }
                }
            });


            AudioFileReader reader = new AudioFileReader("tmp\\audio.wav");
            WaveOutEvent woe = new WaveOutEvent();
            woe.Init(reader);
            Console.WriteLine("\n\nPress return to play!");
            Console.ReadLine();
            Console.Clear();
            woe.Play();

            while (true)
            {
                float Fpercentage = woe.GetPosition() / (float)reader.Length;
                int frame = (int)(Fpercentage * frameCount);
                if (frame >= frames.Count)
                    break;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine(frames[frame]);

                if (Console.KeyAvailable)
                {
                    ConsoleKey pressed = Console.ReadKey().Key;
                    switch (pressed)
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
                                woe.Stop();
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