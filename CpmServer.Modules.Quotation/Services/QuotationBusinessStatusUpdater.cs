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
}
