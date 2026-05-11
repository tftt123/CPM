using CpmServer.Common;
using CpmServer.DTOs.Customer;

namespace CpmServer.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetListAsync(int pageNum, int pageSize, string? keyword);
    Task<CustomerDto?> GetByIdAsync(long id);
    Task<long> AddAsync(CustomerDto dto);
    Task UpdateAsync(long id, CustomerDto dto);
    Task DeleteAsync(long id);
}