namespace Peer.Domain;

public class Message
{
    public Message(string id, string text, IFormFile file)
    {
        Id = id;
        Text = text;
        File = file;
    }

    public string Id { get; set; }
    public string Text { get; set; }
    public IFormFile? File { get; set; }
}