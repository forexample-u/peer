using Microsoft.AspNetCore.Mvc;
using Peer.Application;
using Peer.Domain;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    private static Minidb _db = new Minidb(Config.TextPath, Config.FilePath);
    public static DateTime NextCommitTime = DateTime.UtcNow.AddSeconds(Config.CommitBlockSecond);
    public static long CountQuery = 0;

    [HttpPost("write/{id}")]
    public long Write(long id, Message message)
    {
        long deleteUnixAt = _db.Write(id, message);
        if (deleteUnixAt <= 0) return deleteUnixAt;
        CountQuery += 1;
        if (CountQuery % 20 == 0)
        {
            _db.Shrink();
        }
        if (_db.BlockInBytes > Config.AvgSizeBlock || DateTime.UtcNow > NextCommitTime)
        {
            NextCommitTime = DateTime.UtcNow.AddSeconds(Config.CommitBlockSecond);
            _db.Commit();
        }
        return deleteUnixAt;
    }

    [HttpGet("write/{id}/{text}")]
    public long Write(long id, string text)
    {
        return Write(id, new Message() { Text = text });
    }

    [HttpGet("text/{id}")]
    public string Text(long id)
    {
        return _db.Get(id)?.Text ?? "";
    }

    [HttpGet("file/{id}")]
    public IActionResult File(long id)
    {
        Data data = _db.Get(id);
        if (data != null)
        {
            string fullPath = Path.Combine("peer", id.ToString() + Path.GetExtension(data.Filename));
            Response.Headers.Add("Content-Disposition", $"inline; filename*=UTF-8''{Uri.EscapeDataString(data.Filename)}");
            return File(fullPath, data.ContentType, data.Filename);
        }
        return NotFound();
    }

    [HttpGet("download/{id}")]
    public IActionResult Download(long id)
    {
        Data data = _db.Get(id);
        if (data != null)
        {
            string fullPath = Path.Combine("peer", id.ToString() + Path.GetExtension(data.Filename));
            return File(fullPath, data.ContentType, data.Filename);
        }
        return NotFound();
    }
}