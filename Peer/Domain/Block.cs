namespace Peer.Domain;

public class Block
{
    public Block(long index, long deleteUnix)
    {
        Index = index;
        DeleteUnix = deleteUnix;
    }

    public long Index { get; set; }
    public long DeleteUnix { get; set; }
}