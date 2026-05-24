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
public class GeneralizedCodeController : ControllerBase
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GeneralizedCodeController(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private IQueryable<SysGeneralizedCode> ApplyAppSiteFilter(IQueryable<SysGeneralizedCode> query)
    {
        var app = _currentUser.App;
        var site = _currentUser.Site;

        if (!string.IsNullOrWhiteSpace(app))
            query = query.Where(g => g.App == app || string.IsNullOrEmpty(g.App));
        else
            query = query.Where(g => string.IsNullOrEmpty(g.App));

        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));
        else
            query = query.Where(g => string.IsNullOrEmpty(g.Site));

        return query;
    }

    [HttpGet]
    public async Task<ApiResult<List<GeneralizedCodeDto>>> GetByDomain([FromQuery] string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return ApiResult<List<GeneralizedCodeDto>>.Error("Domain is required");

        var query = ApplyAppSiteFilter(_db.GeneralizedCodes.AsQueryable());

        var list = await query
            .Where(g => g.Domain == domain)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Code)
            .Select(g => new GeneralizedCodeDto
            {
                Id = g.Id,
                Domain = g.Domain,
                Code = g.Code,
                Label = g.Label,
                LabelEn = g.LabelEn,
                SortOrder = g.SortOrder,
                IsActive = g.IsActive,
                TagType = g.TagType,
                Attributes = g.Attributes,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            })
            .ToListAsync();

        return ApiResult<List<GeneralizedCodeDto>>.Success(list);
    }

    [HttpGet("domains")]
    public async Task<ApiResult<List<DomainSummaryDto>>> GetDomains()
    {
        var query = ApplyAppSiteFilter(_db.GeneralizedCodes.AsQueryable());

        var list = await query
            .Where(g => g.IsActive)
            .GroupBy(g => g.Domain)
            .Select(g => new DomainSummaryDto
            {
                Domain = g.Key,
                Count = g.Count()
            })
            .OrderBy(g => g.Domain)
            .ToListAsync();

        return ApiResult<List<DomainSummaryDto>>.Success(list);
    }

    [HttpGet("validate")]
    public async Task<ApiResult<bool>> Validate([FromQuery] string domain, [FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(code))
            return ApiResult<bool>.Success(false);

        var query = ApplyAppSiteFilter(_db.GeneralizedCodes.AsQueryable());

        var exists = await query.AnyAsync(g =>
            g.Domain == domain &&
            g.Code == code &&
            g.IsActive);

        return ApiResult<bool>.Success(exists);
    }

    [HttpPost("batch")]
    public async Task<ApiResult> BatchUpdate([FromBody] GeneralizedCodeBatchRequestDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Domain) || dto.Items == null || dto.Items.Count == 0)
            return ApiResult.Error("Invalid request");

        var app = _currentUser.App ?? "cpm";
        var site = _currentUser.Site;
        var domain = dto.Domain;

        var query = _db.GeneralizedCodes.AsQueryable();
        query = query.Where(g => g.App == app);
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));
        else
            query = query.Where(g => string.IsNullOrEmpty(g.Site));

        var existing = await query
            .Where(g => g.Domain == domain)
            .ToDictionaryAsync(g => g.Code);

        var incomingCodes = dto.Items.Select(d => d.Code).ToHashSet();

        foreach (var item in dto.Items)
        {
            if (existing.TryGetValue(item.Code, out var entity))
            {
                entity.Label = item.Label;
                entity.LabelEn = item.LabelEn;
                entity.SortOrder = item.SortOrder;
                entity.IsActive = item.IsActive;
                entity.TagType = item.TagType;
                entity.Attributes = item.Attributes;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.GeneralizedCodes.Add(new SysGeneralizedCode
                {
                    Domain = domain,
                    Code = item.Code,
                    Label = item.Label,
                    LabelEn = item.LabelEn,
                    SortOrder = item.SortOrder,
                    IsActive = item.IsActive,
                    TagType = item.TagType,
                    Attributes = item.Attributes,
                    App = app,
                    Site = site,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Soft-delete items not in incoming list
        foreach (var kv in existing)
        {
            if (!incomingCodes.Contains(kv.Key))
            {
                kv.Value.IsActive = false;
                kv.Value.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPost("init")]
    public async Task<ApiResult> InitDefaults()
    {
        var app = _currentUser.App ?? "cpm";
        var site = _currentUser.Site;

        var query = _db.GeneralizedCodes.AsQueryable();
        query = query.Where(g => g.App == app);
        if (!string.IsNullOrWhiteSpace(site))
            query = query.Where(g => g.Site == site || string.IsNullOrEmpty(g.Site));
        else
            query = query.Where(g => string.IsNullOrEmpty(g.Site));

        var existingCount = await query.CountAsync();
        if (existingCount > 0)
            return ApiResult.Success(); // Already initialized

        var defaults = new List<SysGeneralizedCode>
        {
            // OPP_STAGE
            new() { Domain = "OPP_STAGE", Code = "NEW", Label = "新建", LabelEn = "New", SortOrder = 0, TagType = "info", App = app, Site = site },
            new() { Domain = "OPP_STAGE", Code = "QUALIFIED", Label = "需求确认", LabelEn = "Qualified", SortOrder = 1, TagType = "warning", App = app, Site = site },
            new() { Domain = "OPP_STAGE", Code = "PROPOSAL", Label = "方案中", LabelEn = "Proposal", SortOrder = 2, TagType = "warning", App = app, Site = site },
            new() { Domain = "OPP_STAGE", Code = "NEGOTIATION", Label = "谈判中", LabelEn = "Negotiation", SortOrder = 3, TagType = "warning", App = app, Site = site },
            new() { Domain = "OPP_STAGE", Code = "CLOSED", Label = "已成交", LabelEn = "Closed", SortOrder = 4, TagType = "success", App = app, Site = site },
            new() { Domain = "OPP_STAGE", Code = "LOST", Label = "已流失", LabelEn = "Lost", SortOrder = 5, TagType = "danger", App = app, Site = site },

            // QUO_STATUS
            new() { Domain = "QUO_STATUS", Code = "0", Label = "草稿", LabelEn = "Draft", SortOrder = 0, TagType = "info", App = app, Site = site },
            new() { Domain = "QUO_STATUS", Code = "1", Label = "待评审", LabelEn = "Pending Review", SortOrder = 1, TagType = "warning", App = app, Site = site },
            new() { Domain = "QUO_STATUS", Code = "2", Label = "待审批", LabelEn = "Pending Approval", SortOrder = 2, TagType = "warning", App = app, Site = site },
            new() { Domain = "QUO_STATUS", Code = "3", Label = "已发布", LabelEn = "Issued", SortOrder = 3, TagType = "success", App = app, Site = site },
            new() { Domain = "QUO_STATUS", Code = "9", Label = "已完成", LabelEn = "Completed", SortOrder = 4, TagType = "success", App = app, Site = site },

            // INDUSTRY
            new() { Domain = "INDUSTRY", Code = "AUTOMOTIVE", Label = "汽车", LabelEn = "Automotive", SortOrder = 0, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Data Storage", Label = "数据存储", LabelEn = "Data Storage", SortOrder = 1, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Domestic Appliances", Label = "家用电器", LabelEn = "Domestic Appliances", SortOrder = 2, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "HealthCare", Label = "医疗保健", LabelEn = "Health Care", SortOrder = 3, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Imaging and Printing", Label = "影像与印刷", LabelEn = "Imaging and Printing", SortOrder = 4, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Leisure", Label = "休闲", LabelEn = "Leisure", SortOrder = 5, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Machinery", Label = "机械", LabelEn = "Machinery", SortOrder = 6, App = app, Site = site },
            new() { Domain = "INDUSTRY", Code = "Others", Label = "其他", LabelEn = "Others", SortOrder = 7, App = app, Site = site },

            // CURRENCY
            new() { Domain = "CURRENCY", Code = "CNY", Label = "人民币", LabelEn = "CNY", SortOrder = 0, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "USD", Label = "美元", LabelEn = "USD", SortOrder = 1, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "MYN", Label = "马来西亚林吉特", LabelEn = "MYR", SortOrder = 2, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "SGD", Label = "新加坡元", LabelEn = "SGD", SortOrder = 3, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "EUR", Label = "欧元", LabelEn = "EUR", SortOrder = 4, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "JPY", Label = "日元", LabelEn = "JPY", SortOrder = 5, App = app, Site = site },
            new() { Domain = "CURRENCY", Code = "HKD", Label = "港币", LabelEn = "HKD", SortOrder = 6, App = app, Site = site },

            // PM_TRACE_STATUS
            new() { Domain = "PM_TRACE_STATUS", Code = "0", Label = "草稿", LabelEn = "Draft", SortOrder = 0, TagType = "info", App = app, Site = site },
            new() { Domain = "PM_TRACE_STATUS", Code = "1", Label = "进行中", LabelEn = "Running", SortOrder = 1, TagType = "warning", App = app, Site = site },
            new() { Domain = "PM_TRACE_STATUS", Code = "2", Label = "已完成", LabelEn = "Completed", SortOrder = 2, TagType = "success", App = app, Site = site },

            // ACT_STATUS
            new() { Domain = "ACT_STATUS", Code = "0", Label = "有效", LabelEn = "Active", SortOrder = 0, TagType = "success", App = app, Site = site },
            new() { Domain = "ACT_STATUS", Code = "1", Label = "过期", LabelEn = "Expired", SortOrder = 1, TagType = "warning", App = app, Site = site },
            new() { Domain = "ACT_STATUS", Code = "2", Label = "作废", LabelEn = "Invalidated", SortOrder = 2, TagType = "danger", App = app, Site = site },

            // APPROVAL_STEP_TYPE
            new() { Domain = "APPROVAL_STEP_TYPE", Code = "REVIEW", Label = "评审", LabelEn = "Review", SortOrder = 0, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_TYPE", Code = "APPROVAL", Label = "审批", LabelEn = "Approval", SortOrder = 1, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_TYPE", Code = "NOTIFY", Label = "通知", LabelEn = "Notify", SortOrder = 2, App = app, Site = site },

            // APPROVAL_STEP_MODE
            new() { Domain = "APPROVAL_STEP_MODE", Code = "SEQUENTIAL", Label = "串行", LabelEn = "Sequential", SortOrder = 0, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_MODE", Code = "PARALLEL", Label = "会签", LabelEn = "Parallel (All)", SortOrder = 1, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_MODE", Code = "PARALLEL_ANY", Label = "或签", LabelEn = "Parallel (Any)", SortOrder = 2, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_MODE", Code = "CONDITIONAL", Label = "条件分支", LabelEn = "Conditional", SortOrder = 3, App = app, Site = site },
            new() { Domain = "APPROVAL_STEP_MODE", Code = "CC", Label = "抄送", LabelEn = "CC", SortOrder = 4, App = app, Site = site },

            // REJECT_BEHAVIOR
            new() { Domain = "REJECT_BEHAVIOR", Code = "REJECT_AND_CLOSE", Label = "关闭流程", LabelEn = "Close Flow", SortOrder = 0, App = app, Site = site },
            new() { Domain = "REJECT_BEHAVIOR", Code = "REJECT_TO_PREV", Label = "退回上一步", LabelEn = "Return to Previous", SortOrder = 1, App = app, Site = site },
            new() { Domain = "REJECT_BEHAVIOR", Code = "REJECT_TO_STEP", Label = "退回指定步", LabelEn = "Return to Step", SortOrder = 2, App = app, Site = site },
            new() { Domain = "REJECT_BEHAVIOR", Code = "REJECT_TO_START", Label = "退回起点", LabelEn = "Return to Start", SortOrder = 3, App = app, Site = site },
            new() { Domain = "REJECT_BEHAVIOR", Code = "REJECT_TO_REQUESTOR", Label = "退回发起人", LabelEn = "Return to Requestor", SortOrder = 4, App = app, Site = site },

            // RULE_TYPE
            new() { Domain = "RULE_TYPE", Code = "FIXED_ROLE", Label = "固定角色", LabelEn = "Fixed Role", SortOrder = 0, App = app, Site = site },
            new() { Domain = "RULE_TYPE", Code = "FIXED_USER", Label = "固定用户", LabelEn = "Fixed User", SortOrder = 1, App = app, Site = site },
            new() { Domain = "RULE_TYPE", Code = "ORG_TREE", Label = "组织树", LabelEn = "Org Tree", SortOrder = 2, App = app, Site = site },
            new() { Domain = "RULE_TYPE", Code = "SUBMITTER", Label = "发起人", LabelEn = "Submitter", SortOrder = 3, App = app, Site = site },

            // SEQUENCE_RESET_RULE
            new() { Domain = "SEQUENCE_RESET_RULE", Code = "0", Label = "从不", LabelEn = "Never", SortOrder = 0, App = app, Site = site },
            new() { Domain = "SEQUENCE_RESET_RULE", Code = "1", Label = "每日", LabelEn = "Daily", SortOrder = 1, App = app, Site = site },
            new() { Domain = "SEQUENCE_RESET_RULE", Code = "2", Label = "每月", LabelEn = "Monthly", SortOrder = 2, App = app, Site = site },
            new() { Domain = "SEQUENCE_RESET_RULE", Code = "3", Label = "每年", LabelEn = "Yearly", SortOrder = 3, App = app, Site = site },

            // LOGIN_DOMAIN
            new() { Domain = "LOGIN_DOMAIN", Code = "NT01", Label = "NT01", LabelEn = "NT01", SortOrder = 0, App = app, Site = site },
            new() { Domain = "LOGIN_DOMAIN", Code = "MY01", Label = "MY01", LabelEn = "MY01", SortOrder = 1, App = app, Site = site },
        };

        _db.GeneralizedCodes.AddRange(defaults);
        await _db.SaveChangesAsync();

        return ApiResult.Success();
    }
}

public class GeneralizedCodeDto
{
    public long Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? LabelEn { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? TagType { get; set; }
    public string? Attributes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DomainSummaryDto
{
    public string Domain { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class GeneralizedCodeBatchRequestDto
{
    public string Domain { get; set; } = string.Empty;
    public List<GeneralizedCodeBatchItemDto> Items { get; set; } = new();
}

public class GeneralizedCodeBatchItemDto
{
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? LabelEn { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TagType { get; set; }
    public string? Attributes { get; set; }
}
