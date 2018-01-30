using System;
using System.Collections.Generic;

namespace Ekklesia.Songs
{
    internal class Song
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
        public List<string> Presentation { get; set; }
        public string HymnNumber { get; set; }
        public string Capo { get; set; }
        public string Tempo { get; set; }
        public string TimeSignature { get; set; }
        public string Ccli { get; set; }
        public string Theme { get; set; }
        public string AlternateTheme { get; set; }
        public string User1 { get; set; }
        public string User2 { get; set; }
        public string User3 { get; set; }
        public string Key { get; set; }
        public string Aka { get; set; }
        public string KeyLine { get; set; }
        public Dictionary<string, SongPart> Lyrics { get; set; }
    }
}
