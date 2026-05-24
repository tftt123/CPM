using CpmServer.Constants;
using CpmServer.Data;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Approval.Services;

public interface IModuleTypeConfigService
{
    Task<List<SysModuleTypeConfig>> GetListAsync(bool onlyActive = true);
    Task<SysModuleTypeConfig?> GetByIdAsync(long id);
    Task<SysModuleTypeConfig?> GetByModuleTypeAsync(string moduleType);
    Task<long> CreateAsync(SysModuleTypeConfig entity);
    Task UpdateAsync(long id, SysModuleTypeConfig entity);
    Task DeleteAsync(long id);
}

public class ModuleTypeConfigService : IModuleTypeConfigService
{
    private readonly CpmDbContext _db;

    public ModuleTypeConfigService(CpmDbContext db)
    {
        _db = db;
    }

    public async Task<List<SysModuleTypeConfig>> GetListAsync(bool onlyActive = true)
    {
        var query = _db.ModuleTypeConfigs.AsQueryable();
        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }
        return await query.OrderBy(x => x.ModuleType).ToListAsync();
    }

    public async Task<SysModuleTypeConfig?> GetByIdAsync(long id)
    {
        return await _db.ModuleTypeConfigs.FindAsync(id);
    }

    public async Task<SysModuleTypeConfig?> GetByModuleTypeAsync(string moduleType)
    {
        return await _db.ModuleTypeConfigs
            .FirstOrDefaultAsync(x => x.ModuleType == moduleType);
    }

    public async Task<long> CreateAsync(SysModuleTypeConfig entity)
    {
        if (!SystemModuleTypes.All.Contains(entity.ModuleType))
        {
            throw new InvalidOperationException($"模块类型 '{entity.ModuleType}' 不是系统预定义类型");
        }

        var exists = await _db.ModuleTypeConfigs
            .AnyAsync(x => x.ModuleType == entity.ModuleType);
        if (exists)
        {
            throw new InvalidOperationException($"ModuleType '{entity.ModuleType}' already exists");
        }

        _db.ModuleTypeConfigs.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(long id, SysModuleTypeConfig entity)
    {
        var existing = await _db.ModuleTypeConfigs.FindAsync(id);
        if (existing == null) return;

        var duplicate = await _db.ModuleTypeConfigs
            .AnyAsync(x => x.Id != id && x.ModuleType == entity.ModuleType);
        if (duplicate)
        {
            throw new InvalidOperationException($"ModuleType '{entity.ModuleType}' already exists");
        }

        existing.ModuleType = entity.ModuleType;
        existing.ModuleName = entity.ModuleName;
        existing.Description = entity.Description;
        existing.IsActive = entity.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.ModuleTypeConfigs.FindAsync(id);
        if (entity != null)
        {
            _db.ModuleTypeConfigs.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
