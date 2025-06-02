using Peer.Domain;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Peer.Application;

public class Minidb
{
    private readonly string _textPath;
    private readonly string _filePath;
    private bool _isLockByCommit = false;
    private bool _isLockByShrink = false;
    public Minidb(string textPath, string filePath, long blockStartIndex = 0)
    {
        _textPath = textPath;
        _filePath = filePath;
        BlockIndex = blockStartIndex;
        Load();
    }

    private List<Data> BlockMessage = new();
    private ConcurrentDictionary<long, Block> Blocks = new();
    public long BlockIndex { get; private set; } = 0;
    public long BlockSizeInBytes { get; private set; } = 0;
    public long SizeAllBlockInBytes { get; private set; } = 0;

    public long Write(Message message)
    {
        string contentType = message.File?.ContentType ?? "";
        string fileName = message.File?.FileName ?? "";
        long fileSize = message.File?.Length ?? 0;
        long textSize = message.Text?.Length ?? 0;
        long querySize = textSize + fileSize;
        if (message == null || querySize > Config.MaxSizeOneQuery || textSize > Config.MaxSizeText || Blocks.ContainsKey(message.Id))
        {
            return 0;
        }

        if (!string.IsNullOrEmpty(fileName))
        {
            string filePath = Path.Combine(_filePath, message.Id.ToString() + Path.GetExtension(fileName));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                message.File?.CopyTo(stream);
            }
        }

        long addSecond = querySize > Config.LimitOtherSmallSizeOneQuery ? Config.LimitOtherBigMessageSecond : Config.LimitOtherSmallMessageSecond;
        if (SizeAllBlockInBytes <= Config.Limit1)
        {
            addSecond = querySize > Config.Limit1SmallSizeOneQuery ? Config.Limit1BigMessageSecond : Config.Limit1SmallMessageSecond;
        }
        long deleteUnixAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + addSecond;
        Data data = new(message.Id, message.Text, 0, fileName, deleteUnixAt, contentType, fileSize);
        Blocks[message.Id] = new Block(BlockIndex, deleteUnixAt);
        BlockMessage.Add(data);
        SizeAllBlockInBytes += querySize;
        BlockSizeInBytes += textSize;
        return deleteUnixAt;
    }

    public Data Get(long id)
    {
        for (int i = 0; i < BlockMessage.Count; i++)
        {
            if (BlockMessage[i].Id == id)
            {
                return BlockMessage[i];
            }
        }

        if (Blocks.TryGetValue(id, out Block? block))
        {
            string path = Path.Combine(_textPath, block.Index.ToString());
            if (File.Exists(path))
            {
                string json = ReadAllText(path);
                Data[] datas = JsonSerializer.Deserialize<Data[]>(json) ?? Array.Empty<Data>();
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].Id == id)
                    {
                        return datas[i];
                    }
                }
            }
        }
        return null;
    }

    public void Shrink()
    {
        try
        {
            if (_isLockByShrink == false)
            {
                _isLockByShrink = true;
                long nowUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                for (int i = BlockMessage.Count - 1; i >= 0; i--)
                {
                    if (BlockMessage[i].DeleteUnixAt < nowUnix)
                    {
                        string fullpath = Path.Combine(_filePath, BlockMessage[i].Id.ToString() + Path.GetExtension(BlockMessage[i].Filename ?? ""));
                        if (File.Exists(fullpath))
                        {
                            File.Delete(fullpath);
                        }
                        SizeAllBlockInBytes -= BlockMessage[i].Text.Length + BlockMessage[i].FileSizeOfBytes;
                        BlockSizeInBytes -= BlockMessage[i].Text.Length;
                        BlockMessage.RemoveAt(i);
                    }
                }

                HashSet<long> indexes = new();
                foreach (var block in Blocks)
                {
                    if (block.Value.DeleteUnix < nowUnix)
                    {
                        indexes.Add(block.Value.Index);
                    }
                }
                foreach (long index in indexes)
                {
                    string path = Path.Combine(_textPath, index.ToString());
                    if (!File.Exists(path)) continue;
                    List<Data> datas = new List<Data>();
                    try
                    {
                        datas = JsonSerializer.Deserialize<List<Data>>(ReadAllText(path)) ?? new List<Data>();
                    }
                    catch { }
                    long sizeQuery = 0;
                    List<long> deleteIds = new List<long>();
                    for (int i = 0; i < datas.Count; i++)
                    {
                        Data data = datas[i];
                        if (data.DeleteUnixAt < nowUnix)
                        {
                            string fullpath = Path.Combine(_filePath, data.Id.ToString() + Path.GetExtension(data.Filename ?? ""));
                            if (File.Exists(fullpath))
                            {
                                File.Delete(fullpath);
                            }
                            sizeQuery += data.Text.Length + data.FileSizeOfBytes;
                            deleteIds.Add(data.Id);
                            datas.RemoveAt(i);
                            i--;
                        }
                    }
                    try
                    {
                        if (datas.Count > 0)
                        {
                            WriteAllText(path, JsonSerializer.Serialize(datas));
                        }
                        else
                        {
                            File.Delete(path);
                        }
                        foreach (long id in deleteIds)
                        {
                            Blocks.TryRemove(id, out Block _);
                        }
                        SizeAllBlockInBytes -= sizeQuery;
                    }
                    catch { }
                }
                _isLockByShrink = false;
            }
        }
        catch
        {
            _isLockByShrink = false;
        }
    }

    public void Commit()
    {
        try
        {
            if (_isLockByCommit == false)
            {
                _isLockByCommit = true;
                string path = Path.Combine(_textPath, BlockIndex.ToString());
                if (!File.Exists(path))
                {
                    string json = JsonSerializer.Serialize(BlockMessage);
                    int blockCount = BlockMessage.Count;
                    WriteAllText(path, json);
                    BlockSizeInBytes = 0;
                    BlockMessage.RemoveRange(0, blockCount);
                }
                BlockIndex += 1;
                _isLockByCommit = false;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        catch
        {
            _isLockByCommit = false;
        }
    }

    private void WriteAllText(string path, string? contents)
    {
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(contents);
            }
        }
    }

    private string ReadAllText(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public void Load()
    {
        string[] filePaths = Directory.GetFiles(_textPath);
        foreach (string filePath in filePaths)
        {
            if (long.TryParse(filePath.Split('\\', '/').Last(), out long blockIndex))
            {
                Data[] datas = JsonSerializer.Deserialize<Data[]>(ReadAllText(filePath)) ?? Array.Empty<Data>();
                foreach (Data data in datas)
                {
                    if (data != null)
                    {
                        Blocks[data.Id] = new Block(blockIndex, data.DeleteUnixAt);
                        SizeAllBlockInBytes += data.Text.Length + data.FileSizeOfBytes;
                    }
                }
                BlockIndex = blockIndex > BlockIndex ? blockIndex : BlockIndex;
            }
        }
        BlockIndex += 1;
    }
}