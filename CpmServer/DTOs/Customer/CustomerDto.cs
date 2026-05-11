namespace CpmServer.DTOs.Customer;

public class CustomerDto
{
    public long? Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? ContactName { get; set; }
    public string? Site { get; set; }
}