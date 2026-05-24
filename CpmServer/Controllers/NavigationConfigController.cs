using CpmServer.Authorization;
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
[Authorize(Policy = Policies.CanManageSystem)]
public class NavigationConfigController : ControllerBase
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NavigationConfigController(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResult<List<NavigationConfigDto>>> GetList()
    {
        var query = _db.NavigationConfigs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(n => n.Site == _currentUser.Site);
        }
        else
        {
            query = query.Where(n => string.IsNullOrEmpty(n.Site));
        }

        var list = await query
            .OrderBy(n => n.ModuleCode)
            .ThenBy(n => n.SortOrder)
            .ThenBy(n => n.Id)
            .Select(n => new NavigationConfigDto
            {
                Id = n.Id,
                NavCode = n.NavCode,
                ModuleCode = n.ModuleCode,
                ModuleLabel = n.ModuleLabel,
                ModuleLabelEn = n.ModuleLabelEn,
                NavLabel = n.NavLabel,
                NavLabelEn = n.NavLabelEn,
                RoutePath = n.RoutePath,
                IconName = n.IconName,
                SortOrder = n.SortOrder,
                IsVisible = n.IsVisible,
                IsActive = n.IsActive,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync();

        return ApiResult<List<NavigationConfigDto>>.Success(list);
    }

    [HttpPost("batch")]
    public async Task<ApiResult> BatchUpdate([FromBody] List<NavigationConfigBatchItemDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return ApiResult.Error("No data to update");

        var site = _currentUser.Site;

        // Get existing records for this site
        var query = _db.NavigationConfigs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(n => n.Site == site);
        else
            query = query.Where(n => string.IsNullOrEmpty(n.Site));

        var existing = await query.ToDictionaryAsync(n => n.NavCode);

        var incomingNavCodes = dtos.Select(d => d.NavCode).ToHashSet();

        foreach (var dto in dtos)
        {
            if (existing.TryGetValue(dto.NavCode, out var entity))
            {
                // Update existing
                entity.ModuleCode = dto.ModuleCode;
                entity.ModuleLabel = dto.ModuleLabel;
                entity.ModuleLabelEn = dto.ModuleLabelEn;
                entity.NavLabel = dto.NavLabel;
                entity.NavLabelEn = dto.NavLabelEn;
                entity.RoutePath = dto.RoutePath;
                entity.IconName = dto.IconName;
                entity.SortOrder = dto.SortOrder;
                entity.IsVisible = dto.IsVisible;
                entity.IsActive = dto.IsActive;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                _db.NavigationConfigs.Add(new SysNavigationConfig
                {
                    NavCode = dto.NavCode,
                    ModuleCode = dto.ModuleCode,
                    ModuleLabel = dto.ModuleLabel,
                    ModuleLabelEn = dto.ModuleLabelEn,
                    NavLabel = dto.NavLabel,
                    NavLabelEn = dto.NavLabelEn,
                    RoutePath = dto.RoutePath,
                    IconName = dto.IconName,
                    SortOrder = dto.SortOrder,
                    IsVisible = dto.IsVisible,
                    IsActive = dto.IsActive,
                    Site = site,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Soft-delete items not in the incoming list
        foreach (var kv in existing)
        {
            if (!incomingNavCodes.Contains(kv.Key))
            {
                kv.Value.IsActive = false;
                kv.Value.IsVisible = false;
                kv.Value.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPost("init")]
    public async Task<ApiResult<List<NavigationConfigDto>>> InitDefaults()
    {
        var site = _currentUser.Site;

        // Check if already initialized
        var query = _db.NavigationConfigs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(n => n.Site == site);
        else
            query = query.Where(n => string.IsNullOrEmpty(n.Site));

        var existingCount = await query.CountAsync();
        if (existingCount > 0)
        {
            // Return existing without modifying
            var existing = await query
                .OrderBy(n => n.ModuleCode)
                .ThenBy(n => n.SortOrder)
                .Select(n => new NavigationConfigDto
                {
                    Id = n.Id,
                    NavCode = n.NavCode,
                    ModuleCode = n.ModuleCode,
                    ModuleLabel = n.ModuleLabel,
                    ModuleLabelEn = n.ModuleLabelEn,
                    NavLabel = n.NavLabel,
                    NavLabelEn = n.NavLabelEn,
                    RoutePath = n.RoutePath,
                    IconName = n.IconName,
                    SortOrder = n.SortOrder,
                    IsVisible = n.IsVisible,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();
            return ApiResult<List<NavigationConfigDto>>.Success(existing);
        }

        // Default navigation structure
        var defaults = new List<SysNavigationConfig>
        {
            new() { NavCode = "home", ModuleCode = "home", RoutePath = "/home", IconName = "HomeFilled", SortOrder = 0, Site = site },
            new() { NavCode = "customer", ModuleCode = "rfq", RoutePath = "/customer", IconName = "UserFilled", SortOrder = 0, Site = site },
            new() { NavCode = "product", ModuleCode = "rfq", RoutePath = "/product", IconName = "Box", SortOrder = 1, Site = site },
            new() { NavCode = "opportunity", ModuleCode = "rfq", RoutePath = "/opportunity", IconName = "FolderOpened", SortOrder = 2, Site = site },
            new() { NavCode = "quotation", ModuleCode = "rfq", RoutePath = "/quotation/list", IconName = "Document", SortOrder = 3, Site = site },
            new() { NavCode = "mfgProcess", ModuleCode = "rfq", RoutePath = "/mfg/process", IconName = "Setting", SortOrder = 4, Site = site },
            new() { NavCode = "actualCycleTime", ModuleCode = "pm", RoutePath = "/pm/actual-cycle-time", IconName = "Timer", SortOrder = 0, Site = site },
            new() { NavCode = "productTrace", ModuleCode = "pm", RoutePath = "/pm/trace", IconName = "TrendCharts", SortOrder = 1, Site = site },
            new() { NavCode = "approvalCenter", ModuleCode = "approval", RoutePath = "/approval/center", IconName = "CircleCheck", SortOrder = 0, Site = site },
            new() { NavCode = "dataAnalysis", ModuleCode = "salesReport", RoutePath = "/report/analysis", IconName = "DataAnalysis", SortOrder = 0, Site = site },
        };

        _db.NavigationConfigs.AddRange(defaults);
        await _db.SaveChangesAsync();

        var result = defaults.Select(n => new NavigationConfigDto
        {
            Id = n.Id,
            NavCode = n.NavCode,
            ModuleCode = n.ModuleCode,
            ModuleLabel = n.ModuleLabel,
            ModuleLabelEn = n.ModuleLabelEn,
            NavLabel = n.NavLabel,
            NavLabelEn = n.NavLabelEn,
            RoutePath = n.RoutePath,
            IconName = n.IconName,
            SortOrder = n.SortOrder,
            IsVisible = n.IsVisible,
            IsActive = n.IsActive,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt
        }).ToList();

        return ApiResult<List<NavigationConfigDto>>.Success(result);
    }
}

public class NavigationConfigDto
{
    public long Id { get; set; }
    public string NavCode { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string? ModuleLabel { get; set; }
    public string? ModuleLabelEn { get; set; }
    public string? NavLabel { get; set; }
    public string? NavLabelEn { get; set; }
    public string RoutePath { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NavigationConfigBatchItemDto
{
    public string NavCode { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string? ModuleLabel { get; set; }
    public string? ModuleLabelEn { get; set; }
    public string? NavLabel { get; set; }
    public string? NavLabelEn { get; set; }
    public string RoutePath { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public bool IsActive { get; set; }
}
