namespace Peer.Domain;

public class Data
{
    public Data(string text, ulong filehash, string filename, long deleteUnixAt, string contentType, long fileSizeOfBytes)
    {
        Text = text;
        Filehash = filehash;
        Filename = filename;
        DeleteUnixAt = deleteUnixAt;
        ContentType = contentType;
        FileSizeOfBytes = fileSizeOfBytes;
    }

    public string Text { get; set; }
    public ulong Filehash { get; set; }
    public string Filename { get; set; }
    public long DeleteUnixAt { get; set; }
    public long FileSizeOfBytes { get; set; }
    public string ContentType { get; set; }
}