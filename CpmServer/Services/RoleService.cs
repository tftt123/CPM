using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.Role;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class RoleService : IRoleService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RoleService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<RoleDto>> GetListAsync(string? site)
    {
        var query = _db.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(r => r.Site == _currentUser.Site);
        }

        var list = await query.OrderBy(r => r.RoleCode).ToListAsync();

        return list.Select(r => new RoleDto
        {
            Id = r.Id,
            RoleCode = r.RoleCode,
            RoleName = r.RoleName,
            Site = r.Site
        }).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(long id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return null;

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == id && rp.IsActive)
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync();

        return new RoleDto
        {
            Id = role.Id,
            RoleCode = role.RoleCode,
            RoleName = role.RoleName,
            Site = role.Site,
            Permissions = permissions
        };
    }

    public async Task<long> CreateAsync(RoleCreateDto dto)
    {
        if (await _db.Roles.AnyAsync(r => r.RoleCode == dto.RoleCode))
            throw new BusinessException("角色编码已存在");

        var entity = new SysRole
        {
            RoleCode = dto.RoleCode,
            RoleName = dto.RoleName,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site
        };

        _db.Roles.Add(entity);
        await _db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, RoleUpdateDto dto)
    {
        var entity = await _db.Roles.FindAsync(id);
        if (entity == null) throw new BusinessException("角色不存在");

        entity.RoleName = dto.RoleName;
        entity.Site = dto.Site;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Roles.FindAsync(id);
        if (entity == null) return;

        // 检查是否有关联用户
        var hasUsers = await _db.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (hasUsers)
            throw new BusinessException("该角色已分配给用户，无法删除");

        _db.Roles.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRolePermissionsAsync(long id, List<string> permissionCodes)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) throw new BusinessException("角色不存在");

        var site = _currentUser.Site;
        var app = _currentUser.App;

        // 删除该角色在当前 Site/App 下的旧权限
        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == id && rp.App == app && rp.Site == site)
            .ToListAsync();
        _db.RolePermissions.RemoveRange(existing);

        // 插入新权限
        foreach (var code in permissionCodes.Distinct())
        {
            _db.RolePermissions.Add(new SysRolePermission
            {
                RoleId = id,
                PermissionCode = code,
                Site = site,
                App = app,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }
}
