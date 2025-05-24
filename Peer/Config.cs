namespace Peer;

public class Config
{
    public static int MaxSizeOneQuery { get; set; } = 3200000;
    public static int MaxSizeText { get; set; } = 1050000;
    public static int Limit1 { get; set; } = 28000000;
    public static int Limit1BigMessageSecond { get; set; } = 60;
    public static int Limit1SmallMessageSecond { get; set; } = 960;
    public static int Limit1SmallSizeOneQuery { get; set; } = 120000;
    public static int LimitOtherBigMessageSecond { get; set; } = 7;
    public static int LimitOtherSmallMessageSecond { get; set; } = 65;
    public static int LimitOtherSmallSizeOneQuery { get; set; } = 5100;
}