using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Ekklesia.Audio
{
    internal class IceCaster : IIceCaster
    {
        public Stream StartStreaming(string host, string password, string mountPoint, string mimeType, bool isPublic, string name, string description, string url, string genre, int port = 8000)
        {
            IceCastStream iceCastStream = new IceCastStream(host, port);

            var byteArray = Encoding.ASCII.GetBytes($"source:{password}");
            var basicAuth = Convert.ToBase64String(byteArray);

            StreamWriter streamWriter = new StreamWriter(iceCastStream, Encoding.ASCII);
            StreamReader streamReader = new StreamReader(iceCastStream, Encoding.ASCII);

            streamWriter.WriteLine($"PUT /{mountPoint} HTTP/1.1");

            streamWriter.WriteLine($"Host: {host}:{port}");
            streamWriter.WriteLine($"Authorization: Basic {basicAuth}");
            streamWriter.WriteLine("User-Agent: icecaster");
            streamWriter.WriteLine("Accept: */*");
            streamWriter.WriteLine("Transfer-Encoding: chunked");
            streamWriter.WriteLine($"Content-Type: {mimeType}");
            streamWriter.WriteLine($"Ice-Public: {(isPublic ? 1 : 0)}");
            streamWriter.WriteLine($"Ice-Name: {name}");
            streamWriter.WriteLine($"Ice-Description: {description}");
            streamWriter.WriteLine($"Ice-URL: {url}");
            streamWriter.WriteLine($"Ice-Genre: {genre}");
            streamWriter.WriteLine("Expect: 100-continue");
            streamWriter.WriteLine();
            streamWriter.Flush();
            var response = streamReader.ReadLine();
            if (response == null)
            {
                throw new IceCastException("Server didn't respond");
            }

            string responseCode = Regex.Replace(response, @"^HTTP/\d\.\d\s+(\d+ .*?)\s*$", "$1");
            switch (responseCode)
            {
                case "100 Continue":
                    break;

                default:
                    throw new IceCastException(responseCode);
            }

            return iceCastStream;
        }

        private class IceCastStream : Stream
        {
            private ILogger _logger = Log.ForContext<IceCastStream>();
            private Stream _icecastStream = null;
            private TcpClient _tcpClient = new TcpClient();

            public IceCastStream(string host, int port)
            {
                try
                {
                    _tcpClient.Connect(host, port);
                    _icecastStream = _tcpClient.GetStream();

                }
                catch (Exception e)
                {
                    _logger.Debug(e, "Something bad happened");
                    CleanUp();
                    throw;
                }
            }

            private void CleanUp()
            {
                _icecastStream?.Dispose();
                _icecastStream = null;
                _tcpClient.Dispose();
            }

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new System.NotImplementedException();

            public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
            public override void Flush()
            {
                _icecastStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new System.NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new System.NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new System.NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    _icecastStream.Write(buffer, offset, count);
                    _icecastStream.Flush();
                }
                catch (Exception e)
                {
                    CleanUp();
                    throw new IceCastException("Couldn't write", e);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _icecastStream?.Flush();
                    _icecastStream?.Dispose();
                    _tcpClient?.Dispose();
                    _tcpClient = null;
                }
            }
        }
    }

    public class IceCastException : Exception
    {
        public IceCastException()
        {
        }

        public IceCastException(string message)
            : base(message)
        {
        }

        public IceCastException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
