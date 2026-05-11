namespace CpmServer.Modules.Quotation.DTOs;

public class FileRecordDto
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public string? ModuleType { get; set; }
    public long? BusinessId { get; set; }
    public DateTime CreatedAt { get; set; }
}
