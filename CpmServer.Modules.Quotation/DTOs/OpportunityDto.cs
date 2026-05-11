namespace CpmServer.Modules.Quotation.DTOs;

public class OpportunityDto
{
    public long? Id { get; set; }
    public string OpportunityNo { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? ExpectedAmount { get; set; }
    public DateTime? QuoteDeadline { get; set; }
    public string Stage { get; set; } = "NEW";
    public int Status { get; set; }
    public long OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? Site { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OpportunityCreateDto
{
    public long CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal? ExpectedAmount { get; set; }
    public DateTime? QuoteDeadline { get; set; }
    public string? Site { get; set; }
}

public class OpportunityStageUpdateDto
{
    public string Stage { get; set; } = string.Empty;
}
