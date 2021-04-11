namespace Ekklesia.Songs
{
    internal interface ISongParser
    {
        Song Parse(string xml, string id);
    }
}
