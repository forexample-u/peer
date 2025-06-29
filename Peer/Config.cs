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
    public static string? ConnectionString { get; set; }

    public static void Load(string fileName = "appsettings.json")
    {
        string json = File.ReadAllText(fileName);
        JObject data = JObject.Parse(json);
        JToken peerMain = data["PeerMain"];
        JToken peerAdditional = data["PeerAdditional"];
        MaxSizeOneQuery = Convert.ToInt32(peerMain["MaxSizeOneQuery"]);
        MaxSizeText = Convert.ToInt32(peerMain["MaxSizeText"]);
        Limit1 = Convert.ToInt32(peerMain["Limit1"]);
        Limit1BigMessageSecond = Convert.ToInt32(peerMain["Limit1BigMessageSecond"]);
        Limit1SmallMessageSecond = Convert.ToInt32(peerMain["Limit1SmallMessageSecond"]);
        Limit1SmallSizeOneQuery = Convert.ToInt32(peerMain["Limit1SmallSizeOneQuery"]);
        LimitOtherBigMessageSecond = Convert.ToInt32(peerMain["LimitOtherBigMessageSecond"]);
        LimitOtherSmallMessageSecond = Convert.ToInt32(peerMain["LimitOtherSmallMessageSecond"]);
        LimitOtherSmallSizeOneQuery = Convert.ToInt32(peerMain["LimitOtherSmallSizeOneQuery"]);
        TextPath = Convert.ToString(peerAdditional["TextPath"]);
        FilePath = Convert.ToString(peerAdditional["FilePath"]);
        WebUrlFilePath = Convert.ToString(peerAdditional["WebUrlFilePath"]);
        CommitBlockSecond = Convert.ToInt32(peerAdditional["CommitBlockSecond"]);
        AvgSizeBlock = Convert.ToInt32(peerAdditional["AvgSizeBlock"]);
        ConnectionString = Convert.ToString(peerAdditional["ConnectionString"]);
    }
}