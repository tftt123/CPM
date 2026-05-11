using CpmServer.Common;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Modules.Quotation.DTOs;

namespace CpmServer.Modules.Quotation.Contracts;

public interface IQuotationService
{
    // 商机
    Task<PagedResult<OpportunityDto>> GetOpportunityListAsync(int pageNum, int pageSize, string? keyword, string? stage);
    Task<OpportunityDto?> GetOpportunityByIdAsync(long id);
    Task<long> CreateOpportunityAsync(OpportunityCreateDto dto, long userId);
    Task UpdateOpportunityAsync(long id, OpportunityCreateDto dto);
    Task UpdateOpportunityStageAsync(long id, string stage);
    Task DeleteOpportunityAsync(long id);

    // 报价单
    Task<PagedResult<QuotationDto>> GetQuotationListAsync(int pageNum, int pageSize, string? keyword, int? status);
    Task<QuotationDto?> GetQuotationByIdAsync(long id);
    Task<long> CreateQuotationAsync(QuotationCreateDto dto, long userId);
    Task UpdateQuotationAsync(long id, QuotationCreateDto dto);
    Task DeleteQuotationAsync(long id);

    // 审批操作
    Task SubmitForApprovalAsync(long quotationId, long userId);
    Task ProcessApprovalAsync(long quotationId, CpmServer.Modules.Quotation.DTOs.ApprovalActionDto dto, long approverId);
    Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long quotationId);
    Task<List<ApprovalStepDto>> GetApprovalStepsAsync(long quotationId);
}
