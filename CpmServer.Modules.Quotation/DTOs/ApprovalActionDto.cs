namespace CpmServer.Modules.Quotation.DTOs;

public class ApprovalActionDto
{
    public long QuotationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public long? TransferToUserId { get; set; }
    public decimal? ReviewCost { get; set; }
}
