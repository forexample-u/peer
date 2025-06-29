namespace Peer.Domain;

public class Data
{
    public Data(string uniqueKey, string text, int filehash, string filename, long deleteUnixAt, string contentType, int fileSizeOfBytes, byte[] bytes)
    {
        UniqueKey = uniqueKey;
        Text = text;
        Filehash = filehash;
        Filename = filename;
        DeleteUnixAt = deleteUnixAt;
        ContentType = contentType;
        FileSizeOfBytes = fileSizeOfBytes;
        Bytes = bytes;
    }

    public string UniqueKey { get; set; }
    public string Text { get; set; }
    public int Filehash { get; set; }
    public string Filename { get; set; }
    public long DeleteUnixAt { get; set; }
    public string ContentType { get; set; }
    public int FileSizeOfBytes { get; set; }
    public byte[] Bytes { get; set; }
}