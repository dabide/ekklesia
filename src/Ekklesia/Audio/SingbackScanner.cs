using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using File = TagLib.File;

namespace Ekklesia.Audio
{
    public class SingbackScanner : ISingbackScanner
    {
        private readonly IEkklesiaConfiguration _ekklesiaConfiguration;
        private readonly Func<string, bool, File.IFileAbstraction> _fileAbstractionFactory;
        private readonly IFileSystem _fileSystem;

        public SingbackScanner(IFileSystem fileSystem, Func<string, bool, File.IFileAbstraction> fileAbstractionFactory,
            IEkklesiaConfiguration ekklesiaConfiguration)
        {
            _fileSystem = fileSystem;
            _fileAbstractionFactory = fileAbstractionFactory;
            _ekklesiaConfiguration = ekklesiaConfiguration;
        }

        public Dictionary<string, string> Scan()
        {
            Regex titleRegex = new Regex(@"^(\d+)\s+(.*)$");

            return (from file in _fileSystem.DirectoryInfo.FromDirectoryName(_ekklesiaConfiguration.SingbackFolder)
                                            .GetFiles("*.mp3", SearchOption.AllDirectories)
                    where !file.Name.StartsWith(".")
                    let mp3File = File.Create(_fileAbstractionFactory(file.FullName, false))
                    let match = titleRegex.Match(mp3File.Tag.Title)
                    where match.Success
                    let songNumber = match.Groups[1].Value
                    let songTitle = match.Groups[2].Value
                    select new {SongNumber = songNumber, SongTitle = songTitle, Path = file.FullName})
                .ToDictionary(s => s.SongNumber, s => s.Path);
        }
    }
}
