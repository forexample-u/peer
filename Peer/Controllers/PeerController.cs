using Microsoft.AspNetCore.Mvc;
using Peer.Domain;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Peer.Controllers;

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    public static ConcurrentDictionary<long, Block> Blocks = new();
    public static List<Data> Messages = new();
    public static long CountQuery = 0;
    public static long SizeAllQuery = 0;
    public static long SizeAllText = 0;
    public static long BlockIndex = 0;

    [HttpPost("write")]
    public long Write(Message message)
    {
        string fileName = message?.File?.FileName ?? "";
        string contentType = message?.File?.ContentType ?? "";
        long fileSize = message?.File?.Length ?? 0;
        long textSize = message?.Text?.Length ?? 0;
        long querySize = fileSize + textSize;
        if (message == null || message.Id < 0 || querySize > Config.MaxSizeOneQuery || textSize > Config.MaxSizeText || Blocks.ContainsKey(message.Id))
        {
            return 0;
        }

        if (fileName != "")
        {
            string filePath = Path.Combine("wwwroot", "peer", message.Id.ToString() + Path.GetExtension(fileName));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                message.File.CopyTo(stream);
            }
        }

        // remove old messages
        long nowUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        CountQuery += 1;
        if (CountQuery % 10 == 0)
        {
            Dictionary<long, List<long>> deleteList = new();
            foreach (var blockId in Blocks)
            {
                if (blockId.Value.DeleteUnix < nowUnix)
                {
                    if (!deleteList.ContainsKey(blockId.Value.Index))
                    {
                        deleteList[blockId.Value.Index] = new();
                    }
                    deleteList[blockId.Value.Index].Add(blockId.Key);
                }
            }
            foreach (var indexDelete in deleteList)
            {
                string path = Path.Combine("data", "text", indexDelete.Key.ToString());
                if (!System.IO.File.Exists(path)) continue;
                List<Data> datas = JsonSerializer.Deserialize<List<Data>>(System.IO.File.ReadAllText(path));
                List<long> deleteIds = new List<long>();
                long sizeQuery = 0;
                foreach (long id in indexDelete.Value)
                {
                    Data deleteData = datas.FirstOrDefault(x => x.Id == id);
                    if (deleteData != null)
                    {
                        string fullpath = Path.Combine("wwwroot", "peer", id.ToString() + Path.GetExtension(deleteData.Filename ?? ""));
                        if (System.IO.File.Exists(fullpath))
                        {
                            System.IO.File.Delete(fullpath);
                        }
                        sizeQuery += deleteData.Text.Length + deleteData.FileSizeOfBytes;
                        datas.Remove(deleteData);
                        deleteIds.Add(id);
                    }
                }
                try
                {
                    if (datas.Count > 0)
                    {
                        System.IO.File.WriteAllText(path, JsonSerializer.Serialize(datas));
                    }
                    else
                    {
                        System.IO.File.Delete(path);
                    }
                    foreach (long id in deleteIds)
                    {
                        Blocks.TryRemove(id, out Block _);
                    }
                    SizeAllQuery -= sizeQuery;
                } catch { }
                datas.Clear();
            }
            deleteList.Clear();
        }

        if (SizeAllText > 1000000)
        {
            SizeAllText = 0;
            System.IO.File.WriteAllText(Path.Combine("data", "text", BlockIndex.ToString()), JsonSerializer.Serialize(Messages));
            Messages.Clear();
            BlockIndex += 1;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        long addSecond = querySize > Config.LimitOtherSmallSizeOneQuery ? Config.LimitOtherBigMessageSecond : Config.LimitOtherSmallMessageSecond;
        if (SizeAllQuery <= Config.Limit1)
        {
            addSecond = querySize > Config.Limit1SmallSizeOneQuery ? Config.Limit1BigMessageSecond : Config.Limit1SmallMessageSecond;
        }
        Data newData = new(message.Id, message.Text, 0, fileName, nowUnix + addSecond, contentType, fileSize);
        Blocks[newData.Id] = new Block(BlockIndex, newData.DeleteUnixAt);
        Messages.Add(newData);
        SizeAllQuery += querySize;
        SizeAllText += textSize;
        return newData.DeleteUnixAt;
    }

    [HttpGet("write/{id}/{text}")]
    public long Write(long id, string text)
    {
        return Write(new Message() { Text = text, Id = id });
    }

    [HttpGet("get/{id}")]
    public string Get(long id)
    {
        return GetData(id)?.Text ?? "";
    }

    [HttpGet("getfile/{id}")]
    public IActionResult GetFile(long id)
    {
        Data data = GetData(id);
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
        Data data = GetData(id);
        return (data?.Filename?.Length ?? 0) == 0 ? "" : "peer/" + id.ToString() + Path.GetExtension(data.Filename);
    }

    private Data GetData(long id)
    {
        Data findData = Messages.FirstOrDefault(x => x.Id == id);
        if (findData != null)
        {
            return findData;
        }

        if (Blocks.TryGetValue(id, out Block block))
        {
            string path = Path.Combine("data", "text", block.Index.ToString());
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                Data[] datas = JsonSerializer.Deserialize<Data[]>(json);
                return datas.FirstOrDefault(x => x.Id == id);
            }
        }
        return null;
    }
}