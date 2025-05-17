using Microsoft.AspNetCore.Mvc;
using Peer.Domain;
using Peer.Utils;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    public static Dictionary<long, Data> messages = new Dictionary<long, Data>();
    public static Dictionary<uint, byte[]> files = new Dictionary<uint, byte[]>();

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
        int fileSizeBytes = message?.Bytes?.Length ?? 0;
        if (message == null || message.Id < 0 || fileSizeBytes > maxOneFileSize || message.Text.Length > maxLengthText || messages.ContainsKey(message.Id))
        {
            Array.Clear(message?.Bytes ?? [], 0, fileSizeBytes);
            return 0;
        }

        // set bytes by hash
        uint fileHash = 0;
        if (fileSizeBytes == 0 || files.ContainsKey(fileHash))
        {
            Array.Clear(message.Bytes, 0, fileSizeBytes);
        }
        else
        {
            fileHash = (uint)HashHelper.Hash(message.Bytes);
            files[fileHash] = message.Bytes;
        }

        long sizeFiles = files.Sum(x => x.Value.LongLength);
        long nowSecondUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        long debufSubtSecond = sizeFiles < minDebufSizeBytes ? 0 : debufSecond;
        long addSecond = bigMessageSecond;
        if (message.Text.Length <= smallTextLength && fileSizeBytes <= smallFileSizeBytes)
        {
            addSecond = messages.Count > 100 ? smallMessageSecond : first100SmallMessageSecond;
        }
        long deleteSecondUnix = nowSecondUnix + addSecond - debufSubtSecond;

        // delete old messages
        foreach (var keyValue in messages)
        {
            if (keyValue.Value.DeleteUnixAt < nowSecondUnix)
            {
                uint hash = keyValue.Value.Filehash;
                Array.Clear(files[hash], 0, files[hash].Length);
                files.Remove(hash);
                messages.Remove(keyValue.Key);
            }
        }

        messages[message.Id] = new Data(message.Text, fileHash, message?.Filename ?? "", deleteSecondUnix);
        return deleteSecondUnix;
    }

    [HttpGet("write/{id}/{text}")]
    public long Write(long id, string text)
    {
        return Write(new Message() { Bytes = Array.Empty<byte>(), Text = text, Id = id });
    }

    [HttpGet("get/{id}")]
    public string Get(long id)
    {
        if (messages.TryGetValue(id, out Data? value))
        {
            return value.Text;
        }
        return "";
    }

    [HttpGet("getfile/{id}")]
    public IActionResult GetFile(long id)
    {
        if (messages.TryGetValue(id, out Data? data))
        {
            string fileName = Path.GetFileName(data.Filename ?? "");
            string extension = Path.GetExtension(fileName ?? "");
            string contentType = WebHelper.GetContentTypeByExtension(extension);
            return File(files[data.Filehash], contentType, fileName);
        }
        return NotFound();
    }

    [HttpGet("list")]
    public IEnumerable<string> List()
    {
        return messages.Where(x => x.Value.Filehash == 0).Select(x => $"{x.Key}:{x.Value.DeleteUnixAt}");
    }

    [HttpGet("ids")]
    public IEnumerable<long> Ids()
    {
        return messages.Where(x => x.Value.Filehash == 0).Select(x => x.Key);
    }
}