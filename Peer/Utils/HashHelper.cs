namespace Peer.Utils;

public static class HashHelper
{
    public static ulong Hash(string input)
    {
        ulong hash = 5381;
        foreach (char ch in input)
        {
            hash = (hash << 5) + hash + ch;
        }
        return hash;
    }
}