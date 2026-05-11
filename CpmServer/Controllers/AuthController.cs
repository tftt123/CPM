using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.Auth;
using CpmServer.Models;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IQadAuthService _qadAuth;
    private readonly JwtHelper _jwt;
    private readonly CpmDbContext _db;

    public AuthController(IAuthService authService, IQadAuthService qadAuth, JwtHelper jwt, CpmDbContext db)
    {
        _authService = authService;
        _qadAuth = qadAuth;
        _jwt = jwt;
        _db = db;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return ApiResult<LoginResponse>.Success(result);
    }

    [HttpPost("qad-login")]
    [AllowAnonymous]
    public async Task<ApiResult<LoginResponse>> QadLogin([FromBody] QadLoginRequest request)
    {
        // 1. QAD 验证
        var (msg, success) = await _qadAuth.ValidateLoginAsync(request.Username, request.Password, request.Domain);
        if (!success)
            throw new BusinessException(msg);

        // 2. 查本地用户，不存在则自动创建
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            user = new SysUser
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                RealName = request.Username,
                Site = request.Domain,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 分配默认 USER 角色
            var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "USER");
            if (userRole == null)
            {
                userRole = new SysRole { RoleCode = "USER", RoleName = "普通用户", Site = user.Site };
                _db.Roles.Add(userRole);
                await _db.SaveChangesAsync();
            }
            _db.UserRoles.Add(new SysUserRole { UserId = user.Id, RoleId = userRole.Id });
            await _db.SaveChangesAsync();

            _db.UserSites.Add(new SysUserSite { UserId = user.Id, Site = user.Site ?? "NT01" });
            await _db.SaveChangesAsync();
        }

        if (!user.IsActive)
            throw new BusinessException("账号已被禁用");

        // 3. 获取用户角色
        var roleCodes = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.RoleCode)
            .ToListAsync();

        if (!roleCodes.Any())
            roleCodes.Add("USER");

        // 4. 生成 Token
        var token = _jwt.GenerateToken(user.Id, user.Username, roleCodes, user.Site ?? "NT01");

        return ApiResult<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            RealName = user.RealName,
            Site = user.Site,
            Roles = roleCodes
        });
    }

    [HttpGet("my-sites")]
    [Authorize]
    public async Task<ApiResult<List<string>>> GetMySites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return ApiResult<List<string>>.Success(new List<string>());

        var sites = await _authService.GetUserSitesAsync(userId);
        return ApiResult<List<string>>.Success(sites);
    }

    [HttpPost("switch-site")]
    [Authorize]
    public async Task<ApiResult<LoginResponse>> SwitchSite([FromBody] SwitchSiteRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("用户未登录");

        var result = await _authService.SwitchSiteAsync(userId, request.Site);
        return ApiResult<LoginResponse>.Success(result);
    }

    [HttpGet("sites")]
    [AllowAnonymous]
    public async Task<ApiResult<List<string>>> GetSites()
    {
        var sites = await _db.Users
            .Where(u => u.Site != null)
            .Select(u => u.Site!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
        return ApiResult<List<string>>.Success(sites);
    }
}