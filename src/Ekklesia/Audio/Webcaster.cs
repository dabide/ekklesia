using System;
using System.IO;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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
            server.StartAsync(cancellationToken);

            _logger.Information("Webcast server started at " + endpoint);

            Task.Run(() => AcceptWebSocketClientsAsync(server, cancellationToken), cancellationToken);

            _logger.Information($"Listening on port {port}");
        }

        private async Task AcceptWebSocketClientsAsync(WebSocketListener server, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    WebSocket webSocketConnection = await server.AcceptWebSocketAsync(token).ConfigureAwait(false);
                    if (webSocketConnection != null)
                    {
                        HandleConnection(webSocketConnection, token);
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Error Accepting client");
                }
            }

            _logger.Information("Server stopped accepting clients");
        }

        private void HandleConnection(WebSocket webSocket, CancellationToken cancellation)
        {
            IConnectableObservable<StreamPart> observable = Observable
                                                            .Create<StreamPart>(async observer => await ObserveAudioStream(webSocket, cancellation, observer))
                                                            .SubscribeOn(TaskPoolScheduler.Default)
                                                            .ObserveOn(TaskPoolScheduler.Default)
                                                            .Publish();

            SubscribeFile(cancellation, observable);
            SubscribeIcecast(cancellation, observable);

            observable.Connect();
        }

        private void SubscribeFile(CancellationToken cancellation, IConnectableObservable<StreamPart> observable)
        {
            FileStream outStream = null;
            observable.Subscribe(next =>
                                 {
                                     string ext = next.Info.Data.Mime == "audio/mpeg" ? "mp3" : "raw";

                                     if (outStream == null)
                                     {
                                         outStream = File.OpenWrite($"websocket-test-{Guid.NewGuid():N}.{ext}");
                                     }

                                     outStream.Write(next.Data, 0, next.Data.Length);
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

        private void SubscribeIcecast(CancellationToken cancellation, IConnectableObservable<StreamPart> observable)
        {
            Stream icecastStream = null;
            observable.Subscribe(next =>
                                 {
                                     if (icecastStream == null)
                                     {
                                         icecastStream = _iceCaster.StartStreaming(_ekklesiaConfiguration.IceCastServer,
                                                                                   _ekklesiaConfiguration.IceCastPassword,
                                                                                   _ekklesiaConfiguration.IceCastMountPoint,
                                                                                   next.Info.Data.Mime, false, "Test", "Test stream", "http://0.0.0.0",
                                                                                   "Undefined");
                                     }

                                     icecastStream.Write(next.Data, 0, next.Data.Length);
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

        private async Task<IDisposable> ObserveAudioStream(WebSocket webSocket, CancellationToken cancellation, IObserver<StreamPart> observer)
        {
            try
            {
                WebCastMessage helloMessage = null;
                while (webSocket.IsConnected && !cancellation.IsCancellationRequested)
                {
                    WebSocketMessageReadStream message = await webSocket.ReadMessageAsync(cancellation).ConfigureAwait(false);
                    if (message == null) continue;

                    switch (message.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            string messageContent;
                            using (StreamReader streamReader = new StreamReader(message, Encoding.UTF8))
                            {
                                messageContent = await streamReader.ReadToEndAsync();
                            }

                            WebCastMessage webCastMessage = messageContent.FromJson<WebCastMessage>();

                            if (helloMessage == null && webCastMessage.Type == "hello")
                            {
                                helloMessage = webCastMessage;
                                _logger.Debug($"Mount point: {webSocket.HttpRequest.RequestUri}");
                                _logger.Debug($"MIME type: {helloMessage.Data.Mime}");
                                _logger.Debug($"Audio channels: {helloMessage.Data.Audio.Channels}");
                                if (helloMessage.Data.Mime == "audio/mpeg")
                                {
                                    _logger.Debug($"Audio bitrate: {helloMessage.Data.Audio.BitRate}");
                                }
                            }

                            if (helloMessage != null && webCastMessage.Type != "hello")
                            {
                                switch (webCastMessage.Type)
                                {
                                    case "metadata":
                                        _logger.Debug("Metadata received: {webCastMessage.Data}");
                                        break;

                                    default:
                                        _logger.Warning("Invalid message");
                                        break;
                                }
                            }

                            break;

                        case WebSocketMessageType.Binary:
                            if (helloMessage != null)
                            {
                                byte[] bytes = message.ReadFully();
                                observer.OnNext(new StreamPart {Info = helloMessage, Data = bytes});
                            }

                            break;
                    }

                    if (helloMessage == null)
                    {
                        _logger.Error("Say hello first!");
                        throw new Exception("Should have said hello");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error Handling connection");
                observer.OnError(exception);
            }

            observer.OnCompleted();

            return Disposable.Create(webSocket.Dispose);
        }
    }

    internal class StreamPart
    {
        public WebCastMessage Info { get; set; }
        public byte[] Data { get; set; }
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
