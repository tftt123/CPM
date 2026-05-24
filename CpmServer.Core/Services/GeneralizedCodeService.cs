using CpmServer.Common;
using CpmServer.Data;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public interface IGeneralizedCodeService
{
    Task<bool> IsValidAsync(string domain, string code, string? site = null, string? app = null);
    Task<List<string>> GetValidCodesAsync(string domain, string? site = null, string? app = null);
    Task ValidateAsync(string domain, string code, string? site = null, string? app = null);
}

public class GeneralizedCodeService : IGeneralizedCodeService
{
    private readonly CpmDbContext _db;

    public GeneralizedCodeService(CpmDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsValidAsync(string domain, string code, string? site = null, string? app = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        var query = _db.GeneralizedCodes
            .Where(g => g.Domain == domain && g.Code == code && g.IsActive);

        if (!string.IsNullOrWhiteSpace(app))
            query = query.Where(g => g.App == app || string.IsNullOrEmpty(g.App));
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));

        return await query.AnyAsync();
    }

    public async Task<List<string>> GetValidCodesAsync(string domain, string? site = null, string? app = null)
    {
        var query = _db.GeneralizedCodes
            .Where(g => g.Domain == domain && g.IsActive);

        if (!string.IsNullOrWhiteSpace(app))
            query = query.Where(g => g.App == app || string.IsNullOrEmpty(g.App));
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));

        return await query
            .OrderBy(g => g.SortOrder)
            .Select(g => g.Code)
            .ToListAsync();
    }

    public async Task ValidateAsync(string domain, string code, string? site = null, string? app = null)
    {
        if (!await IsValidAsync(domain, code, site, app))
        {
            var validCodes = await GetValidCodesAsync(domain, site, app);
            var validList = validCodes.Count > 0 ? string.Join(", ", validCodes) : "(无配置)";
            throw new BusinessException($"无效的 {domain} 值 '{code}'。有效值: {validList}");
        }
    }
}
