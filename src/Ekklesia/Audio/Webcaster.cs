using System;
using System.IO;
using System.Net;
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
        private ILogger _logger = Log.ForContext<Webcaster>();
        private readonly IIceCaster _iceCaster;
        private readonly IEkklesiaConfiguration _ekklesiaConfiguration;

        public Webcaster(IIceCaster iceCaster, IEkklesiaConfiguration ekklesiaConfiguration)
        {
            _ekklesiaConfiguration = ekklesiaConfiguration;
            _iceCaster = iceCaster;
        }

        public void Start(int port, CancellationToken cancellationToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            var server = new WebSocketListener(endpoint, new WebSocketListenerOptions { SubProtocols = new[] { "webcast" } });
            server.Standards.RegisterStandard(new WebSocketFactoryRfc6455());
            server.StartAsync(cancellationToken);

            _logger.Information("Webcast server started at " + endpoint.ToString());

            var task = Task.Run(() => AcceptWebSocketClientsAsync(server, cancellationToken));

            _logger.Information($"Listening on port {port}");
        }

        private async Task AcceptWebSocketClientsAsync(WebSocketListener server, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var webSocketConnection = await server.AcceptWebSocketAsync(token).ConfigureAwait(false);
                    if (webSocketConnection != null)
                        Task.Run(() => HandleConnectionAsync(webSocketConnection, token));
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Error Accepting client");
                }
            }

            _logger.Information("Server stopped accepting clients");
        }

        private async Task HandleConnectionAsync(WebSocket webSocket, CancellationToken cancellation)
        {
            Stream outStream = null;
            Stream icecastStream = null;

            try
            {
                WebCastMessage helloMessage = null;
                while (webSocket.IsConnected && !cancellation.IsCancellationRequested)
                {
                    var message = await webSocket.ReadMessageAsync(cancellation).ConfigureAwait(false);
                    if (message == null) continue;

                    switch (message.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var messageContent = string.Empty;
                            using (var streamReader = new StreamReader(message, Encoding.UTF8))
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
                                // We only support mp3 and raw PCM for now
                                string ext = helloMessage.Data.Mime == "audio/mpeg" ? "mp3" : "raw";
                                outStream = File.OpenWrite($"websocket-test-{Guid.NewGuid():N}.{ext}");
                                icecastStream = _iceCaster.StartStreaming(_ekklesiaConfiguration.IceCastServer, _ekklesiaConfiguration.IceCastPassword, _ekklesiaConfiguration.IceCastMountPoint, helloMessage.Data.Mime, false, "Test", "Test stream", "http://0.0.0.0", "Undefined");
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
                                var bytes = message.ReadFully();
                                Task.Run(() =>
                                {
                                    outStream.Write(bytes, 0, bytes.Length);
                                    icecastStream.Write(bytes, 0, bytes.Length);
                                });

                            }

                            break;
                    }

                    if (helloMessage == null)
                    {
                        _logger.Error("Say hello first!");
                        throw new Exception("Should have said hello");
                    }
                }

                await outStream?.FlushAsync();
                outStream?.Dispose();
                outStream = null;
                await icecastStream?.FlushAsync();
                icecastStream?.Dispose();
                icecastStream = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error Handling connection");
                try
                {
                    await outStream?.FlushAsync();
                    outStream?.Dispose();
                    await icecastStream?.FlushAsync();
                    icecastStream?.Dispose();
                    webSocket.Close();
                }
                catch (Exception e)
                {
                    _logger.Warning($"Couldn't close: {e}");
                }
            }
            finally
            {
                webSocket.Dispose();
            }
        }
    }

    class WebCastMessage
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
