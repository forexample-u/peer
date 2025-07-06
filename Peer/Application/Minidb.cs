using Npgsql;
using Peer.Domain;

namespace Peer.Application;

public class Minidb
{
    public string Write(Message message, long sizeAllBlockInBytes = 0)
    {
        string contentType = message.File?.ContentType ?? "";
        string fileName = message.File?.FileName ?? "";
        int fileSize = (int)(message.File?.Length ?? 0);
        int textSize = message.Text?.Length ?? 0;
        int querySize = textSize + fileSize;
        if (textSize > Config.MaxSizeText || querySize > Config.MaxSizeOneQuery || message.Id.Length > 256)
        {
            return "0";
        }
        string finddata = GetText(message.Id);
        if (finddata == null)
        {
            return "0";
        }

        int addSecond = querySize > Config.LimitOtherSmallSizeOneQuery ? Config.LimitOtherBigMessageSecond : Config.LimitOtherSmallMessageSecond;
        if (sizeAllBlockInBytes <= Config.Limit1)
        {
            addSecond = querySize > Config.Limit1SmallSizeOneQuery ? Config.Limit1BigMessageSecond : Config.Limit1SmallMessageSecond;
        }
        byte[] bytes = [];
        if (message.File != null)
        {
            using (MemoryStream memoryStream = new())
            {
                message.File.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }
        }
        string deleteUnixAt = new DateTimeOffset(DateTime.UtcNow.AddSeconds(addSecond)).ToUnixTimeSeconds().ToString();
        string sql = $"UPDATE datainfo set count_query = count_query + 1; UPDATE datainfo set query_size_of_bytes = query_size_of_bytes + {querySize};" +
            "INSERT INTO data (unique_key, text, filename, delete_unix_at, content_type, file_size_of_bytes, bytes) " +
            $"VALUES (@unique_key, @text, @filename, {deleteUnixAt}, @content_type, @file_size_of_bytes, @bytes);";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.Parameters.AddWithValue("@unique_key", message.Id);
            command.Parameters.AddWithValue("@text", message.Text);
            command.Parameters.AddWithValue("@filename", fileName);
            command.Parameters.AddWithValue("@content_type", contentType);
            command.Parameters.AddWithValue("@file_size_of_bytes", fileSize);
            command.Parameters.AddWithValue("@bytes", bytes);
            command.ExecuteNonQuery();
        }
        return deleteUnixAt;
    }

    public Data GetFile(string id)
    {
        Data data = null;
        string sql = "SELECT filename, content_type, file_size_of_bytes, bytes FROM data WHERE unique_key = @id;";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                if (reader.HasRows)
                {
                    string filename = reader.GetString(0);
                    string contentType = reader.GetString(1);
                    int fileSizeOfBytes = reader.GetInt32(2);
                    byte[] bytes = reader.IsDBNull(3) ? [] : (byte[])reader[3];
                    data = new Data(id, "", 0, filename, 0, contentType, fileSizeOfBytes, bytes);
                }
            }
        }
        return data;
    }

    public string GetText(string id)
    {
        string text = null;
        string sql = "SELECT text FROM data WHERE unique_key = @id;";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                if (reader.HasRows)
                {
                    string filename = reader.GetString(0);
                }
            }
        }
        return text;
    }

    public DataInformation GetInfo()
    {
        DataInformation data = null;
        string sql = "SELECT count_query, query_size_of_bytes FROM datainfo WHERE id = 1;";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                if (reader.HasRows)
                {
                    long countQuery = reader.GetInt64(0);
                    long querySizeOfBytes = reader.GetInt64(1);
                    data = new DataInformation(countQuery, querySizeOfBytes);
                }
            }
        }
        return data;
    }

    public void Shrink()
    {
        string nowUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string sql = $"DELETE FROM data WHERE delete_unix_at < {nowUnix};";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    public static void Init()
    {
        string sql = @"
            CREATE TABLE IF NOT EXISTS data (
                id SERIAL PRIMARY KEY,
                unique_key VARCHAR(227) UNIQUE,
                text TEXT,
                filename TEXT,
                delete_unix_at BIGINT,
                content_type TEXT,
                file_size_of_bytes INTEGER,
                bytes BYTEA
            );
            CREATE TABLE IF NOT EXISTS datainfo (
                id INTEGER PRIMARY KEY,
                count_query BIGINT,
                query_size_of_bytes BIGINT
            );
            INSERT INTO datainfo (id, count_query, query_size_of_bytes)
            SELECT 1, 0, 0
            WHERE NOT EXISTS (SELECT 1 FROM datainfo WHERE id = 1);
        ";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}