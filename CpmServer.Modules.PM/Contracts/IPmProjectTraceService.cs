using CpmServer.Modules.PM.DTOs;

namespace CpmServer.Modules.PM.Contracts;

public interface IPmProjectTraceService
{
    Task<PmProjectTraceListResponse> GetListAsync(string? keyword, int? status, int page, int pageSize);
    Task<PmProjectTraceDetailDto?> GetDetailAsync(long id);
    Task<List<PmProjectTraceStepCycleTimeListItemDto>> GetAllStepsWithLatestCycleTimeAsync(string? keyword);
    Task<long> SubmitCycleTimeChangeRequestAsync(long stepId, long traceId, string submitterId, string submitterName, List<PmStepCycleTimeChangeDetailDto> changes);
    Task ExecuteApprovedChangeRequestAsync(long requestId);
    Task RejectChangeRequestAsync(long requestId, string? remarks);
    Task<long> CreateAsync(PmProjectTraceCreateRequest dto);
    Task UpdateAsync(long id, PmProjectTraceUpdateRequest dto);
    Task DeleteAsync(long id);
}
