using Microsoft.AspNetCore.Mvc;

namespace Peer.Domain;

public class Message
{
    [FromForm(Name = "text")]
    public string Text { get; set; }

    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }
}