using Microsoft.AspNetCore.Mvc;
using Peer.Domain;
using Peer.Utils;
using System.Collections.Concurrent;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    public static ConcurrentDictionary<long, Data> messages = new ConcurrentDictionary<long, Data>();
    public static long CountQuery = 0;
    public static long SizeAllQuery = 0;

    [HttpPost("write")]
    public long Write(Message message)
    {
        // settings
        long maxSizeOneQuery = Config.MaxSizeOneQuery;
        long maxSizeText = Config.MaxSizeText;
        long limit1 = Config.Limit1;
        long limit1BigMessageSecond = Config.Limit1BigMessageSecond;
        long limit1SmallMessageSecond = Config.Limit1SmallMessageSecond;
        long limit1SmallSizeOneQuery = Config.Limit1SmallSizeOneQuery;
        long limitOtherBigMessageSecond = Config.LimitOtherBigMessageSecond;
        long limitOtherSmallMessageSecond = Config.LimitOtherSmallMessageSecond;
        long limitOtherSmallSizeOneQuery = Config.LimitOtherSmallSizeOneQuery;

        // if not correct not save file or text
        string fileName = message?.File?.FileName ?? "";
        string contentType = message?.File?.ContentType ?? "";
        long fileSize = message?.File?.Length ?? 0;
        long textSize = message?.Text?.Length ?? 0;
        long querySize = fileSize + textSize;
        if (message == null || message.Id < 0 || querySize > maxSizeOneQuery || textSize > maxSizeText || messages.ContainsKey(message.Id))
        {
            return 0;
        }

        // remove old messages
        long nowSecondUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        ulong fileHash = HashHelper.Hash(fileName + fileSize.ToString());
        bool isUniqueFile = fileName != "";
        List<long> keysToRemove = new List<long>();
        foreach (var keyValue in messages)
        {
            if (keyValue.Value.DeleteUnixAt < nowSecondUnix)
            {
                string fullpath = Path.Combine("wwwroot", "peer", keyValue.Key.ToString() + Path.GetExtension(keyValue.Value.Filename));
                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }
                keysToRemove.Add(keyValue.Key);
                SizeAllQuery -= keyValue.Value.FileSizeOfBytes + keyValue.Value.Text.Length;
            }
        }
        foreach (long key in keysToRemove)
        {
            messages.TryRemove(key, out Data data);
        }
        if (CountQuery % 1000 == 0)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // set bytes by hash
        if (isUniqueFile)
        {
            string filePath = Path.Combine("wwwroot", "peer", message.Id.ToString() + Path.GetExtension(fileName));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                message.File.CopyTo(stream);
            }
        }

        long addSecond = limitOtherBigMessageSecond;
        if (SizeAllQuery <= limit1)
        {
            addSecond = querySize > limit1SmallSizeOneQuery ? limit1BigMessageSecond : limit1SmallMessageSecond;
        }
        else
        {
            addSecond = querySize > limitOtherSmallSizeOneQuery ? limitOtherBigMessageSecond : limitOtherSmallMessageSecond;
        }
        long deleteSecondUnix = nowSecondUnix + addSecond;
        messages[message.Id] = new Data(message.Text, fileHash, fileName, deleteSecondUnix, contentType, fileSize);
        SizeAllQuery += querySize;
        CountQuery += 1;
        return deleteSecondUnix;
    }

    [HttpGet("write/{id}/{text}")]
    public long Write(long id, string text)
    {
        return Write(new Message() { Text = text, Id = id });
    }

    [HttpGet("get/{id}")]
    public string Get(long id)
    {
        messages.TryGetValue(id, out Data? data);
        return data?.Text ?? "";
    }

    [HttpGet("getfile/{id}")]
    public IActionResult GetFile(long id)
    {
        if (messages.TryGetValue(id, out Data? data))
        {
            string fullPath = Path.Combine("peer", id.ToString() + Path.GetExtension(data.Filename));
            return File(fullPath, data.ContentType, data.Filename);
        }
        return NotFound();
    }

    [HttpGet("getfilepath/{id}")]
    public string GetFilePath(long id)
    {
        messages.TryGetValue(id, out Data? data);
        return (data?.Filename?.Length ?? 0) == 0 ? "" : "peer/" + id.ToString() + Path.GetExtension(data.Filename);
    }

    [HttpGet("list")]
    public IEnumerable<string> List()
    {
        return messages.Where(x => x.Value.Filehash == 0).Select(x => $"{x.Key}:{x.Value.DeleteUnixAt}");
    }
}