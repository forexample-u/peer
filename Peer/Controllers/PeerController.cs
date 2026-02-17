using Microsoft.AspNetCore.Mvc;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    private static readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>()
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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<string> PeerUpload(IFormFile? file)
    {
        if (file == null || file.Length > 28311552)
        {
            return "";
        }
        string uploadpath = Path.Combine("wwwroot", "peerdata");
        Directory.CreateDirectory(uploadpath);
        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filepath = Path.Combine(uploadpath, filename);
        try
        {
            using var stream = new FileStream(filepath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/peer/{filename}";
        }
        catch
        {
            return "";
        }
    }

    [HttpGet("{filename}")]
    public IActionResult PeerLoad(string filename)
    {
        string filepath = Path.Combine("wwwroot", "peerdata", filename);
        if (!System.IO.File.Exists(filepath))
        {
            return NotFound("");
        }
        string contentType = "application/octet-stream";
        if (_contentTypes.TryGetValue(Path.GetExtension(filename).ToLower(), out string contentTypeResult))
        {
            contentType = contentTypeResult;
            if (contentType.StartsWith("text/") || contentType == "application/json" || contentType == "application/xml" || contentType == "application/javascript")
            {
                contentType += ";charset=utf-8";
            }
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(filename)}\"";
        }
        return File(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read), contentType, true);
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        return Content(await System.IO.File.ReadAllTextAsync(Path.Combine("wwwroot", "peerdata", "index.html")), "text/html");
    }
}