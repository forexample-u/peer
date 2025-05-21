namespace Peer.Domain;

public class Message
{
    public long Id { get; set; }
    public string Text { get; set; }
    public IFormFile? File { get; set; }
}