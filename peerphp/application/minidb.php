<?php
require_once __DIR__ . '/../domain/data.php';
require_once __DIR__ . '/../domain/datainformation.php';
require_once __DIR__ . '/../domain/message.php';
require_once __DIR__ . '/../config.php';

class Minidb {
    public function write(mysqli $mysqli, Message $message, float $sizeAllQueryMB): string {
        $config = new Config();
        $contentType = $message->file->contentType ?? "";
        $filename = $message->file->fileName ?? "";
        $filelength = $message->file->length ?? 0;
        $fileSizeMB = (float)$filelength / 1048576.0;
        $textSizeMB = (float)(isset($message->text) ? strlen($message->text) : 0) / 1048576.0;
        $querySizeMB = $textSizeMB + $fileSizeMB;
        if ($textSizeMB > $config::$MaxSizeTextMB || $fileSizeMB > $config::$MaxSizeFileMB)
        {
            return "0";
        }
        $finddelete_unix = $this->get_delete_unix($mysqli, $message->id);
        if ($finddelete_unix !== "0")
        {
            return "0";
        }
        $addSecond = $querySizeMB > $config::$LimitOtherSmallSizeOneQueryMB ? $config::$LimitOtherBigMessageSecond : $config::$LimitOtherSmallMessageSecond;
        if ($sizeAllQueryMB <= $config::$Limit1MB)
        {
            $addSecond = $querySizeMB > $config::$Limit1SmallSizeOneQueryMB ? $config::$Limit1BigMessageSecond : $config::$Limit1SmallMessageSecond;
        }
        $date = new DateTime('now', new DateTimeZone('UTC'));
        $date->modify("+" . (string)$addSecond . ' seconds');
        $deleteUnixAt = $date->format('U');
        $stmt = $mysqli->prepare("INSERT INTO data (unique_key, text, filename, filelength, content_type, query_size_mb, bytes, delete_unix_at) VALUES (?, ?, ?, ?, ?, ?, ?, $deleteUnixAt);");
        $stmt->bind_param("sssisdb", $message->id, $message->text, $filename, $filelength, $contentType, $querySizeMB, $null);
        $stmt->send_long_data(6, $message->file->stream);
        $stmt->execute();
        $mysqli->query("UPDATE datainfo SET count_query = count_query + 1, query_size_mb = query_size_mb + " . (string)$querySizeMB . ";");
        return $deleteUnixAt;
    }

    function get_info(mysqli $mysqli) : DataInformation {
        $datainfo = new DataInformation(0, 0.0);
        $sql = "SELECT count_query, query_size_mb FROM datainfo WHERE id = 1";
        if ($result = $mysqli->query($sql)) {
            if ($row = $result->fetch_assoc()) {
                $countQuery = $row['count_query'];
                $querySizeMB = $row['query_size_mb'];
                $datainfo = new DataInformation($countQuery, $querySizeMB);
            }
            $result->free();
        }
        return $datainfo;
    }

    function get_delete_unix(mysqli $mysqli, string $id): string {
        $deleteUnixAt = "0";
        $sql = "SELECT delete_unix_at FROM data WHERE unique_key = ? LIMIT 1";
        if ($stmt = $mysqli->prepare($sql)) {
            $stmt->bind_param("s", $id);
            if ($stmt->execute()) {
                $stmt->bind_result($deleteUnixAt);
                $stmt->fetch();
            }
            $stmt->close();
        }
        return $deleteUnixAt;
    }

    function get_text(mysqli $mysqli, string $id): string {
        $text = "";
        $sql = "SELECT text FROM data WHERE unique_key = ? LIMIT 1";
        if ($stmt = $mysqli->prepare($sql)) {
            $stmt->bind_param("s", $id);
            if ($stmt->execute()) {
                $stmt->bind_result($text);
                $stmt->fetch();
            }
            $stmt->close();
        }
        return $text;
    }

    function get_file(mysqli $mysqli, string $id) : Data {
        $data = new Data("", "", 0, "", 0, 0, "", 0, null);
        $sql = "SELECT filename, filelength, delete_unix_at, content_type, query_size_mb, bytes FROM data WHERE unique_key = ? LIMIT 1";
        if ($stmt = $mysqli->prepare($sql)) {
            $stmt->bind_param("s", $id);
            $stmt->execute();
            $result = $stmt->get_result();
            if ($row = $result->fetch_assoc()) {
                $filename = $row['filename'];
                $filelength =  (int)$row['filelength'];
                $deleteUnixAt = $row['delete_unix_at'];
                $contentType = $row['content_type'];
                $bytes = is_null($row['bytes']) ? null : $row['bytes'];
                $data = new Data($id, "", 0, $filename, $filelength, $deleteUnixAt, $contentType, 0, $bytes);
            }
            $stmt->close();
        }
        return $data;
    }

    function shrink(mysqli $mysqli) : void {
        $now = new DateTime('now', new DateTimeZone('UTC'));
        $nowUnix = $now->format('U');
        $mysqli->query("UPDATE datainfo SET query_size_mb = query_size_mb - (SELECT SUM(query_size_mb) FROM data WHERE delete_unix_at < " . $nowUnix . ");");
        $mysqli->query("DELETE FROM data WHERE delete_unix_at < " . $nowUnix);
    }

    function init(mysqli $mysqli) : void {
        $sql = "
            CREATE TABLE IF NOT EXISTS data (
                id BIGINT AUTO_INCREMENT PRIMARY KEY,
                unique_key VARCHAR(128) UNIQUE,
                text TEXT,
                filename VARCHAR(512),
                filelength INT,
                content_type VARCHAR(128),
                delete_unix_at BIGINT,
                query_size_mb FLOAT,
                bytes BLOB
            );
            CREATE TABLE IF NOT EXISTS datainfo (
                id INT PRIMARY KEY,
                count_query INT,
                query_size_mb FLOAT
            );
            INSERT INTO datainfo (id, count_query, query_size_mb)
            SELECT 1, 0, 0 FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM datainfo WHERE id = 1);
        ";
    
        if ($mysqli->multi_query($sql)) {
            do {
                if ($result = $mysqli->store_result()) {
                    $result->free();
                }
            } while ($mysqli->more_results() && $mysqli->next_result());
        }
    }
}
?>