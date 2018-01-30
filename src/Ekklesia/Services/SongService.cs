using System.IO.Abstractions;
using Ekklesia.Songs;
using ServiceStack;

namespace Ekklesia.Services
{
    internal class SongService : Service
    {
        private readonly IEkklesiaConfiguration _ekklesiaConfiguration;
        private readonly IFileSystem _fileSystem;
        private readonly ISongParser _songParser;

        public SongService(IEkklesiaConfiguration ekklesiaConfiguration, IFileSystem fileSystem, ISongParser songParser)
        {
            _ekklesiaConfiguration = ekklesiaConfiguration;
            _fileSystem = fileSystem;
            _songParser = songParser;
        }

        public SongResponse Get(SongRequest request)
        {
            string fileName = _fileSystem.Path.Combine(_ekklesiaConfiguration.SongsFolder, request.Name);
            string songXml = _fileSystem.File.ReadAllText(fileName);
            Song song = _songParser.Parse(songXml);
            return new SongResponse {Song = song};
        }
    }

    [Route("/api/song/{Name}")]
    internal class SongRequest : IReturn<SongResponse>
    {
        public string Name { get; set; }
    }

    internal class SongResponse
    {
        public Song Song { get; set; }
    }
}
