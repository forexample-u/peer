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

    [HttpPost("write")]
    public long Write(Message message)
    {
        long deleteUnixAt = _db.Write(message);
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
        return Write(new Message() { Text = text, Id = id });
    }

    [HttpGet("get/{id}")]
    public string Get(long id)
    {
        return _db.Get(id)?.Text ?? "";
    }

    [HttpGet("getfile/{id}")]
    public IActionResult GetFile(long id)
    {
        Data data = _db.Get(id);
        if (data != null)
        {
            string fullPath = Path.Combine("peer", id.ToString() + Path.GetExtension(data.Filename));
            return File(fullPath, data.ContentType, data.Filename);
        }
        return NotFound();
    }

    [HttpGet("getfilepath/{id}")]
    public string GetFilePath(long id)
    {
        Data data = _db.Get(id);
        return (data?.Filename?.Length ?? 0) == 0 ? "" : Config.WebUrlFilePath + "/" + id.ToString() + Path.GetExtension(data.Filename);
    }
}