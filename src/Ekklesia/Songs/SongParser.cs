using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Ekklesia.Songs
{
    internal class SongParser : ISongParser
    {
        public Song Parse(string xml)
        {
            try
            {
                XDocument document = XDocument.Parse(xml);
                XElement songElement = document.Element("song");
                if (songElement == null)
                {
                    throw new Exception("Couldn't parse song");
                }

                Song song = new Song
                {
                    Id = Guid.NewGuid(),
                    Title = songElement.Element("title")?.Value,
                    Author = songElement.Element("author")?.Value,
                    Copyright = songElement.Element("copyright")?.Value,
                    Presentation = GetPresentation(songElement.Element("presentation")?.Value),
                    HymnNumber = songElement.Element("hymn_number")?.Value,
                    Capo = songElement.Element("capo")?.Value,
                    Tempo = songElement.Element("tempo")?.Value,
                    TimeSignature = songElement.Element("timesig")?.Value,
                    Ccli = songElement.Element("ccli")?.Value,
                    Theme = songElement.Element("theme")?.Value,
                    AlternateTheme = songElement.Element("alttheme")?.Value,
                    User1 = songElement.Element("user1")?.Value,
                    User2 = songElement.Element("user2")?.Value,
                    User3 = songElement.Element("user3")?.Value,
                    Key = songElement.Element("key")?.Value,
                    Aka = songElement.Element("aka")?.Value,
                    KeyLine = songElement.Element("key_line")?.Value
                };
                List<SongPart> songParts = GetLyrics(songElement.Element("lyrics")?.Value).ToList();
                if (song.Presentation == null)
                {
                    song.Presentation = songParts.Select(v => v.Identifier).ToList();
                }

                song.Lyrics = songParts.ToDictionary(v => v.Identifier);
                return song;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't parse song", e);
            }
        }

        private List<string> GetPresentation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim().Split(' ').ToList();
        }

        private IEnumerable<SongPart> GetLyrics(string value)
        {
            StringReader stringReader = new StringReader(value);
            Regex identifierRegex = new Regex(@"^\s*\[([^\]]+)\]\s*");
            Regex lineRegex = new Regex(@"^ (\S.*?)\s*$");

            string line;
            SongPart currentPart = null;
            while ((line = stringReader.ReadLine()) != null)
            {
                Match identifierMatch = identifierRegex.Match(line);
                if (identifierMatch.Success)
                {
                    if (currentPart != null)
                    {
                        yield return currentPart;
                    }

                    currentPart = new SongPart { Identifier = identifierMatch.Groups[1].Value, Lines = new List<string>()};
                    continue;
                }

                if (currentPart == null)
                {
                    continue;
                }

                Match lineMatch = lineRegex.Match(line);
                if (lineMatch.Success)
                {
                    currentPart.Lines.Add(lineMatch.Groups[1].Value);
                }
            }

            if (currentPart != null)
            {
                yield return currentPart;
            }
        }
    }
}
