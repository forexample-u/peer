namespace Peer.Domain;

public class Block
{
    public Block(long index, long deleteUnixAt)
    {
        Index = index;
        DeleteUnixAt = deleteUnixAt;
    }

    public long Index { get; set; }
    public long DeleteUnixAt { get; set; }
}