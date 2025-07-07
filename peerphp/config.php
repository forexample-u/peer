<?php
class Config {
    public static float $MaxSizeFileMB;
    public static float $MaxSizeTextMB;
    public static float $Limit1MB;
    public static int $Limit1BigMessageSecond;
    public static int $Limit1SmallMessageSecond;
    public static float $Limit1SmallSizeOneQueryMB;
    public static int $LimitOtherBigMessageSecond;
    public static int $LimitOtherSmallMessageSecond;
    public static float $LimitOtherSmallSizeOneQueryMB;
    public static ?string $TextPath = null;
    public static ?string $FilePath = null;
    public static ?string $WebUrlFilePath = null;
    public static int $CommitBlockSecond;
    public static int $AvgSizeBlock;
    public static ?string $ConnectionString = null;
    public static ?string $MysqlHost = null;
    public static int $MysqlPort;
    public static ?string $MysqlUser = null;
    public static ?string $MysqlPassword = null;
    public static ?string $MysqlDatabase = null;
    public static ?string $SecretUrlInitDatabase = null;

    public static function load(string $fileName = "appsettings.json"): void {
        $json = file_get_contents($fileName);
        $data = json_decode($json, true);
        $peerMain = $data['PeerMain'];
        $peerAdditional = $data['PeerAdditional'];
        self::$MaxSizeFileMB = (float)($peerMain['MaxSizeFileMB'] ?? 0);
        self::$MaxSizeTextMB = (float)($peerMain['MaxSizeTextMB'] ?? 0);
        self::$Limit1MB = (float)($peerMain['Limit1MB'] ?? 0);
        self::$Limit1BigMessageSecond = (int)($peerMain['Limit1BigMessageSecond'] ?? 0);
        self::$Limit1SmallMessageSecond = (int)($peerMain['Limit1SmallMessageSecond'] ?? 0);
        self::$Limit1SmallSizeOneQueryMB = (float)($peerMain['Limit1SmallSizeOneQueryMB'] ?? 0);
        self::$LimitOtherBigMessageSecond = (int)($peerMain['LimitOtherBigMessageSecond'] ?? 0);
        self::$LimitOtherSmallMessageSecond = (int)($peerMain['LimitOtherSmallMessageSecond'] ?? 0);
        self::$LimitOtherSmallSizeOneQueryMB = (float)($peerMain['LimitOtherSmallSizeOneQueryMB'] ?? 0);

        self::$TextPath = isset($peerAdditional['TextPath']) ? (string)$peerAdditional['TextPath'] : null;
        self::$FilePath = isset($peerAdditional['FilePath']) ? (string)$peerAdditional['FilePath'] : null;
        self::$WebUrlFilePath = isset($peerAdditional['WebUrlFilePath']) ? (string)$peerAdditional['WebUrlFilePath'] : null;
        self::$CommitBlockSecond = (int)($peerAdditional['CommitBlockSecond'] ?? 0);
        self::$AvgSizeBlock = (int)($peerAdditional['AvgSizeBlock'] ?? 0);
        self::$ConnectionString = isset($peerAdditional['ConnectionString']) ? (string)$peerAdditional['ConnectionString'] : null;

        self::$MysqlHost = isset($peerAdditional['MysqlHost']) ? (string)$peerAdditional['MysqlHost'] : null;
        self::$MysqlPort = isset($peerAdditional['MysqlPort']) ? (int)$peerAdditional['MysqlPort'] : null;
        self::$MysqlUser = isset($peerAdditional['MysqlUser']) ? (string)$peerAdditional['MysqlUser'] : null;
        self::$MysqlPassword = isset($peerAdditional['MysqlPassword']) ? (string)$peerAdditional['MysqlPassword'] : null;
        self::$MysqlDatabase = isset($peerAdditional['MysqlDatabase']) ? (string)$peerAdditional['MysqlDatabase'] : null;
        self::$SecretUrlInitDatabase = isset($peerAdditional['SecretUrlInitDatabase']) ? (string)$peerAdditional['SecretUrlInitDatabase'] : null;
    }
}
?>