namespace CpmServer.DTOs.Product;

public class ProductDto
{
    public long? Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Material { get; set; }
    public string? SurfaceTreatment { get; set; }
    public string? Site { get; set; }
}
