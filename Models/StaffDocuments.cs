namespace POE_CoffeeNChill.Models;

public class StaffDocuments
{
    public string FileName { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string ContentType { get; set; } ="application/octet-stream";
}