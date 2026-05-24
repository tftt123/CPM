using CpmServer.Constants;
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

        if (approvalStatus == ApprovalConstants.InstanceStatus.Completed)
        {
            quotation.Status = QuotationConstants.Status.Issued;
            await CreateProjectTraceAsync(quotation);
        }
        else if (approvalStatus == ApprovalConstants.InstanceStatus.Rejected)
        {
            quotation.Status = QuotationConstants.Status.Draft;
        }
        else
        {
            if (currentStepId.HasValue)
            {
                var step = await _db.ApprovalSteps.FindAsync(currentStepId.Value);
                if (step?.StepType == ApprovalConstants.StepType.Review)
                    quotation.Status = QuotationConstants.Status.PendingReview;
                else if (step?.StepType == ApprovalConstants.StepType.Approval)
                    quotation.Status = QuotationConstants.Status.PendingApproval;
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
        var customer = await _db.Customers.FindAsync(quotation.CustomerId);
        var items = await _db.QuotationItems
            .Where(i => i.QuotationId == quotation.Id)
            .ToListAsync();

        var firstItem = items.FirstOrDefault();
        CrmProduct? product = null;
        if (firstItem?.ProductId.HasValue == true)
        {
            product = await _db.Products.FindAsync(firstItem.ProductId.Value);
        }

        var trace = await _db.ProjectTraces
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.QuotationId == quotation.Id);

        if (trace == null)
        {
            trace = new PmProjectTrace
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
            _db.ProjectTraces.Add(trace);
        }
        else
        {
            trace.CustomerName = customer?.CustomerName ?? string.Empty;
            trace.ProductId = firstItem?.ProductId;
            trace.ProductCode = product?.ProductCode ?? string.Empty;
            trace.ProductName = product?.ProductName;
            trace.UpdatedAt = DateTime.Now;

            _db.ProjectTraceSteps.RemoveRange(trace.Steps);
            trace.Steps.Clear();
        }

        int order = 1;
        foreach (var item in items)
        {
            var step = new PmProjectTraceStep
            {
                StepOrder = order++,
                ProcessName = item.ProcessType ?? item.EquipmentType ?? item.Equipment ?? $"Step{order - 1}",
                Equipment = item.Equipment,
                CycleTime = item.CycleTime,
            };
            trace.Steps.Add(step);
        }

        await _db.SaveChangesAsync();
    }
}