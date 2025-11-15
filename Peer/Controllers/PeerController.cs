using Microsoft.AspNetCore.Mvc;
using System.Web;

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
        string uploadpath = Path.Combine("wwwroot", "peerdata");
        string filename = message.File.FileName ?? "";
        Directory.CreateDirectory(uploadpath);
        int i = 0;
        while (true)
        {
            string fileid = $"{i}_{filename}";
            string filepath = Path.Combine(uploadpath, fileid);
            i += 1;
            if (System.IO.File.Exists(filepath))
            {
                continue;
            }
            try
            {
                using var stream = new FileStream(filepath, FileMode.CreateNew);
                message.File.CopyTo(stream);
                return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/peer/{fileid}";
            }
            catch
            {
                if (System.IO.File.Exists(filepath))
                {
                    continue;
                }
                return "";
            }
        }
    }

    [HttpGet("{filename}")]
    public IActionResult Load(string filename)
    {
        string filepath = Path.Combine("wwwroot", "peerdata", filename);
        if (!System.IO.File.Exists(filepath))
        {
            return NotFound();
        }
        Dictionary<string, string> contentTypes = new Dictionary<string, string>()
        {
            { ".jpeg", "image/jpeg" }, { ".jpg", "image/jpeg" }, { ".png", "image/png" }, { ".gif", "image/gif" }, { ".webp", "image/webp" },
            { ".bmp", "image/bmp" }, { ".svg", "image/svg+xml" }, { ".ico", "image/x-icon" },
            { ".mp4", "video/mp4" }, { ".webm", "video/webm" }, { ".mkv", "video/x-matroska" },
            { ".mov", "video/quicktime" }, { ".ogv", "video/ogg" }, { ".flv", "video/x-flv" },
            { ".mp3", "audio/mpeg" }, { ".aac", "audio/aac" }, { ".wav", "audio/wav" }, { ".flac", "audio/flac" },
            { ".ogg", "audio/ogg" }, { ".opus", "audio/opus" }, { ".m4a", "audio/mp4" },
            { ".pdf", "application/pdf" }, { ".zip", "application/zip" }, { ".php", "application/x-php" },
            { ".html", "text/html" }, { ".css", "text/css" }, { ".js", "application/javascript" },
            { ".json", "application/json" }, { ".xml", "application/xml" }, { ".txt", "text/plain" }, { ".csv", "text/csv" },
            { ".woff", "font/woff" }, { ".woff2", "font/woff2" }, { ".rar", "application/vnd.rar" }, { ".torrent", "application/x-bittorrent" }
        };
        string extension = Path.GetExtension(filename).ToLower();
        string contentType = "application/octet-stream";
        if (contentTypes.TryGetValue(extension, out string contentTypeResult))
        {
            contentType = contentTypeResult;
            if (contentType.StartsWith("text/") || contentType == "application/json" || contentType == "application/xml"
                || contentType == "application/javascript" || contentType == "application/pdf")
            {
                contentType += ";charset=utf-8";
            }
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(filename)}\"";
        }
        return File(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read), contentType, true);
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        return Content(System.IO.File.ReadAllText(Path.Combine("wwwroot", "peerdata", "index.html")), "text/html");
    }
}