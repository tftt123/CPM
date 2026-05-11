using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.User;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class UserService : IUserService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UserService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UserDto>> GetListAsync(int pageNum, int pageSize, string? keyword)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(u => u.Site == _currentUser.Site);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(u =>
                u.Username.Contains(keyword) ||
                (u.RealName != null && u.RealName.Contains(keyword)) ||
                (u.Email != null && u.Email.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = new List<UserDto>();
        foreach (var user in list)
        {
            var roleIds = await _db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _db.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.RoleName)
                .ToListAsync();

            dtoList.Add(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                RealName = user.RealName,
                Phone = user.Phone,
                Email = user.Email,
                IsActive = user.IsActive,
                Site = user.Site,
                RoleIds = roleIds,
                Roles = roles,
                CreatedAt = user.CreatedAt
            });
        }

        return PagedResult<UserDto>.Of(dtoList, total, pageNum, pageSize);
    }

    public async Task<UserDto?> GetByIdAsync(long id)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(u => u.Site == _currentUser.Site);
        }
        var user = await query.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.RoleName)
            .ToListAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            RealName = user.RealName,
            Phone = user.Phone,
            Email = user.Email,
            IsActive = user.IsActive,
            Site = user.Site,
            RoleIds = roleIds,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<long> CreateAsync(UserCreateDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            throw new BusinessException("用户名已存在");

        var entity = new SysUser
        {
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RealName = dto.RealName,
            Phone = dto.Phone,
            Email = dto.Email,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        // 分配角色
        foreach (var roleId in dto.RoleIds)
        {
            _db.UserRoles.Add(new SysUserRole { UserId = entity.Id, RoleId = roleId });
        }
        await _db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, UserUpdateDto dto)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) throw new BusinessException("用户不存在");

        entity.RealName = dto.RealName;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Site = dto.Site;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.Now;

        // 更新角色
        var existingRoles = _db.UserRoles.Where(ur => ur.UserId == id);
        _db.UserRoles.RemoveRange(existingRoles);

        foreach (var roleId in dto.RoleIds)
        {
            _db.UserRoles.Add(new SysUserRole { UserId = id, RoleId = roleId });
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return;

        // 删除关联角色
        var roles = _db.UserRoles.Where(ur => ur.UserId == id);
        _db.UserRoles.RemoveRange(roles);

        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(long id, string newPassword)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) throw new BusinessException("用户不存在");

        entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateProfileAsync(long userId, ProfileUpdateDto dto)
    {
        var entity = await _db.Users.FindAsync(userId);
        if (entity == null) throw new BusinessException("用户不存在");

        if (!string.IsNullOrWhiteSpace(dto.RealName))
            entity.RealName = dto.RealName;
        if (dto.Phone != null)
            entity.Phone = dto.Phone;
        if (dto.Email != null)
            entity.Email = dto.Email;
        if (dto.AvatarUrl != null)
            entity.AvatarUrl = dto.AvatarUrl;

        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var entity = await _db.Users.FindAsync(userId);
        if (entity == null) throw new BusinessException("用户不存在");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, entity.Password))
            throw new BusinessException("当前密码错误");

        entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
