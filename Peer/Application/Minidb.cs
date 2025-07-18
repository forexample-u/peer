﻿using Peer.Domain;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Peer.Application;

public class Minidb
{
    public Minidb(string textPath, string filePath)
    {
        _textPath = textPath;
        _filePath = filePath;
        Load();
    }

    private readonly string _textPath, _filePath;
    private bool _isLockByCommit = false;
    private bool _isLockByShrink = false;
    private List<Data> _blockData = new();
    private ConcurrentDictionary<long, Block> _blocks = new();
    public long BlockIndex { get; private set; } = 0;
    public long BlockInBytes { get; private set; } = 0;
    public long SizeAllBlockInBytes { get; private set; } = 0;

    public long Write(long id, Message message)
    {
        string contentType = message.File?.ContentType ?? "";
        string fileName = message.File?.FileName ?? "";
        long fileSize = message.File?.Length ?? 0;
        long textSize = message.Text?.Length ?? 0;
        long querySize = textSize + fileSize;
        if (textSize > Config.MaxSizeText || querySize > Config.MaxSizeOneQuery || _blocks.ContainsKey(id))
        {
            return 0;
        }

        if (message.File != null)
        {
            using var stream = new FileStream(Path.Combine(_filePath, id.ToString() + Path.GetExtension(fileName)), FileMode.Create);
            message.File?.CopyTo(stream);
        }

        long addSecond = querySize > Config.LimitOtherSmallSizeOneQuery ? Config.LimitOtherBigMessageSecond : Config.LimitOtherSmallMessageSecond;
        if (SizeAllBlockInBytes <= Config.Limit1)
        {
            addSecond = querySize > Config.Limit1SmallSizeOneQuery ? Config.Limit1BigMessageSecond : Config.Limit1SmallMessageSecond;
        }
        long deleteUnixAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + addSecond;
        _blockData.Add(new Data(id, message.Text, 0, fileName, deleteUnixAt, contentType, fileSize));
        _blocks[id] = new Block(BlockIndex, deleteUnixAt);
        SizeAllBlockInBytes += querySize;
        BlockInBytes += querySize;
        return deleteUnixAt;
    }

    public Data Get(long id)
    {
        for (int i = _blockData.Count - 1; i >= 0; i--)
        {
            if (_blockData[i].Id == id)
            {
                return _blockData[i];
            }
        }

        if (_blocks.TryGetValue(id, out Block? block))
        {
            string path = Path.Combine(_textPath, block.Index.ToString());
            if (!File.Exists(path)) return null;
            List<Data> datas = JsonSerializer.Deserialize<List<Data>>(ReadAllText(path)) ?? new List<Data>();
            return datas.FirstOrDefault(x => x.Id == id);
        }
        return null;
    }

    public void Shrink()
    {
        if (_isLockByShrink) return;
        _isLockByShrink = true;
        try
        {
            long nowUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            HashSet<long> indexes = new();
            foreach (var block in _blocks.Values)
            {
                if (block.DeleteUnixAt < nowUnix)
                {
                    indexes.Add(block.Index);
                }
            }
            foreach (long index in indexes)
            {
                try
                {
                    string path = Path.Combine(_textPath, index.ToString());
                    if (!File.Exists(path)) continue;
                    List<Data> datas = JsonSerializer.Deserialize<List<Data>>(ReadAllText(path)) ?? new List<Data>();
                    List<long> deleteIds = new();
                    long sizeReduced = 0;
                    for (int i = datas.Count - 1; i >= 0; i--)
                    {
                        Data data = datas[i];
                        if (data.DeleteUnixAt < nowUnix)
                        {
                            string fullpath = Path.Combine(_filePath, data.Id.ToString() + Path.GetExtension(data.Filename ?? ""));
                            if (File.Exists(fullpath))
                            {
                                File.Delete(fullpath);
                            }
                            sizeReduced += data.Text.Length + data.FileSizeOfBytes;
                            deleteIds.Add(data.Id);
                            datas.RemoveAt(i);
                        }
                    }
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
                        _blocks.TryRemove(id, out Block _);
                    }
                    SizeAllBlockInBytes -= sizeReduced;
                } catch { }
            }
        } catch { }
        _isLockByShrink = false;
    }

    public void Commit()
    {
        if (_isLockByCommit) return;
        _isLockByCommit = true;
        try
        {
            string path = Path.Combine(_textPath, BlockIndex.ToString());
            BlockIndex += 1;
            if (!File.Exists(path))
            {
                int blockCount = _blockData.Count;
                WriteAllText(path, JsonSerializer.Serialize(_blockData));
                _blockData.RemoveRange(0, blockCount);
                BlockInBytes = 0;
            }
        } catch { }
        _isLockByCommit = false;
    }

    public void Load()
    {
        foreach (string filePath in Directory.GetFiles(_textPath))
        {
            if (long.TryParse(filePath.Split('\\', '/').Last(), out long blockIndex))
            {
                List<Data> datas = JsonSerializer.Deserialize<List<Data>>(ReadAllText(filePath)) ?? new List<Data>();
                foreach (Data data in datas)
                {
                    if (data == null) continue;
                    _blocks[data.Id] = new Block(blockIndex, data.DeleteUnixAt);
                    SizeAllBlockInBytes += data.Text.Length + data.FileSizeOfBytes;
                }
                BlockIndex = blockIndex > BlockIndex ? blockIndex : BlockIndex;
            }
        }
        BlockIndex += 1;
    }

    private static void WriteAllText(string path, string? contents)
    {
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(contents);
        }
    }

    private static string ReadAllText(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}