using CpmServer.Modules.SequenceRule.DTOs;

namespace CpmServer.Modules.SequenceRule.Contracts;

public interface ISequenceRuleService
{
    Task<List<SequenceRuleDto>> GetRulesAsync(string? site);
    Task<SequenceRuleDto?> GetRuleByIdAsync(long id);
    Task<SequenceRuleDto?> GetRuleByModuleTypeAsync(string moduleType, string? site);
    Task<long> CreateRuleAsync(SequenceRuleCreateDto dto, string? site);
    Task UpdateRuleAsync(long id, SequenceRuleUpdateDto dto);
    Task DeleteRuleAsync(long id);
    Task<string> GenerateSequenceAsync(string moduleType, string? site);
}
