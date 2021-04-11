using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Ekklesia.Audio
{
    internal static class TranscodingExtensions
    {
        public static IObservable<byte[]> Transcode(this IObservable<byte[]> observable, AudioStreamType inputType, AudioStreamType outputType)
        {
            return Observable.Create<byte[]>(observer =>
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    using (Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments =
                                $"{inputType.InputOptions} -i - {outputType.OutputOptions} -",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                        EnableRaisingEvents = true
                    })
                    {
                        process.ErrorDataReceived += (sender, eventArgs) =>
                        {
                            Log.Logger.Debug(eventArgs.Data);
                        };

                        process.Start();
                        process.BeginErrorReadLine();

                        StreamWriter inputStream = process.StandardInput;
                        StreamReader outputStream = process.StandardOutput;

                        Task writerTask = null;
                        observable.Subscribe(async bytes =>
                            {
                                try
                                {
                                    if (cancellationTokenSource.IsCancellationRequested) return;

                                    await inputStream.BaseStream.WriteAsync(bytes, 0, bytes.Length,
                                        cancellationTokenSource.Token);
                                    await inputStream.BaseStream.FlushAsync(cancellationTokenSource.Token);

                                    if (writerTask == null)
                                    {
                                        writerTask = Task.Run(async () =>
                                        {
                                            try
                                            {
                                                byte[] buffer = new byte[1024];

                                                while (!cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    int read = await outputStream.BaseStream.ReadAsync(
                                                        buffer, 0, buffer.Length,
                                                        cancellationTokenSource.Token);
                                                    if (read <= 0)
                                                    {
                                                        break;
                                                    }

                                                    observer.OnNext(buffer.Take(read).ToArray());
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                observer.OnError(e);
                                                if (!cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    cancellationTokenSource.Cancel();
                                                }
                                            }
                                        }, cancellationTokenSource.Token);
                                    }
                                }
                                catch (Exception e)
                                {
                                    observer.OnError(e);
                                    if (!cancellationTokenSource.IsCancellationRequested)
                                    {
                                        cancellationTokenSource.Cancel();
                                    }
                                }
                            }, error =>
                            {
                                if (!cancellationTokenSource.IsCancellationRequested)
                                {
                                    cancellationTokenSource.Cancel();
                                }
                            },
                            () =>
                            {
                                inputStream.Close();
                            }, cancellationTokenSource.Token);

                        process.WaitForExit();

                        observer.OnCompleted();
                    }
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    cancellationTokenSource.Cancel();
                }

                return Disposable.Create(() =>
                {
                    cancellationTokenSource.Cancel();
                });
            });
        }
    }

    internal abstract class AudioStreamType
    {
        public abstract string InputOptions { get; }
        public abstract string OutputOptions { get; }
        public abstract string Extension { get; }
    }


    internal class PcmStream : AudioStreamType
    {
        private PcmStream()
        {
        }

        public PcmStream(string mime)
        {
            Regex regex = new Regex(@"audio/x-raw,format=(?<format>\S+),channels=(?<channels>\d+),layout=interleaved,rate=(?<samplerate>\d+)");

            Match match = regex.Match(mime);
            InputOptions =
                $"-f {match.Groups["format"].Value.ToLowerInvariant()} -ar {match.Groups["samplerate"].Value} -ac {match.Groups["channels"].Value}";
        }

        public static PcmStream Default { get; } = new PcmStream();

        public override string InputOptions { get; } = "-f s16le -ar 44.1k -ac 2";

        public override string OutputOptions => InputOptions;

        public override string Extension => "raw";
    }

    internal class WavStream : AudioStreamType
    {
        private WavStream()
        {
        }

        public static WavStream Default { get; } = new WavStream();

        public override string InputOptions => "-f wav";

        public override string OutputOptions => InputOptions;

        public override string Extension => "wav";
    }


    internal class FlacStream : AudioStreamType
    {
        private FlacStream()
        {
        }

        public static FlacStream Default { get; } = new FlacStream();

        public override string InputOptions => "-f flac";

        public override string OutputOptions => InputOptions;

        public override string Extension => "flac";
    }

    internal class Mp3Stream : AudioStreamType
    {
        private Mp3Stream() : this(128000)
        {
        }

        public Mp3Stream(int sampleRate)
        {
            SampleRate = sampleRate;
        }

        public static Mp3Stream Default { get; } = new Mp3Stream();
        public int SampleRate { get; }

        public override string InputOptions => "-f mp3";
        public override string OutputOptions => $"-c:a libmp3lame -abr 1 -b:a {SampleRate} -f mp3";

        public override string Extension => "mp3";
    }
}
