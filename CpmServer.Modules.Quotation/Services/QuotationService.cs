using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Modules.Quotation.Contracts;
using CpmServer.Modules.Quotation.DTOs;
using CpmServer.Modules.SequenceRule.Contracts;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Quotation.Services;

public class QuotationService : IQuotationService
{
    private readonly CpmDbContext _db;
    private readonly ILogger<QuotationService> _logger;
    private readonly IApprovalService _approvalService;
    private readonly ISequenceRuleService _sequenceRuleService;
    private readonly ICurrentUser _currentUser;
    private readonly IGeneralizedCodeService _gc;

    public QuotationService(CpmDbContext db, ILogger<QuotationService> logger, IApprovalService approvalService, ISequenceRuleService sequenceRuleService, ICurrentUser currentUser, IGeneralizedCodeService gc)
    {
        _db = db;
        _logger = logger;
        _approvalService = approvalService;
        _sequenceRuleService = sequenceRuleService;
        _currentUser = currentUser;
        _gc = gc;
    }

    #region 商机管理

    public async Task<PagedResult<OpportunityDto>> GetOpportunityListAsync(int pageNum, int pageSize, string? keyword, string? stage)
    {
        var query = _db.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(o => o.Site == _currentUser.Site);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(o =>
                o.OpportunityNo.Contains(keyword) ||
                o.Title.Contains(keyword) ||
                (o.Customer != null && o.Customer.CustomerName.Contains(keyword)));
        }

        if (!string.IsNullOrEmpty(stage))
        {
            query = query.Where(o => o.Stage == stage);
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = list.Select(o => new OpportunityDto
        {
            Id = o.Id,
            OpportunityNo = o.OpportunityNo,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.CustomerName ?? string.Empty,
            Title = o.Title,
            ExpectedAmount = o.ExpectedAmount,
            QuoteDeadline = o.QuoteDeadline,
            Stage = o.Stage,
            Status = o.Status,
            OwnerId = o.OwnerId,
            OwnerName = o.Owner?.RealName ?? o.Owner?.Username ?? string.Empty,
            Site = o.Site,
            CreatedAt = o.CreatedAt
        }).ToList();

        return PagedResult<OpportunityDto>.Of(dtoList, total, pageNum, pageSize);
    }

