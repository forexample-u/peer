using Npgsql;
using Peer.Domain;

namespace Peer.Application;

public class Minidb
{
    public long Write(Message message, long sizeAllBlockInBytes = 0)
    {
        string contentType = message.File?.ContentType ?? "";
        string fileName = message.File?.FileName ?? "";
        int fileSize = (int)(message.File?.Length ?? 0);
        long textSize = message.Text?.Length ?? 0;
        long querySize = textSize + fileSize;
        if (textSize > Config.MaxSizeText || querySize > Config.MaxSizeOneQuery || message.Id.Length > 255)
        {
            return 0;
        }
        Data finddata = Get(message.Id);
        if (finddata != null)
        {
            return 0;
        }

        long addSecond = querySize > Config.LimitOtherSmallSizeOneQuery ? Config.LimitOtherBigMessageSecond : Config.LimitOtherSmallMessageSecond;
        if (sizeAllBlockInBytes <= Config.Limit1)
        {
            addSecond = querySize > Config.Limit1SmallSizeOneQuery ? Config.Limit1BigMessageSecond : Config.Limit1SmallMessageSecond;
        }
        long deleteUnixAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + addSecond;
        byte[] bytes = [];
        if (message.File != null)
        {
            using (MemoryStream memoryStream = new())
            {
                message.File.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }
        }
        Data data = new Data(message.Id, message.Text, 0, fileName, (int)deleteUnixAt, contentType, fileSize, bytes);
        string sql = $"UPDATE datainfo set count_query = count_query + 1; UPDATE datainfo set query_size_of_bytes = query_size_of_bytes + {querySize};" +
            "INSERT INTO data (unique_key, text, filename, delete_unix_at, content_type, file_size_of_bytes, bytes) " +
            "VALUES (@unique_key, @text, @filename, @delete_unix_at, @content_type, @file_size_of_bytes, @bytes);";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.Parameters.AddWithValue("@unique_key", message.Id);
            command.Parameters.AddWithValue("@text", message.Text);
            command.Parameters.AddWithValue("@filename", fileName);
            command.Parameters.AddWithValue("@delete_unix_at", deleteUnixAt);
            command.Parameters.AddWithValue("@content_type", contentType);
            command.Parameters.AddWithValue("@file_size_of_bytes", fileSize);
            command.Parameters.AddWithValue("@bytes", bytes);
            command.ExecuteNonQuery();
        }
        return deleteUnixAt;
    }

    public Data Get(string id)
    {
        Data data = null;
        string sql = "SELECT text, filename, delete_unix_at, content_type, file_size_of_bytes, bytes FROM data WHERE unique_key = @id;";
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
                    string text = reader.GetString(0);
                    string filename = reader.GetString(1);
                    int deleteUnixAt = reader.GetInt32(2);
                    string contentType = reader.GetString(3);
                    int fileSizeOfBytes = reader.GetInt32(4);
                    byte[] bytes = reader.IsDBNull(5) ? [] : (byte[])reader[5];
                    data = new Data(id, text, 0, filename, deleteUnixAt, contentType, fileSizeOfBytes, bytes);
                }
            }
        }
        return data;
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
        long nowUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
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
        string sql = "CREATE TABLE IF NOT EXISTS data (id SERIAL PRIMARY KEY, unique_key VARCHAR(255) UNIQUE, text TEXT, filename TEXT, delete_unix_at BIGINT, content_type TEXT, file_size_of_bytes INTEGER, bytes BYTEA);" +
            "CREATE TABLE IF NOT EXISTS datainfo (id INTEGER PRIMARY KEY, count_query BIGINT, query_size_of_bytes BIGINT);" +
            "INSERT INTO datainfo (id, count_query, query_size_of_bytes) SELECT 1, 0, 0 WHERE NOT EXISTS (SELECT 1 FROM datainfo WHERE id = 1);";
        using NpgsqlConnection connection = new(Config.ConnectionString);
        connection.Open();
        using (NpgsqlCommand command = new(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}