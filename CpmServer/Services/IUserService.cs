using CpmServer.Common;
using CpmServer.DTOs.User;

namespace CpmServer.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetListAsync(int pageNum, int pageSize, string? keyword);
    Task<UserDto?> GetByIdAsync(long id);
    Task<long> CreateAsync(UserCreateDto dto);
    Task UpdateAsync(long id, UserUpdateDto dto);
    Task DeleteAsync(long id);
    Task ResetPasswordAsync(long id, string newPassword);
    Task UpdateProfileAsync(long userId, ProfileUpdateDto dto);
    Task ChangePasswordAsync(long userId, string currentPassword, string newPassword);
}