    public async Task<OpportunityDto?> GetOpportunityByIdAsync(long id)
    {
        var query = _db.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.Owner)
            .Where(o => o.Id == id);

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(o => o.Site == _currentUser.Site);
        }

        var o = await query.FirstOrDefaultAsync();

        if (o == null) return null;

        return new OpportunityDto
        {
            Id = o.Id,
            OpportunityNo = o.OpportunityNo,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.CustomerName ?? string.Empty,
            Title = o.Title,
            ExpectedAmount = o.ExpectedAmount,
            QuoteDeadline = o.QuoteDeadline,
            Stage = o.Stage,
            Status = o.Status,
            OwnerId = o.OwnerId,
            OwnerName = o.Owner?.RealName ?? o.Owner?.Username ?? string.Empty,
            Site = o.Site,
            CreatedAt = o.CreatedAt
        };
    }

    public async Task<long> CreateOpportunityAsync(OpportunityCreateDto dto, long userId)
    {
        var opportunityNo = await GenerateOpportunityNoAsync();

        var entity = new QuoOpportunity
        {
            OpportunityNo = opportunityNo,
            CustomerId = dto.CustomerId,
            Title = dto.Title,
            ExpectedAmount = dto.ExpectedAmount,
            QuoteDeadline = dto.QuoteDeadline,
            Stage = "NEW",
            Status = 0,
            OwnerId = userId,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Opportunities.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("商机已创建: OpportunityNo={OpportunityNo}, User={UserId}", opportunityNo, userId);
        return entity.Id;
    }

    public async Task UpdateOpportunityAsync(long id, OpportunityCreateDto dto)
    {
        var entity = await _db.Opportunities.FindAsync(id);
        if (entity == null) return;

        entity.CustomerId = dto.CustomerId;
        entity.Title = dto.Title;
        entity.ExpectedAmount = dto.ExpectedAmount;
        entity.QuoteDeadline = dto.QuoteDeadline;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateOpportunityStageAsync(long id, string stage)
    {
        var entity = await _db.Opportunities.FindAsync(id);
        if (entity == null) return;

        await _gc.ValidateAsync("OPP_STAGE", stage, entity.Site, _currentUser.App ?? "cpm");

        entity.Stage = stage;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteOpportunityAsync(long id)
    {
        var entity = await _db.Opportunities.FindAsync(id);
        if (entity == null) return;

        // 检查是否有关联报价单
        var hasQuotations = await _db.Quotations.AnyAsync(q => q.OpportunityId == id);
        if (hasQuotations)
        {
            throw new InvalidOperationException("该商机已有关联报价单，无法删除");
        }

        _db.Opportunities.Remove(entity);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region 报价单管理
    public async Task<PagedResult<QuotationDto>> GetQuotationListAsync(int pageNum, int pageSize, string? keyword, int? status)
    {
        var query = _db.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .Include(q => q.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(q => q.Site == _currentUser.Site);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(q =>
                q.QuotationNo.Contains(keyword) ||
                (q.Customer != null && q.Customer.CustomerName.Contains(keyword)) ||
                (q.Opportunity != null && q.Opportunity.Title.Contains(keyword)));
        }

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = new List<QuotationDto>();
        foreach (var q in list)
        {
            var creator = await _db.Users.FindAsync(q.CreatedBy);
            string? currentStepName = null;
            if (q.CurrentStepId.HasValue)
            {
                var step = await _db.ApprovalSteps.FindAsync(q.CurrentStepId.Value);
                currentStepName = step?.StepName;
            }

            var fileIds = await _db.FileRecords
                .Where(f => f.ModuleType == "quotation" && f.BusinessId == q.Id)
                .Select(f => f.Id)
                .ToListAsync();

            dtoList.Add(new QuotationDto
            {
                Id = q.Id,
                QuotationNo = q.QuotationNo,
                RfqNo = q.RfqNo,
                Title = q.Title,
                OpportunityId = q.OpportunityId,
                OpportunityTitle = q.Opportunity?.Title ?? string.Empty,
                CustomerId = q.CustomerId,
                CustomerName = q.Customer?.CustomerName ?? string.Empty,
                CustomerCurrency = q.Customer?.Currency,
                TotalAmount = q.TotalAmount,
                CurrentStepId = q.CurrentStepId,
                CurrentStepName = currentStepName,
                Status = q.Status,
                CreatedBy = q.CreatedBy,
                CreatedByName = creator?.RealName ?? creator?.Username ?? string.Empty,
                Site = q.Site,
                CreatedAt = q.CreatedAt,
                FileIds = fileIds,
                Items = q.Items?.Select(i => new QuotationItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.ProductName ?? $"产品#{i.ProductId}",
                    Qty = i.Qty,
                    LineAmount = i.LineAmount,
                    ProcessType = i.ProcessType,
                    EquipmentType = i.EquipmentType,
                    Equipment = i.Equipment,
                    CycleTime = i.CycleTime,
                    HourlyRate = i.HourlyRate,
                    Cost = i.Cost,
                    PackagingCost = i.PackagingCost,
                    TransportCost = i.TransportCost,
                    IsProcessRow = i.IsProcessRow
                }).ToList() ?? new List<QuotationItemDto>()
            });
        }

        return PagedResult<QuotationDto>.Of(dtoList, total, pageNum, pageSize);
    }

    public async Task<QuotationDto?> GetQuotationByIdAsync(long id)
    {
        var q = await _db.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .Include(q => q.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (q == null) return null;

        var creator = await _db.Users.FindAsync(q.CreatedBy);
        string? currentStepName = null;
        if (q.CurrentStepId.HasValue)
        {
            var step = await _db.ApprovalSteps.FindAsync(q.CurrentStepId.Value);
            currentStepName = step?.StepName;
        }

        var fileIds = await _db.FileRecords
            .Where(f => f.ModuleType == "quotation" && f.BusinessId == q.Id)
            .Select(f => f.Id)
            .ToListAsync();

        return new QuotationDto
        {
            Id = q.Id,
            QuotationNo = q.QuotationNo,
            RfqNo = q.RfqNo,
            Title = q.Title,
            OpportunityId = q.OpportunityId,
            OpportunityTitle = q.Opportunity?.Title ?? string.Empty,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.CustomerName ?? string.Empty,
            CustomerCurrency = q.Customer?.Currency,
            TotalAmount = q.TotalAmount,
            CurrentStepId = q.CurrentStepId,
            CurrentStepName = currentStepName,
            Status = q.Status,
            CreatedBy = q.CreatedBy,
            CreatedByName = creator?.RealName ?? creator?.Username ?? string.Empty,
            Site = q.Site,
            CreatedAt = q.CreatedAt,
            FileIds = fileIds,
            Items = q.Items.Select(i => new QuotationItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.ProductName ?? $"产品#{i.ProductId}",
                Qty = i.Qty,
                LineAmount = i.LineAmount,
                ProcessType = i.ProcessType,
                EquipmentType = i.EquipmentType,
                Equipment = i.Equipment,
                CycleTime = i.CycleTime,
                HourlyRate = i.HourlyRate,
                Cost = i.Cost,
                PackagingCost = i.PackagingCost,
                TransportCost = i.TransportCost,
                IsProcessRow = i.IsProcessRow
            }).ToList()
        };
    }

    public async Task<long> CreateQuotationAsync(QuotationCreateDto dto, long userId)
    {
        // 过滤无效明细（未选择产品的行）
        var validItems = dto.Items?.Where(i => i.ProductId.HasValue).ToList() ?? new List<QuotationItemDto>();
        if (validItems.Count == 0)
        {
            throw new BusinessException("请至少添加一个有效的产品明细");
        }

        // 报价单号引用关联商机的 RFQ 编号
        var opportunity = await _db.Opportunities.FindAsync(dto.OpportunityId);
        var quotationNo = opportunity?.OpportunityNo ?? await GenerateQuotationNoAsync();

        // 计算总金额（行金额已包含包装和运输费）
        decimal totalAmount = validItems.Sum(i => i.LineAmount ?? 0);

        var entity = new QuoQuotation
        {
            QuotationNo = quotationNo,
            RfqNo = dto.RfqNo,
            Title = dto.Title,
            OpportunityId = dto.OpportunityId,
            CustomerId = dto.CustomerId,
            TotalAmount = totalAmount,
            Status = 0,
            CreatedBy = userId,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Quotations.Add(entity);
        await _db.SaveChangesAsync();

        // 添加明细
        foreach (var item in validItems)
        {
            var cost = item.CycleTime.HasValue && item.HourlyRate.HasValue
                ? item.CycleTime.Value / 3600m * item.HourlyRate.Value
                : item.Cost;
            _db.QuotationItems.Add(new QuoQuotationItem
            {
                QuotationId = entity.Id,
                ProductId = item.ProductId,
                Qty = item.Qty,
                LineAmount = item.LineAmount,
                ProcessType = item.ProcessType,
                EquipmentType = item.EquipmentType,
                Equipment = item.Equipment,
                CycleTime = item.CycleTime,
                HourlyRate = item.HourlyRate,
                Cost = cost,
                PackagingCost = item.PackagingCost,
                TransportCost = item.TransportCost,
                IsProcessRow = item.IsProcessRow
            });
        }

        await _db.SaveChangesAsync();

        // 关联附件
        if (dto.FileIds?.Count > 0)
        {
            var files = await _db.FileRecords.Where(f => dto.FileIds.Contains(f.Id)).ToListAsync();
            foreach (var file in files)
            {
                file.BusinessId = entity.Id;
            }
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("报价单已创建: QuotationNo={QuotationNo}, User={UserId}", quotationNo, userId);
        return entity.Id;
    }

    public async Task UpdateQuotationAsync(long id, QuotationCreateDto dto)
    {
        var entity = await _db.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (entity == null) return;

        // 只有草稿状态可以编辑
        if (entity.Status != 0)
        {
            throw new InvalidOperationException("只有草稿状态的报价单可以编辑");
        }

        entity.OpportunityId = dto.OpportunityId;
        entity.RfqNo = dto.RfqNo;
        entity.Title = dto.Title;
        entity.CustomerId = dto.CustomerId;
        entity.UpdatedAt = DateTime.Now;

        // 过滤无效明细
        var validItems = dto.Items?.Where(i => i.ProductId.HasValue).ToList() ?? new List<QuotationItemDto>();

        // 删除旧明细
        _db.QuotationItems.RemoveRange(entity.Items);

        // 添加新明细
        foreach (var item in validItems)
        {
            var cost = item.CycleTime.HasValue && item.HourlyRate.HasValue
                ? item.CycleTime.Value / 3600m * item.HourlyRate.Value
                : item.Cost;
            _db.QuotationItems.Add(new QuoQuotationItem
            {
                QuotationId = entity.Id,
                ProductId = item.ProductId,
                Qty = item.Qty,
                LineAmount = item.LineAmount,
                ProcessType = item.ProcessType,
                EquipmentType = item.EquipmentType,
                Equipment = item.Equipment,
                CycleTime = item.CycleTime,
                HourlyRate = item.HourlyRate,
                Cost = cost,
                PackagingCost = item.PackagingCost,
                TransportCost = item.TransportCost,
                IsProcessRow = item.IsProcessRow
            });
        }

        entity.TotalAmount = validItems.Sum(i => i.LineAmount ?? 0);
        await _db.SaveChangesAsync();

        // 关联附件
        if (dto.FileIds?.Count > 0)
        {
            var files = await _db.FileRecords.Where(f => dto.FileIds.Contains(f.Id) && f.BusinessId == null).ToListAsync();
            foreach (var file in files)
            {
                file.BusinessId = entity.Id;
            }
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteQuotationAsync(long id)
    {
        var entity = await _db.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (entity == null) return;

        if (entity.Status != 0)
        {
            throw new InvalidOperationException("只有草稿状态的报价单可以删除");
        }

        _db.QuotationItems.RemoveRange(entity.Items);
        _db.Quotations.Remove(entity);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region 审批操作

    public async Task SubmitForApprovalAsync(long quotationId, long userId)
    {
        var quotation = await _db.Quotations.FindAsync(quotationId);
        if (quotation == null)
        {
            throw new InvalidOperationException("报价单不存在");
        }

        if (quotation.Status != 0)
        {
            throw new InvalidOperationException("只有草稿状态的报价单可以提交审批");
        }

        // 检查是否已有进行中的审批流程
        var existingInstance = await _approvalService.GetInstanceAsync("Quotation", quotationId);
        if (existingInstance != null && existingInstance.Status == 0)
        {
            throw new InvalidOperationException("该报价单已有进行中的审批流程");
        }

        // 启动审批流程（Phase 1：使用 Site+ModuleType 自动匹配模板）
        var instance = await _approvalService.StartApprovalAsync(
            "Quotation", quotationId, "Quotation", quotation.Site, userId);

        quotation.Status = 1; // 待评审
        quotation.CurrentStepId = instance.CurrentStepId;
        quotation.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        _logger.LogInformation("报价单已提交审批: QuotationId={QuotationId}, InstanceId={InstanceId}",
            quotationId, instance.Id);
    }

    public async Task ProcessApprovalAsync(long quotationId, CpmServer.Modules.Quotation.DTOs.ApprovalActionDto dto, long approverId)
    {
        var instance = await _approvalService.GetInstanceAsync("Quotation", quotationId);
        if (instance == null)
        {
            throw new InvalidOperationException("该报价单没有进行中的审批流程");
        }

        if (dto.Action == "APPROVE")
        {
            await _approvalService.ApproveAsync(instance.Id, approverId, dto.Comment ?? string.Empty, dto.ReviewCost);
        }
        else if (dto.Action == "REJECT")
        {
            await _approvalService.RejectAsync(instance.Id, approverId, dto.Comment ?? string.Empty);
        }
        else if (dto.Action == "TRANSFER")
        {
            if (!dto.TransferToUserId.HasValue)
            {
                throw new InvalidOperationException("转交目标用户不能为空");
            }
            await _approvalService.TransferAsync(instance.Id, approverId, dto.TransferToUserId.Value, dto.Comment);
        }
        else
        {
            throw new InvalidOperationException("不支持的审批操作");
        }

        // 同步报价单状态
        var quotation = await _db.Quotations.FindAsync(quotationId);
        if (quotation != null)
        {
            var updatedInstance = await _approvalService.GetInstanceAsync("Quotation", quotationId);
            if (updatedInstance != null)
            {
                quotation.CurrentStepId = updatedInstance.CurrentStepId;
                quotation.Status = updatedInstance.Status switch
                {
                    0 => 1, // 进行中 -> 待评审
                    1 => 3, // 完成 -> 已发布
                    2 => 0, // 驳回 -> 草稿
                    _ => quotation.Status
                };

                if (updatedInstance.Status == 0)
                {
                    var currentStep = await _approvalService.GetCurrentStepAsync(updatedInstance.Id);
                    if (currentStep?.StepType == "APPROVAL")
                    {
                        quotation.Status = 2; // 待审批
                    }
                }

                quotation.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long quotationId)
    {
        var instance = await _approvalService.GetInstanceAsync("Quotation", quotationId);
        if (instance == null) return new List<ApprovalRecordDto>();

        return await _approvalService.GetApprovalRecordsAsync(instance.Id);
    }

    public async Task<List<ApprovalStepDto>> GetApprovalStepsAsync(long quotationId)
    {
        var instance = await _approvalService.GetInstanceAsync("Quotation", quotationId);
        if (instance == null) return new List<ApprovalStepDto>();

        return await _approvalService.GetApprovalStepsWithStatusAsync(instance.Id);
    }

    #endregion

    #region 私有方法

    private async Task<string> GenerateOpportunityNoAsync()
    {
        return await _sequenceRuleService.GenerateSequenceAsync("Opportunity", _currentUser.Site);
    }

    private async Task<string> GenerateQuotationNoAsync()
    {
        return await _sequenceRuleService.GenerateSequenceAsync("Quotation", _currentUser.Site);
    }

    #endregion
}
