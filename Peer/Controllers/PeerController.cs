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

    [HttpGet("file/{id}")]
    public IActionResult File(string id)
    {
        if (!System.IO.File.Exists(Path.Combine("wwwroot", "peer", id)))
        {
            return NotFound();
        }
        string filename = string.Concat(id.Split("_").Skip(1));
        string contentType = GetContentType(Path.GetExtension(filename));
        Response.Headers.Add("Content-Disposition", "inline");
        return File(Path.Combine("peer", id));
    }

    private string GetContentType(string ext)
    {
        string type = "application/octet-stream";
        if (ext == ".html") { type = "text/html"; }
        if (ext == ".json") { type = "application/json"; }
        if (ext == ".jpeg") { type = "image/jpeg"; }
        if (ext == ".txt") { type = "text/plain"; }
        if (ext == ".jpg") { type = "image/jpeg"; }
        if (ext == ".png") { type = "image/png"; }
        if (ext == ".gif") { type = "image/gif"; }
        if (ext == ".mp4") { type = "video/mp4"; }
        if (ext == ".mp3") { type = "audio/mpeg"; }
        if (ext == ".pdf") { type = "application/pdf"; }
        return type;
    }
}