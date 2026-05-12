using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.MfgProcess;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class MfgProcessService : IMfgProcessService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MfgProcessService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private string? CurrentSite => _currentUser.Site;

    private IQueryable<MfgProcess> ProcessQuery =>
        _db.MfgProcesses.Where(p => string.IsNullOrEmpty(CurrentSite) || p.Site == CurrentSite);

    private IQueryable<MfgSubCategory> SubCategoryQuery =>
        _db.MfgSubCategories.Where(s => string.IsNullOrEmpty(CurrentSite) || s.Site == CurrentSite);

    private IQueryable<MfgEquipment> EquipmentQuery =>
        _db.MfgEquipments.Where(e => string.IsNullOrEmpty(CurrentSite) || e.Site == CurrentSite);

    #region Cascade Options

    public async Task<List<string>> GetCategoryListAsync()
    {
        return await ProcessQuery
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<MfgCascadeOption>> GetProcessOptionsAsync(string? category = null)
    {
        var query = ProcessQuery.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        return await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new MfgCascadeOption
            {
                Id = p.Id,
                Label = p.ProcessName,
                Owner = p.Owner
            })
            .ToListAsync();
    }

    public async Task<List<MfgCascadeOption>> GetSubCategoryOptionsAsync(long processId)
    {
        return await SubCategoryQuery
            .Where(s => s.ProcessId == processId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Select(s => new MfgCascadeOption
            {
                Id = s.Id,
                Label = s.SubCategoryName
            })
            .ToListAsync();
    }

    #endregion

    #region Process

    public async Task<List<MfgProcessDto>> GetProcessesAsync(string? category)
    {
        var query = ProcessQuery
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        return await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new MfgProcessDto
            {
                Id = p.Id,
                Category = p.Category,
                ProcessCode = p.ProcessCode,
                ProcessName = p.ProcessName,
                Description = p.Description,
                Owner = p.Owner,
                StdTimeMin = p.StdTimeMin,
                CostRate = p.CostRate,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<long> CreateProcessAsync(MfgProcessCreateDto dto)
    {
        var entity = new MfgProcess
        {
            Category = dto.Category,
            ProcessCode = dto.ProcessCode,
            ProcessName = dto.ProcessName,
            Description = dto.Description,
            Owner = dto.Owner,
            Site = CurrentSite,
            StdTimeMin = dto.StdTimeMin,
            CostRate = dto.CostRate,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.MfgProcesses.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateProcessAsync(long id, MfgProcessCreateDto dto)
    {
        var entity = await ProcessQuery.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) throw new BusinessException("工艺不存在");

        entity.Category = dto.Category;
        entity.ProcessCode = dto.ProcessCode;
        entity.ProcessName = dto.ProcessName;
        entity.Description = dto.Description;
        entity.Owner = dto.Owner;
        entity.StdTimeMin = dto.StdTimeMin;
        entity.CostRate = dto.CostRate;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteProcessAsync(long id)
    {
        var entity = await ProcessQuery.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return;

        var hasChildren = await SubCategoryQuery.AnyAsync(s => s.ProcessId == id);
        if (hasChildren) throw new BusinessException("该工艺下存在子工艺，无法删除");

        _db.MfgProcesses.Remove(entity);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region SubCategory

    public async Task<List<MfgSubCategoryDto>> GetSubCategoriesAsync(long? processId)
    {
        var query = SubCategoryQuery
            .Include(s => s.Process)
            .Where(s => s.IsActive)
            .AsQueryable();

        if (processId.HasValue)
            query = query.Where(s => s.ProcessId == processId.Value);

        return await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Select(s => new MfgSubCategoryDto
            {
                Id = s.Id,
                ProcessId = s.ProcessId,
                Category = s.Process != null ? s.Process.Category : null,
                ProcessName = s.Process != null ? s.Process.ProcessName : null,
                SubCategoryCode = s.SubCategoryCode,
                SubCategoryName = s.SubCategoryName,
                Description = s.Description,
                ToleranceGrade = s.ToleranceGrade,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                Equipments = s.Equipments.Select(e => new MfgEquipmentDto
                {
                    Id = e.Id,
                    SubCategoryId = e.SubCategoryId,
                    EquipmentCode = e.EquipmentCode,
                    EquipmentName = e.EquipmentName,
                    Description = e.Description,
                    Model = e.Model,
                    Spec = e.Spec,
                    Manufacturer = e.Manufacturer,
                    Owner = e.Owner,
                    CostRate = e.CostRate,
                    IsActive = e.IsActive
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<long> CreateSubCategoryAsync(MfgSubCategoryCreateDto dto)
    {
        var entity = new MfgSubCategory
        {
            ProcessId = dto.ProcessId,
            SubCategoryCode = dto.SubCategoryCode,
            SubCategoryName = dto.SubCategoryName,
            Description = dto.Description,
            Site = CurrentSite,
            ToleranceGrade = dto.ToleranceGrade,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.MfgSubCategories.Add(entity);
        await _db.SaveChangesAsync();

        foreach (var eqDto in dto.Equipments)
        {
            _db.MfgEquipments.Add(new MfgEquipment
            {
                SubCategoryId = entity.Id,
                EquipmentCode = eqDto.EquipmentCode,
                EquipmentName = eqDto.EquipmentName,
                Description = eqDto.Description,
                Site = CurrentSite,
                Model = eqDto.Model,
                Spec = eqDto.Spec,
                Manufacturer = eqDto.Manufacturer,
                Owner = eqDto.Owner,
                CostRate = eqDto.CostRate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }
        await _db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateSubCategoryAsync(long id, MfgSubCategoryCreateDto dto)
    {
        var entity = await SubCategoryQuery
            .Include(s => s.Equipments)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) throw new BusinessException("子工艺不存在");

        entity.ProcessId = dto.ProcessId;
        entity.SubCategoryCode = dto.SubCategoryCode;
        entity.SubCategoryName = dto.SubCategoryName;
        entity.Description = dto.Description;
        entity.ToleranceGrade = dto.ToleranceGrade;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.Now;

        _db.MfgEquipments.RemoveRange(entity.Equipments);
        foreach (var eqDto in dto.Equipments)
        {
            _db.MfgEquipments.Add(new MfgEquipment
            {
                SubCategoryId = entity.Id,
                EquipmentCode = eqDto.EquipmentCode,
                EquipmentName = eqDto.EquipmentName,
                Description = eqDto.Description,
                Site = CurrentSite,
                Model = eqDto.Model,
                Spec = eqDto.Spec,
                Manufacturer = eqDto.Manufacturer,
                Owner = eqDto.Owner,
                CostRate = eqDto.CostRate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteSubCategoryAsync(long id)
    {
        var entity = await SubCategoryQuery.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return;

        _db.MfgSubCategories.Remove(entity);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Equipment

    public async Task<List<MfgEquipmentDto>> GetEquipmentsAsync(long? subCategoryId)
    {
        var query = EquipmentQuery
            .Include(e => e.SubCategory)
            .ThenInclude(s => s!.Process)
            .Where(e => e.IsActive)
            .AsQueryable();

        if (subCategoryId.HasValue)
            query = query.Where(e => e.SubCategoryId == subCategoryId.Value);

        return await query
            .OrderBy(e => e.Id)
            .Select(e => new MfgEquipmentDto
            {
                Id = e.Id,
                SubCategoryId = e.SubCategoryId,
                Category = e.SubCategory != null && e.SubCategory.Process != null ? e.SubCategory.Process.Category : null,
                ProcessName = e.SubCategory != null && e.SubCategory.Process != null ? e.SubCategory.Process.ProcessName : null,
                SubCategoryName = e.SubCategory != null ? e.SubCategory.SubCategoryName : null,
                EquipmentCode = e.EquipmentCode,
                EquipmentName = e.EquipmentName,
                Description = e.Description,
                Model = e.Model,
                Spec = e.Spec,
                Manufacturer = e.Manufacturer,
                Owner = e.Owner,
                CostRate = e.CostRate,
                IsActive = e.IsActive
            })
            .ToListAsync();
    }

    public async Task<long> CreateEquipmentAsync(MfgEquipmentCreateDto dto)
    {
        var entity = new MfgEquipment
        {
            SubCategoryId = dto.SubCategoryId,
            EquipmentCode = dto.EquipmentCode,
            EquipmentName = dto.EquipmentName,
            Description = dto.Description,
            Site = CurrentSite,
            Model = dto.Model,
            Spec = dto.Spec,
            Manufacturer = dto.Manufacturer,
            Owner = dto.Owner,
            CostRate = dto.CostRate,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.MfgEquipments.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateEquipmentAsync(long id, MfgEquipmentCreateDto dto)
    {
        var entity = await EquipmentQuery.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) throw new BusinessException("设备不存在");

        entity.SubCategoryId = dto.SubCategoryId;
        entity.EquipmentCode = dto.EquipmentCode;
        entity.EquipmentName = dto.EquipmentName;
        entity.Description = dto.Description;
        entity.Model = dto.Model;
        entity.Spec = dto.Spec;
        entity.Manufacturer = dto.Manufacturer;
        entity.Owner = dto.Owner;
        entity.CostRate = dto.CostRate;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteEquipmentAsync(long id)
    {
        var entity = await EquipmentQuery.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return;

        _db.MfgEquipments.Remove(entity);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Flat Records

    public async Task<List<MfgProcessRecordDto>> GetFlatRecordsAsync(string? category, string? keyword)
    {
        var query = EquipmentQuery
            .Include(e => e.SubCategory)
            .ThenInclude(s => s!.Process)
            .Where(e => e.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.SubCategory != null && e.SubCategory.Process != null && e.SubCategory.Process.Category == category);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(e =>
                (e.EquipmentName != null && e.EquipmentName.Contains(keyword)) ||
                (e.EquipmentCode != null && e.EquipmentCode.Contains(keyword)) ||
                (e.SubCategory != null && e.SubCategory.SubCategoryName != null && e.SubCategory.SubCategoryName.Contains(keyword)) ||
                (e.SubCategory != null && e.SubCategory.Process != null && e.SubCategory.Process.ProcessName != null && e.SubCategory.Process.ProcessName.Contains(keyword)));

        return await query
            .OrderBy(e => e.SubCategory!.Process!.Category)
            .ThenBy(e => e.SubCategory!.Process!.SortOrder)
            .ThenBy(e => e.SubCategory!.SortOrder)
            .ThenBy(e => e.Id)
            .Select(e => new MfgProcessRecordDto
            {
                EquipmentId = e.Id,
                Category = e.SubCategory != null && e.SubCategory.Process != null ? e.SubCategory.Process.Category : string.Empty,
                ProcessName = e.SubCategory != null && e.SubCategory.Process != null ? e.SubCategory.Process.ProcessName : string.Empty,
                SubCategoryName = e.SubCategory != null ? e.SubCategory.SubCategoryName : string.Empty,
                EquipmentCode = e.EquipmentCode,
                EquipmentName = e.EquipmentName,
                Description = e.Description,
                Model = e.Model,
                Spec = e.Spec,
                Manufacturer = e.Manufacturer,
                Owner = e.Owner,
                CostRate = e.CostRate,
                IsActive = e.IsActive,
                ProcessId = e.SubCategory != null && e.SubCategory.Process != null ? e.SubCategory.Process.Id : 0,
                SubCategoryId = e.SubCategoryId
            })
            .ToListAsync();
    }

    #endregion
}
