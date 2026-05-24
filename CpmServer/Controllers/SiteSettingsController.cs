using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SiteSettingsController : ControllerBase
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SiteSettingsController(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResult<SysSiteSettings?>> Get()
    {
        var currentSite = _currentUser.Site;
        var settings = await _db.SiteSettings
            .FirstOrDefaultAsync(s => s.Site == currentSite);
        return ApiResult<SysSiteSettings?>.Success(settings);
    }

    [HttpPost]
    public async Task<ApiResult> Save([FromBody] SiteSettingsRequest dto)
    {
        var currentSite = _currentUser.Site;
        var settings = await _db.SiteSettings
            .FirstOrDefaultAsync(s => s.Site == currentSite);
        if (settings == null)
        {
            settings = new SysSiteSettings
            {
                Site = dto.Site,
                SiteCode = dto.SiteCode,
                Currency = dto.Currency,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _db.SiteSettings.Add(settings);
        }
        else
        {
            settings.Site = dto.Site;
            settings.SiteCode = dto.SiteCode;
            settings.Currency = dto.Currency;
            settings.Description = dto.Description;
            settings.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}

public class SiteSettingsRequest
{
    public string Site { get; set; } = string.Empty;
    public string? SiteCode { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
}
