namespace Peer.Domain;

public class Data
{
    public Data(string text, uint filehash, string filename, long deleteUnixAt)
    {
        Text = text;
        Filehash = filehash;
        DeleteUnixAt = deleteUnixAt;
        Filename = filename;
    }

    public string Text { get; set; }
    public uint Filehash { get; set; }
    public long DeleteUnixAt { get; set; }
    public string Filename { get; set; }
}