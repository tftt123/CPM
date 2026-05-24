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
public class AlertRecipientController : ControllerBase
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AlertRecipientController(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResult<List<AlertRecipientDto>>> GetList()
    {
        var query = _db.AlertRecipients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(r => r.Site == _currentUser.Site);
        }

        var list = await query
            .OrderBy(r => r.AlertType)
            .ThenBy(r => r.RecipientType)
            .Select(r => new AlertRecipientDto
            {
                Id = r.Id,
                AlertType = r.AlertType,
                RecipientType = r.RecipientType,
                RecipientValue = r.RecipientValue,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return ApiResult<List<AlertRecipientDto>>.Success(list);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] AlertRecipientCreateRequest dto)
    {
        var entity = new SysAlertRecipient
        {
            AlertType = dto.AlertType,
            RecipientType = dto.RecipientType,
            RecipientValue = dto.RecipientValue,
            IsActive = dto.IsActive,
            Site = _currentUser.Site,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.AlertRecipients.Add(entity);
        await _db.SaveChangesAsync();

        return ApiResult<long>.Success(entity.Id);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] AlertRecipientUpdateRequest dto)
    {
        var entity = await _db.AlertRecipients.FindAsync(id);
        if (entity == null) return ApiResult.Error("配置不存在");

        entity.AlertType = dto.AlertType;
        entity.RecipientType = dto.RecipientType;
        entity.RecipientValue = dto.RecipientValue;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.AlertRecipients.FindAsync(id);
        if (entity == null) return ApiResult.Error("配置不存在");

        _db.AlertRecipients.Remove(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}

public class AlertRecipientDto
{
    public long Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientValue { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AlertRecipientCreateRequest
{
    public string AlertType { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientValue { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AlertRecipientUpdateRequest
{
    public string AlertType { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientValue { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
