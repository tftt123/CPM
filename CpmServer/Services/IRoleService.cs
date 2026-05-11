using CpmServer.Common;
using CpmServer.DTOs.Role;

namespace CpmServer.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetListAsync(string? site);
    Task<RoleDto?> GetByIdAsync(long id);
    Task<long> CreateAsync(RoleCreateDto dto);
    Task UpdateAsync(long id, RoleUpdateDto dto);
    Task DeleteAsync(long id);
}
