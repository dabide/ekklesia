using System.IO;

namespace Ekklesia.Audio
{
    internal interface IIceCaster
    {
        Stream StartStreaming(string host,
            string password,
            string mountPoint,
            string mimeType,
            bool isPublic,
            string name,
            string description,
            string url,
            string genre,
            int port = 8000);
    }
}
