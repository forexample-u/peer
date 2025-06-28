namespace Peer.Domain;

public class DataInformation
{
    public DataInformation(long countQuery, long querySizeOfBytes)
    {
        CountQuery = countQuery;
        QuerySizeOfBytes = querySizeOfBytes;
    }

    public int Id { get; set; } = 1;
    public long CountQuery { get; set; }
    public long QuerySizeOfBytes { get; set; }
}