using CpmServer.Data;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class FieldControlService : IFieldControlService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public FieldControlService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private string? CurrentSite => _currentUser.Site;
    private string? CurrentApp => _currentUser.App ?? "cpm";

    // 后端硬编码的模块列表
    private static readonly List<string> Modules = new()
    {
        "Quotation", "Opportunity", "Customer", "Product", "Mfg", "PM", "Approval", "System"
    };

    // 后端硬编码的页面列表
    private static readonly Dictionary<string, List<string>> Pages = new()
    {
        ["Quotation"] = new() { "QuotationForm", "QuotationList", "QuotationDetail" },
        ["Opportunity"] = new() { "OpportunityForm", "OpportunityList" },
        ["Customer"] = new() { "CustomerForm", "CustomerList" },
        ["Product"] = new() { "ProductForm", "ProductList" },
        ["Mfg"] = new() { "MfgProcessManage" },
        ["PM"] = new() { "ProductTraceList", "ProductTraceDetail", "ActualCycleTimeManage" },
        ["Approval"] = new() { "ApprovalConfig" },
        ["System"] = new() { "UserForm", "UserList", "RoleList" }
    };

    public List<string> GetModules() => Modules;

    public List<string> GetPages(string moduleCode)
    {
        return Pages.TryGetValue(moduleCode, out var pages) ? pages : new List<string>();
    }

    public async Task<List<SysFieldControl>> GetFieldsAsync(string moduleCode, string pageCode, string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        var effectiveApp = CurrentApp;
        var query = _db.FieldControls
            .Where(f => f.ModuleCode == moduleCode && f.PageCode == pageCode &&
                (f.App == effectiveApp || string.IsNullOrEmpty(f.App)));

        if (!string.IsNullOrEmpty(effectiveSite))
            query = query.Where(f => f.Site == effectiveSite);
        else
            query = query.Where(f => string.IsNullOrEmpty(f.Site));

        return await query
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<List<SysFieldControl>> InitFieldsAsync(
        string moduleCode, string pageCode, List<FieldInitDto> fields, string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        var effectiveApp = CurrentApp;

        // Check if already initialized
        var existingCount = await _db.FieldControls
            .Where(f => f.ModuleCode == moduleCode && f.PageCode == pageCode)
            .Where(f => f.App == effectiveApp || string.IsNullOrEmpty(f.App))
            .Where(f => string.IsNullOrEmpty(effectiveSite) ? string.IsNullOrEmpty(f.Site) : f.Site == effectiveSite)
            .CountAsync();

        if (existingCount > 0)
        {
            // Already initialized, just return existing
            return await GetFieldsAsync(moduleCode, pageCode, effectiveSite);
        }

        // Initialize from template
        var entities = new List<SysFieldControl>();
        int sort = 0;
        foreach (var field in fields)
        {
            entities.Add(new SysFieldControl
            {
                ModuleCode = moduleCode,
                PageCode = pageCode,
                FieldCode = field.FieldCode,
                IsVisible = true,
                IsRequired = field.DefaultRequired,
                SortOrder = sort++,
                Site = effectiveSite,
                App = effectiveApp,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        _db.FieldControls.AddRange(entities);
        await _db.SaveChangesAsync();
        return entities;
    }

    public async Task UpdateBatchAsync(List<SysFieldControl> fields, string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        var effectiveApp = CurrentApp;

        foreach (var field in fields)
        {
            var existing = await _db.FieldControls
                .FirstOrDefaultAsync(f =>
                    f.ModuleCode == field.ModuleCode &&
                    f.PageCode == field.PageCode &&
                    f.FieldCode == field.FieldCode &&
                    (f.App == effectiveApp || string.IsNullOrEmpty(f.App)) &&
                    (f.Site == effectiveSite || (string.IsNullOrEmpty(f.Site) && string.IsNullOrEmpty(effectiveSite))));

            if (existing != null)
            {
                existing.IsVisible = field.IsVisible;
                existing.IsRequired = field.IsRequired;
                existing.SortOrder = field.SortOrder;
                existing.UpdatedAt = DateTime.Now;
            }
        }

        await _db.SaveChangesAsync();
    }
}
