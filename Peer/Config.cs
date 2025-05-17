namespace Peer;

public class Config
{
    public static long MaxOneFileSize { get; set; } = 3145728;
    public static long MinDebufSizeBytes { get; set; } = 28000000;
    public static long MaxLengthText { get; set; } = 1048576;
    public static long DebufSecond { get; set; } = 53;
    public static long BigMessageSecond { get; set; } = 60;
    public static long SmallMessageSecond { get; set; } = 960;
    public static long First100SmallMessageSecond { get; set; } = 86400;
    public static long SmallTextLength { get; set; } = 5000;
    public static long SmallFileSizeBytes { get; set; } = 10240;
}