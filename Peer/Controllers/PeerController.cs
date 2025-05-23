using Microsoft.AspNetCore.Mvc;
using Peer.Domain;
using Peer.Utils;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    public static Dictionary<long, Data> messages = new Dictionary<long, Data>();

    [HttpPost("write")]
    public long Write(Message message)
    {
        // settings
        long maxOneFileSize = Config.MaxOneFileSize;
        long minDebufSizeBytes = Config.MinDebufSizeBytes;
        long maxLengthText = Config.MaxLengthText;
        long debufSecond = Config.DebufSecond;
        long bigMessageSecond = Config.BigMessageSecond;
        long smallMessageSecond = Config.SmallMessageSecond;
        long first100SmallMessageSecond = Config.First100SmallMessageSecond;
        long smallTextLength = Config.SmallTextLength;
        long smallFileSizeBytes = Config.SmallFileSizeBytes;

        // if not correct not save file or text
        long fileSizeBytes = message?.File?.Length ?? 0;
        string fileName = message?.File?.FileName ?? "";
        string contentType = message?.File?.ContentType ?? "";
        if (message == null || message.Id < 0 || fileSizeBytes > maxOneFileSize || message.Text.Length > maxLengthText || messages.ContainsKey(message.Id))
        {
            return 0;
        }

        // remove old messages
        long nowSecondUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        ulong fileHash = HashHelper.Hash(fileName + fileSizeBytes.ToString());
        long sizeFiles = 0;
        bool isUniqueFile = fileName != "";
        foreach (var keyValue in messages)
        {
            sizeFiles += keyValue.Value.FileSizeOfBytes;
            if (keyValue.Value.DeleteUnixAt < nowSecondUnix)
            {
                string fullpath = Path.Combine("wwwroot", "peer", keyValue.Key.ToString() + Path.GetExtension(keyValue.Value.Filename));
                messages.Remove(keyValue.Key);
                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }
            }
            if (keyValue.Value.Filehash == fileHash)
            {
                isUniqueFile = false;
            }
        }

        // set bytes by hash
        if (isUniqueFile)
        {
            string filePath = Path.Combine("wwwroot", "peer", message.Id.ToString() + Path.GetExtension(message?.File?.FileName ?? ""));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                message.File.CopyTo(stream);
            }
        }

        long debufSubtSecond = sizeFiles < minDebufSizeBytes ? 0 : debufSecond;
        long addSecond = bigMessageSecond;
        if (message.Text.Length <= smallTextLength && fileSizeBytes <= smallFileSizeBytes)
        {
            addSecond = messages.Count > 100 ? smallMessageSecond : first100SmallMessageSecond;
        }
        long deleteSecondUnix = nowSecondUnix + addSecond - debufSubtSecond;
        messages[message.Id] = new Data(message.Text, fileHash, fileName, deleteSecondUnix, contentType, fileSizeBytes);
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
        return data?.FileSizeOfBytes > 0 ? "peer/" + id.ToString() + Path.GetExtension(data.Filename) : "";
    }

    [HttpGet("list")]
    public IEnumerable<string> List()
    {
        return messages.Where(x => x.Value.Filehash == 0).Select(x => $"{x.Key}:{x.Value.DeleteUnixAt}");
    }
}