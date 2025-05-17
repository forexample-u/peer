namespace Peer.Utils;

public static class HashHelper
{
    public static uint FastHash(in byte[] dataToHash)
    {
        int dataLength = dataToHash.Length;
        if (dataLength == 0)
        {
            return 0;
        }
        uint hashValue = (uint)dataLength;
        int remainingBytes = dataLength & 3;
        int numberOfLoops = dataLength >> 2;
        int currentIndex = 0;
        while (numberOfLoops > 0)
        {
            hashValue += (ushort)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8);
            uint tmp = (uint)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8) << 11 ^ hashValue;
            hashValue = hashValue << 16 ^ tmp;
            hashValue += hashValue >> 11;
            numberOfLoops--;
        }

        if (remainingBytes == 3)
        {
            hashValue += (ushort)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8);
            hashValue ^= hashValue << 16;
            hashValue ^= (uint)dataToHash[currentIndex] << 18;
            hashValue += hashValue >> 11;
        }
        if (remainingBytes == 2)
        {
            hashValue += (ushort)(dataToHash[currentIndex++] | dataToHash[currentIndex] << 8);
            hashValue ^= hashValue << 11;
            hashValue += hashValue >> 17;
        }
        if (remainingBytes == 1)
        {
            hashValue += dataToHash[currentIndex];
            hashValue ^= hashValue << 10;
            hashValue += hashValue >> 1;
        }

        hashValue ^= hashValue << 3;
        hashValue += hashValue >> 5;
        hashValue ^= hashValue << 4;
        hashValue += hashValue >> 17;
        hashValue ^= hashValue << 25;
        hashValue += hashValue >> 6;
        return hashValue;
    }

    public static long Hash(byte[] bytes)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in bytes)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 1) != 0)
                {
                    crc = (crc >> 1) ^ 0xEDB88320;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        return (long)(crc ^ 0xFFFFFFFF);
    }
}