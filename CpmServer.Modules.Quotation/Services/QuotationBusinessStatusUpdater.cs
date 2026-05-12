using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Quotation.Services;

public class QuotationBusinessStatusUpdater : IBusinessStatusUpdater
{
    private readonly CpmDbContext _db;

    public QuotationBusinessStatusUpdater(CpmDbContext db)
    {
        _db = db;
    }

    public bool Supports(string businessType) => businessType == "Quotation";

    public async Task UpdateStatusAsync(long businessId, int approvalStatus, long? currentStepId, decimal? reviewCost)
    {
        var quotation = await _db.Quotations.FindAsync(businessId);
        if (quotation == null) return;

        if (approvalStatus == 1)
        {
            quotation.Status = 3;
            // 审批通过后自动创建产品跟踪记录
            await CreateProjectTraceAsync(quotation);
        }
        else if (approvalStatus == 2)
        {
            quotation.Status = 0;
        }
        else
        {
            if (currentStepId.HasValue)
            {
                var step = await _db.ApprovalSteps.FindAsync(currentStepId.Value);
                if (step?.StepType == "REVIEW")
                    quotation.Status = 1;
                else if (step?.StepType == "APPROVAL")
                    quotation.Status = 2;
            }
        }

        if (reviewCost.HasValue)
        {
            quotation.TotalAmount = reviewCost.Value;
        }

        quotation.CurrentStepId = currentStepId;
        quotation.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task CreateProjectTraceAsync(QuoQuotation quotation)
    {
        // 避免重复创建
        var exists = await _db.ProjectTraces.AnyAsync(t => t.QuotationId == quotation.Id);
        if (exists) return;

        // 加载客户和产品信息
        var customer = await _db.Customers.FindAsync(quotation.CustomerId);
        var items = await _db.QuotationItems
            .Where(i => i.QuotationId == quotation.Id)
            .ToListAsync();

        // 取第一个明细的产品信息
        var firstItem = items.FirstOrDefault();
        CrmProduct? product = null;
        if (firstItem?.ProductId.HasValue == true)
        {
            product = await _db.Products.FindAsync(firstItem.ProductId.Value);
        }

        var trace = new PmProjectTrace
        {
            QuotationId = quotation.Id,
            CustomerId = quotation.CustomerId,
            CustomerName = customer?.CustomerName ?? string.Empty,
            ProductId = firstItem?.ProductId,
            ProductCode = product?.ProductCode ?? string.Empty,
            ProductName = product?.ProductName,
            Status = 0,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        // 从报价单明细生成工序步骤
        int order = 1;
        foreach (var item in items)
        {
            var step = new PmProjectTraceStep
            {
                StepOrder = order++,
                ProcessName = item.ProcessType ?? item.EquipmentType ?? item.Equipment ?? $"步骤{order - 1}",
                CycleTime = item.CycleTime,
            };
            trace.Steps.Add(step);
        }

        _db.ProjectTraces.Add(trace);
        await _db.SaveChangesAsync();
    }
}
