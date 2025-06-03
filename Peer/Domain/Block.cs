namespace Peer.Domain;

public class Block
{
    public Block(long index, long deleteUnix)
    {
        Index = index;
        DeleteUnixAt = deleteUnix;
    }

    public long Index { get; set; }
    public long DeleteUnixAt { get; set; }
}