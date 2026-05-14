using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
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

    public SiteSettingsController(CpmDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResult<SysSiteSettings?>> Get()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        return ApiResult<SysSiteSettings?>.Success(settings);
    }

    [HttpPost]
    public async Task<ApiResult> Save([FromBody] SiteSettingsRequest dto)
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SysSiteSettings
            {
                Site = dto.Site,
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
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
}