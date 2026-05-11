using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Quotation.Services;

public class QuotationBusinessVariableProvider : IBusinessVariableProvider
{
    private readonly CpmDbContext _db;

    public QuotationBusinessVariableProvider(CpmDbContext db)
    {
        _db = db;
    }

    public bool Supports(string businessType) => businessType == "Quotation";

    public async Task<Dictionary<string, string>> GetVariablesAsync(long businessId)
    {
        var variables = new Dictionary<string, string>();
        var quotation = await _db.Quotations
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == businessId);

        if (quotation != null)
        {
            variables["QuotationNo"] = quotation.QuotationNo;
            variables["CustomerName"] = quotation.Customer?.CustomerName ?? "";
            variables["TotalAmount"] = quotation.TotalAmount?.ToString("N2") ?? "0.00";
            variables["Status"] = GetStatusText(quotation.Status);
            variables["Site"] = quotation.Site ?? "";
        }

        return variables;
    }

    public async Task<string?> GetCreatorEmailAsync(long businessId)
    {
        var quotation = await _db.Quotations.FindAsync(businessId);
        if (quotation != null)
        {
            var creator = await _db.Users.FindAsync(quotation.CreatedBy);
            return creator?.Email;
        }
        return null;
    }

    public async Task<string?> GetBusinessSiteAsync(long businessId)
    {
        var quotation = await _db.Quotations.FindAsync(businessId);
        return quotation?.Site;
    }

    private static string GetStatusText(int status)
    {
        return status switch
        {
            0 => "草稿",
            1 => "待评审",
            2 => "待审批",
            3 => "已发布",
            9 => "已完成",
            _ => "未知"
        };
    }
}
