using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.Auth;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CpmServer.Services;

public class AuthService : IAuthService
{
    private readonly CpmDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthService(CpmDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<string> StoreRefreshTokenAsync(long userId)
    {
        var refreshToken = GenerateRefreshToken();
        var entity = new SysRefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.Now.AddDays(7),
            CreatedAt = DateTime.Now
        };
        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new BusinessException("用户名或密码错误");

        if (!user.IsActive)
            throw new BusinessException("账号已被禁用");

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.RoleCode)
            .ToListAsync();

        // 获取用户有权限的所有 Site
        var allowedSites = await GetUserSitesAsync(user.Id);

        // 校验 Site
        string loginSite;
        if (!string.IsNullOrWhiteSpace(request.Site))
        {
            if (!allowedSites.Any(s => string.Equals(s, request.Site, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessException("您没有该工厂站点的访问权限");
            loginSite = request.Site;
        }
        else
        {
            loginSite = user.Site ?? allowedSites.FirstOrDefault() ?? string.Empty;
        }

        var token = _jwt.GenerateToken(user.Id, user.Username, roles, loginSite, "cpm");
        var refreshToken = await StoreRefreshTokenAsync(user.Id);

        return new LoginResponse
        {
            Token = token,
            AccessToken = token,
            RefreshToken = refreshToken,
            Username = user.Username,
            RealName = user.RealName,
            Site = loginSite,
            Roles = roles
        };
    }

    public async Task<List<string>> GetUserSitesAsync(long userId)
    {
        var sites = await _db.UserSites
            .Where(us => us.UserId == userId)
            .Select(us => us.Site)
            .Distinct()
            .ToListAsync();

        // 如果没有关联表数据，回退到用户默认 Site
        if (!sites.Any())
        {
            var user = await _db.Users.FindAsync(userId);
            if (!string.IsNullOrWhiteSpace(user?.Site))
                sites.Add(user.Site);
        }

        return sites;
    }

    public async Task<LoginResponse> SwitchSiteAsync(long userId, string site)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) throw new BusinessException("用户不存在");

        var allowedSites = await GetUserSitesAsync(userId);
        if (!allowedSites.Any(s => string.Equals(s, site, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessException("您没有该工厂站点的访问权限");

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.RoleCode)
            .ToListAsync();

        var token = _jwt.GenerateToken(user.Id, user.Username, roles, site);
        var refreshToken = await StoreRefreshTokenAsync(user.Id);

        return new LoginResponse
        {
            Token = token,
            AccessToken = token,
            RefreshToken = refreshToken,
            Username = user.Username,
            RealName = user.RealName,
            Site = site,
            Roles = roles
        };
    }

    public Task<string> GenerateRefreshTokenAsync(long userId)
    {
        return StoreRefreshTokenAsync(userId);
    }

    public async Task<RefreshTokenDto> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);

        if (tokenEntity == null)
            throw new UnauthorizedAccessException("无效的刷新令牌");

        if (tokenEntity.ExpiresAt < DateTime.Now)
            throw new UnauthorizedAccessException("刷新令牌已过期");

        var user = await _db.Users.FindAsync(tokenEntity.UserId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("用户不存在或已被禁用");

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.RoleCode)
            .ToListAsync();

        var loginSite = user.Site ?? await _db.UserSites
            .Where(us => us.UserId == user.Id)
            .Select(us => us.Site)
            .FirstOrDefaultAsync() ?? string.Empty;

        var newAccessToken = _jwt.GenerateToken(user.Id, user.Username, roles, loginSite);
        var newRefreshToken = await StoreRefreshTokenAsync(user.Id);

        tokenEntity.IsRevoked = true;
        tokenEntity.ReplacedByToken = newRefreshToken;
        await _db.SaveChangesAsync();

        return new RefreshTokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}