namespace Peer.Domain;

public class Message
{
    public long Id { get; set; }
    public string Text { get; set; }
    public string Filename { get; set; }
    public byte[] Bytes { get; set; }
}