using CpmServer.Common;
using CpmServer.DTOs.Product;

namespace CpmServer.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetListAsync(int pageNum, int pageSize, string? keyword);
    Task<ProductDto?> GetByIdAsync(long id);
    Task<long> AddAsync(ProductDto dto);
    Task UpdateAsync(long id, ProductDto dto);
    Task DeleteAsync(long id);
}
