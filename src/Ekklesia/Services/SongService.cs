using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Ekklesia.Songs;
using ServiceStack;
using ServiceStack.Caching;

namespace Ekklesia.Services
{
    internal class SongService : Service
    {
        private readonly IEkklesiaConfiguration _ekklesiaConfiguration;
        private readonly IFileSystem _fileSystem;
        private readonly ISongParser _songParser;
        private readonly ICacheClient _cacheClient;

        public SongService(IEkklesiaConfiguration ekklesiaConfiguration, IFileSystem fileSystem, ISongParser songParser, ICacheClient cacheClient)
        {
            _ekklesiaConfiguration = ekklesiaConfiguration;
            _fileSystem = fileSystem;
            _songParser = songParser;
            _cacheClient = cacheClient;
        }

        public SongResponse Get(SongRequest request)
        {
            string fileName = _fileSystem.Path.Combine(_ekklesiaConfiguration.SongsFolder, request.Name);
            string songXml = _fileSystem.File.ReadAllText(fileName);
            Song song = _songParser.Parse(songXml);
            return new SongResponse {Song = song};
        }

        public SongSearchResponse Get(SongSearchRequest request)
        {
            int limit = request.Limit > 0 ? request.Limit : 10;
            IEnumerable<string> songNames = GetFiles()
                .Where(s => s.ToLowerInvariant().Contains(request.Filter.ToLowerInvariant()))
                .Take(limit);

            return new SongSearchResponse
            {
                SongNames = songNames.ToList()
            };
        }

        private string[] GetFiles()
        {
            string[] cachedNames = _cacheClient.Get<string[]>("songs:names");
            if (cachedNames != null)
            {
                return cachedNames;
            }

            string[] names = _fileSystem.DirectoryInfo.FromDirectoryName(_ekklesiaConfiguration.SongsFolder)
                .GetFiles()
                .Select(f => f.Name)
                .ToArray();

            _cacheClient.Set("songs:names", names, TimeSpan.FromMinutes(1));

            return names;
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

    [Route("/api/song-search/{Filter}")]
    internal class SongSearchRequest : IReturn<SongSearchResponse>
    {
        public string Filter { get; set; }
        public int Limit { get; set; }
    }

    internal class SongSearchResponse
    {
        public List<string> SongNames { get; set; }
    }
}
