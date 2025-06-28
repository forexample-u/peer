namespace Peer.Domain;

public class Message
{
    public string Id { get; set; }
    public string Text { get; set; }
    public IFormFile? File { get; set; }
}