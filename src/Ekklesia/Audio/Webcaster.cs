using System;
using System.IO;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ServiceStack;
using vtortola.WebSockets;

namespace Ekklesia.Audio
{
    internal class Webcaster : IWebcaster
    {
        private readonly IEkklesiaConfiguration _ekklesiaConfiguration;
        private readonly IIceCaster _iceCaster;
        private readonly ILogger _logger = Log.ForContext<Webcaster>();

        public Webcaster(IIceCaster iceCaster, IEkklesiaConfiguration ekklesiaConfiguration)
        {
            _ekklesiaConfiguration = ekklesiaConfiguration;
            _iceCaster = iceCaster;
        }

        public void Start(int port, CancellationToken cancellationToken)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            WebSocketListener server =
                new WebSocketListener(endpoint, new WebSocketListenerOptions {SubProtocols = new[] {"webcast"}});
            server.Standards.RegisterStandard(new WebSocketFactoryRfc6455());
            Task.Run(() => server.StartAsync(cancellationToken), cancellationToken);            

            _logger.Information("Webcast server started at " + endpoint);

            AcceptWebSocketClientsAsync(server, cancellationToken);

            _logger.Information($"Listening on port {port}");
        }

        private void AcceptWebSocketClientsAsync(WebSocketListener server, CancellationToken token)
        {
            Observable.FromAsync(server.AcceptWebSocketAsync)
                .Repeat()
                .Where(s => s != null)
                .Subscribe(async socket =>
                {
                    try
                    {
                        await HandleConnection(socket, token);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, "Error accepting client");
                    }
                },
                    error =>
                    {
                        _logger.Error(error, "Error while accepting socket");
                    }, () =>
                    {
                        _logger.Information("Stopped handling connections");
                    }, token);          
        }

        private async Task HandleConnection(WebSocket webSocket, CancellationToken cancellation)
        {
            try
            {
                WebCastData streamInfo = await GetStreamInfo(webSocket, cancellation);
                IConnectableObservable<byte[]> observable = CreateObserver(webSocket);

                SubscribeFile(cancellation, streamInfo, observable);
                SubscribeIcecast(cancellation, streamInfo, observable);

                observable.Connect();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error handling web socket connection");
            }
        }

        private async Task<WebCastData> GetStreamInfo(WebSocket webSocket, CancellationToken cancellation)
        {
            while (webSocket.IsConnected && !cancellation.IsCancellationRequested)
            {
                WebSocketMessageReadStream message = await webSocket.ReadMessageAsync(cancellation).ConfigureAwait(false);
                if (message == null) continue;

                if (message.MessageType != WebSocketMessageType.Text)
                {
                    throw new Exception("Hello must be said");
                }

                string messageContent;
                using (StreamReader streamReader = new StreamReader(message, Encoding.UTF8))
                {
                    messageContent = await streamReader.ReadToEndAsync();
                }

                WebCastMessage webCastMessage = messageContent.FromJson<WebCastMessage>();

                if (webCastMessage.Type == "hello")
                {
                    WebCastMessage helloMessage = webCastMessage;
                    _logger.Debug($"Mount point: {webSocket.HttpRequest.RequestUri}");
                    _logger.Debug($"MIME type: {webCastMessage.Data.Mime}");
                    _logger.Debug($"Audio channels: {webCastMessage.Data.Audio.Channels}");
                    if (webCastMessage.Data.Mime == "audio/mpeg")
                    {
                        _logger.Debug($"Audio bitrate: {webCastMessage.Data.Audio.BitRate}");
                    }

                    helloMessage.Data.MountPoint = Regex.Replace(webSocket.HttpRequest.RequestUri.OriginalString, @"^/", string.Empty);
                    return helloMessage.Data;
                }

               break;
            }

            _logger.Error("Say hello first!");
            throw new Exception("Should have said hello");
        }

        private IConnectableObservable<byte[]> CreateObserver(WebSocket webSocket)
        {
            return Observable.Create<byte[]>(async (observer, cancellation) => 
                {
                    try
                    {
                        while (webSocket.IsConnected && !cancellation.IsCancellationRequested)
                        {
                            WebSocketMessageReadStream message =
                                await webSocket.ReadMessageAsync(cancellation)
                                    .ConfigureAwait(false);
                            if (message == null) continue;

                            switch (message.MessageType)
                            {
                                case WebSocketMessageType.Text:
                                    string messageContent;
                                    using (StreamReader streamReader =
                                        new StreamReader(message, Encoding.UTF8))
                                    {
                                        messageContent = await streamReader.ReadToEndAsync();
                                    }

                                    WebCastMessage webCastMessage =
                                        messageContent.FromJson<WebCastMessage>();
                                    switch (webCastMessage.Type)
                                    {
                                        case "metadata":
                                            _logger.Debug("Metadata received: {webCastMessage.Data}");
                                            break;

                                        default:
                                            _logger.Warning("Invalid message");
                                            break;
                                    }

                                    break;

                                case WebSocketMessageType.Binary:
                                    byte[] bytes = message.ReadFully();
                                    observer.OnNext(bytes);

                                    break;
                            }
                        }

                        observer.OnCompleted();
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, "Error Handling connection");
                        observer.OnError(exception);
                    }

                    return Disposable.Create(webSocket.Dispose);
                })
                .Publish();
        }

        private void SubscribeFile(CancellationToken cancellation, WebCastData info, IConnectableObservable<byte[]> observable)
        {
            IObservable<byte[]> audioStream = info.Mime != "audio/mpeg"
                ? observable.Transcode(new PcmStream(info.Mime), FlacStream.Default)
                : observable;

            FileStream outStream = null;
            audioStream
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(next =>
                                 {
                                     string ext = info.Mime == "audio/mpeg" ? "mp3" : "wav";

                                     if (outStream == null)
                                     {
                                         outStream = File.OpenWrite($"websocket-test-{Guid.NewGuid():N}.{ext}");
                                     }

                                     outStream.Write(next, 0, next.Length);
                                 }, error =>
                                    {
                                        outStream?.Flush();
                                        outStream?.Dispose();
                                    }, () =>
                                    {
                                        outStream?.Flush();
                                        outStream?.Dispose();
                                    }, cancellation);
        }

        private void SubscribeIcecast(CancellationToken cancellation, WebCastData info, IConnectableObservable<byte[]> observable)
        {
            Stream icecastStream = null;
            IObservable<byte[]> audioStream = info.Mime != "audio/mpeg"
                ? observable.Transcode(new PcmStream(info.Mime), Mp3Stream.Default)
                : observable;

            audioStream
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(next =>
                                 {
                                     try
                                     {
                                         if (icecastStream == null)
                                         {
                                             icecastStream = _iceCaster.StartStreaming(_ekklesiaConfiguration.IceCastServer,
                                                 _ekklesiaConfiguration.IceCastPassword,
                                                 info.MountPoint,
                                                 "audio/mpeg", false, "Test", "Test stream", "http://0.0.0.0",
                                                 "Undefined");
                                         }

                                         icecastStream.Write(next, 0, next.Length);
                                     }
                                     catch (Exception e)
                                     {
                                         _logger.Warning(e, "Error streaming");
                                         try
                                         {
                                             icecastStream?.Flush();
                                             icecastStream?.Dispose();
                                             icecastStream = null;
                                         }
                                         catch (Exception exception)
                                         {
                                             _logger.Warning(exception, "Error cleaning up");
                                         }
                                     }
                                 }, error =>
                                    {
                                        icecastStream?.Flush();
                                        icecastStream?.Dispose();
                                    },
                                 () =>
                                 {
                                     icecastStream?.Flush();
                                     icecastStream?.Dispose();
                                 }, cancellation);
        }        
    }

    internal class WebCastMessage
    {
        public string Type { get; set; }
        public WebCastData Data { get; set; }
    }

    public class WebCastData
    {
        public string Mime { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string MountPoint { get; set; }

        public WebCastAudioInfo Audio { get; set; }
    }

    public class WebCastAudioInfo
    {
        public int Channels { get; set; }
        public int SampleRate { get; set; }
        public int BitRate { get; set; }
        public string Encoder { get; set; }
    }
}
