using CpmServer.Data;
using CpmServer.Modules.SequenceRule.Contracts;
using CpmServer.Modules.SequenceRule.DTOs;
using CpmServer.Models;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.SequenceRule.Services;

public class SequenceRuleService : ISequenceRuleService
{
    private readonly CpmDbContext _db;
    private readonly IGeneralizedCodeService _gc;

    public SequenceRuleService(CpmDbContext db, IGeneralizedCodeService gc)
    {
        _db = db;
        _gc = gc;
    }

    public async Task<List<SequenceRuleDto>> GetRulesAsync(string? site)
    {
        var query = _db.SequenceRules.AsQueryable();
        if (!string.IsNullOrWhiteSpace(site))
        {
            query = query.Where(r => r.Site == site);
        }

        var list = await query.OrderBy(r => r.ModuleType).ToListAsync();
        return list.Select(r => ToDto(r)).ToList();
    }

    public async Task<SequenceRuleDto?> GetRuleByIdAsync(long id)
    {
        var rule = await _db.SequenceRules.FindAsync(id);
        return rule == null ? null : ToDto(rule);
    }

    public async Task<SequenceRuleDto?> GetRuleByModuleTypeAsync(string moduleType, string? site)
    {
        var query = _db.SequenceRules.Where(r => r.ModuleType == moduleType);
        if (!string.IsNullOrWhiteSpace(site))
        {
            query = query.Where(r => r.Site == site);
        }
        var rule = await query.FirstOrDefaultAsync();
        return rule == null ? null : ToDto(rule);
    }

    public async Task<long> CreateRuleAsync(SequenceRuleCreateDto dto, string? site)
    {
        await _gc.ValidateAsync("SEQUENCE_RESET_RULE", dto.ResetRule.ToString(), site, "cpm");

        var existing = await _db.SequenceRules
            .FirstOrDefaultAsync(r => r.ModuleType == dto.ModuleType && r.Site == site);
        if (existing != null)
        {
            throw new InvalidOperationException($"模块 '{dto.ModuleType}' 在该站点已存在流水号规则");
        }

        var entity = new SysSequenceRule
        {
            ModuleType = dto.ModuleType,
            ModuleName = dto.ModuleName,
            Prefix = dto.Prefix,
            CurrentSequence = 0,
            SequenceLength = dto.SequenceLength,
            ResetRule = dto.ResetRule,
            Site = site,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.SequenceRules.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateRuleAsync(long id, SequenceRuleUpdateDto dto)
    {
        var entity = await _db.SequenceRules.FindAsync(id);
        if (entity == null) throw new InvalidOperationException("规则不存在");

        await _gc.ValidateAsync("SEQUENCE_RESET_RULE", dto.ResetRule.ToString(), entity.Site, "cpm");

        entity.ModuleType = dto.ModuleType;
        entity.ModuleName = dto.ModuleName;
        entity.Prefix = dto.Prefix;
        entity.SequenceLength = dto.SequenceLength;
        entity.ResetRule = dto.ResetRule;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRuleAsync(long id)
    {
        var entity = await _db.SequenceRules.FindAsync(id);
        if (entity == null) return;

        _db.SequenceRules.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateSequenceAsync(string moduleType, string? site)
    {
        var rule = await _db.SequenceRules
            .FirstOrDefaultAsync(r => r.ModuleType == moduleType && r.Site == site);

        if (rule == null)
        {
            throw new InvalidOperationException($"模块 '{moduleType}' 未配置流水号规则");
        }

        var now = DateTime.Now;
        var year = (now.Year % 100).ToString("D2");

        bool needReset = false;
        if (rule.LastResetDate.HasValue)
        {
            var last = rule.LastResetDate.Value;
            needReset = rule.ResetRule switch
            {
                1 => last.Date != now.Date,
                2 => last.Month != now.Month,
                3 => last.Year != now.Year,
                _ => false
            };
        }

        if (needReset)
        {
            rule.CurrentSequence = 0;
            rule.LastResetDate = now;
        }

        rule.CurrentSequence++;
        var seqStr = rule.CurrentSequence.ToString().PadLeft(rule.SequenceLength, '0');

        var siteSettings = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Site == site);
        var siteCode = siteSettings?.SiteCode ?? site ?? "";

        var generatedNo = $"{siteCode}{rule.Prefix}{year}{seqStr}";

        rule.LastGeneratedNo = generatedNo;
        rule.LastResetDate ??= now;
        rule.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return generatedNo;
    }

    private static SequenceRuleDto ToDto(SysSequenceRule r) => new()
    {
        Id = r.Id,
        ModuleType = r.ModuleType,
        ModuleName = r.ModuleName,
        Prefix = r.Prefix,
        CurrentSequence = r.CurrentSequence,
        SequenceLength = r.SequenceLength,
        ResetRule = r.ResetRule,
        LastResetDate = r.LastResetDate,
        LastGeneratedNo = r.LastGeneratedNo,
        Site = r.Site,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
