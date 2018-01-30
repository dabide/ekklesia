using System.IO;
using Ekklesia.Songs;
using FluentAssertions;
using Xunit;

namespace Ekklesia.Tests
{
    public class SongParserTests
    {
        private readonly SongParser _songParser;

        public SongParserTests()
        {
            _songParser = new SongParser();
        }
        [Fact]
        public void Test1()
        {
            string songXml = File.ReadAllText(@"TestData\Amazing Grace");
            Song song = _songParser.Parse(songXml);

            song.Title.Should().Be("Amazing Grace");
            song.Author.Should().Be("John Newton");
            song.Copyright.Should().Be("1982 Jubilate Hymns Limited");
            song.Ccli.Should().Be("1037882");
            song.Theme.Should().Be("God: Attributes");
            
            song.Presentation.ShouldBeEquivalentTo(new[] { "V1", "V2", "V3", "V4"});
            song.Lyrics.Count.Should().Be(4);
            song.Lyrics["V1"].Lines.Count.Should().Be(2);
            song.Lyrics["V2"].Lines.Count.Should().Be(2);
            song.Lyrics["V3"].Lines.Count.Should().Be(2);
            song.Lyrics["V4"].Lines.Count.Should().Be(2);
        }
    }
}
