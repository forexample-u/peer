using Newtonsoft.Json.Linq;

namespace Peer;

public class Config
{
    public static int MaxSizeOneQuery { get; set; }
    public static int MaxSizeText { get; set; }
    public static int Limit1 { get; set; }
    public static int Limit1BigMessageSecond { get; set; }
    public static int Limit1SmallMessageSecond { get; set; }
    public static int Limit1SmallSizeOneQuery { get; set; }
    public static int LimitOtherBigMessageSecond { get; set; }
    public static int LimitOtherSmallMessageSecond { get; set; }
    public static int LimitOtherSmallSizeOneQuery { get; set; }
    public static string? TextPath { get; set; }
    public static string? FilePath { get; set; }
    public static string? WebUrlFilePath { get; set; }
    public static int CommitBlockSecond { get; set; }
    public static int AvgSizeBlock { get; set; }

    public static void Load(string fileName = "appsettings.json")
    {
        var config = JObject.Parse(File.ReadAllText(fileName))["PeerMain"];
        MaxSizeOneQuery = Convert.ToInt32(config["MaxSizeOneQuery"]);
        MaxSizeText = Convert.ToInt32(config["MaxSizeText"]);
        Limit1 = Convert.ToInt32(config["Limit1"]);
        Limit1BigMessageSecond = Convert.ToInt32(config["Limit1BigMessageSecond"]);
        Limit1SmallMessageSecond = Convert.ToInt32(config["Limit1SmallMessageSecond"]);
        Limit1SmallSizeOneQuery = Convert.ToInt32(config["Limit1SmallSizeOneQuery"]);
        LimitOtherBigMessageSecond = Convert.ToInt32(config["LimitOtherBigMessageSecond"]);
        LimitOtherSmallMessageSecond = Convert.ToInt32(config["LimitOtherSmallMessageSecond"]);
        LimitOtherSmallSizeOneQuery = Convert.ToInt32(config["LimitOtherSmallSizeOneQuery"]);
        config = JObject.Parse(File.ReadAllText(fileName))["PeerAdditional"];
        TextPath = Convert.ToString(config["TextPath"]);
        FilePath = Convert.ToString(config["FilePath"]);
        WebUrlFilePath = Convert.ToString(config["WebUrlFilePath"]);
        CommitBlockSecond = Convert.ToInt32(config["CommitBlockSecond"]);
        AvgSizeBlock = Convert.ToInt32(config["AvgSizeBlock"]);
    }
}