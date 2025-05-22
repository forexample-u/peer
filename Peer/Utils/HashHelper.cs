namespace Peer.Utils;

public static class HashHelper
{
    public static ulong Hash(string input)
    {
        ulong hash = 5381;
        foreach (char ch in input)
        {
            hash = hash * 33 + ch;
        }
        return hash;
    }
}