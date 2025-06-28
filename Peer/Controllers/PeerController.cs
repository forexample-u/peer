using Microsoft.AspNetCore.Mvc;
using Peer.Application;
using Peer.Domain;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    private static Minidb _db = new Minidb();

    [HttpPost("write")]
    public long Write(Message message)
    {
        DataInformation dataInfo = _db.GetInfo();
        if (dataInfo.CountQuery % 20 == 0)
        {
            _db.Shrink();
        }
        return _db.Write(message, dataInfo.QuerySizeOfBytes);
    }

    [HttpGet("write/{id}/{text}")]
    public long Write(string id, string text)
    {
        return Write(new Message() { Text = text, Id = id });
    }

    [HttpGet("text/{id}")]
    public string Text(string id)
    {
        return _db.Get(id)?.Text ?? "";
    }

    [HttpGet("file/{id}")]
    public IActionResult File(string id)
    {
        Data data = _db.Get(id);
        if (data != null)
        {
            Response.Headers.Add("Content-Disposition", $"inline; filename*=UTF-8''{Uri.EscapeDataString(data.Filename)}");
            return File(data.Bytes, data.ContentType);
        }
        return NotFound();
    }

    [HttpGet("download/{id}")]
    public IActionResult Download(string id)
    {
        Data data = _db.Get(id);
        if (data != null)
        {
            return File(data.Bytes, data.ContentType, data.Filename);
        }
        return NotFound();
    }
}