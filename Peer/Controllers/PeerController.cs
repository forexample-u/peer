using Microsoft.AspNetCore.Mvc;

namespace Peer.Controllers;

public class Message
{
    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    [HttpPost("upload")]
    public string Upload(Message message)
    {
        if (message.File == null || message.File.Length > 28311552)
        {
            return "";
        }
        Directory.CreateDirectory(Path.Combine("wwwroot", "peer"));
        string filename = message.File.FileName ?? "";
        for (int i = 0; ; i++)
        {
            string id = i.ToString() + "_" + filename;
            if (System.IO.File.Exists(Path.Combine("wwwroot", "peer", id)))
            {
                continue;
            }
            try
            {
                using var stream = new FileStream(Path.Combine("wwwroot", "peer", id), FileMode.CreateNew);
                message.File.CopyTo(stream);
                return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/peer/{id}";
            }
            catch
            {
                if (System.IO.File.Exists(Path.Combine("wwwroot", "peer", id)))
                {
                    continue;
                }
                return "";
            }
        }
    }
}