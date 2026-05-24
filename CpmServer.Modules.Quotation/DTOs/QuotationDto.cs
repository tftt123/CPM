namespace CpmServer.Modules.Quotation.DTOs;

public class QuotationDto
{
    public long? Id { get; set; }
    public string QuotationNo { get; set; } = string.Empty;
    public string RfqNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCurrency { get; set; }
    public decimal? TotalAmount { get; set; }
    public long? CurrentStepId { get; set; }
    public string? CurrentStepName { get; set; }
    public int Status { get; set; }
    public long CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string? Site { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<long> FileIds { get; set; } = new();
    public List<QuotationItemDto> Items { get; set; } = new();
}

public class QuotationCreateDto
{
    public long OpportunityId { get; set; }
    public string RfqNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public List<QuotationItemDto> Items { get; set; } = new();
    public string? Site { get; set; }
    public List<long> FileIds { get; set; } = new();
}

public class QuotationItemDto
{
    public long? Id { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Qty { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal? LineAmount { get; set; }
    public string? ProcessType { get; set; }
    public string? EquipmentType { get; set; }
    public string? Equipment { get; set; }
    public decimal? CycleTime { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? Cost { get; set; }
    public decimal? PackagingCost { get; set; }
    public decimal? TransportCost { get; set; }
    public bool IsProcessRow { get; set; }
}

public class QuotationSubmitDto
{
    public long TechReviewerId { get; set; }
}
