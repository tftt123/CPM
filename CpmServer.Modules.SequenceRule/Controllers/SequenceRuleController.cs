using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Modules.SequenceRule.Contracts;
using CpmServer.Modules.SequenceRule.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.SequenceRule.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SequenceRuleController : ControllerBase
{
    private readonly ISequenceRuleService _sequenceRuleService;
    private readonly CpmDbContext _db;

    public SequenceRuleController(ISequenceRuleService sequenceRuleService, CpmDbContext db)
    {
        _sequenceRuleService = sequenceRuleService;
        _db = db;
    }

    private string? GetCurrentSite()
    {
        var siteClaim = User.FindFirst("Site")?.Value;
        return siteClaim;
    }

    [HttpGet]
    public async Task<ApiResult<List<SequenceRuleDto>>> GetRules()
    {
        var site = GetCurrentSite();
        var result = await _sequenceRuleService.GetRulesAsync(site);
        return ApiResult<List<SequenceRuleDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<SequenceRuleDto?>> GetRuleById(long id)
    {
        var result = await _sequenceRuleService.GetRuleByIdAsync(id);
        return ApiResult<SequenceRuleDto?>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResult<long>> CreateRule([FromBody] SequenceRuleCreateDto dto)
    {
        var site = GetCurrentSite();
        var id = await _sequenceRuleService.CreateRuleAsync(dto, site);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> UpdateRule(long id, [FromBody] SequenceRuleUpdateDto dto)
    {
        await _sequenceRuleService.UpdateRuleAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> DeleteRule(long id)
    {
        await _sequenceRuleService.DeleteRuleAsync(id);
        return ApiResult.Success();
    }

    /// <summary>
    /// 生成流水号：站点代码 + 自定义前缀 + YY + N位流水号
    /// </summary>
    [HttpPost("generate")]
    public async Task<ApiResult<string>> GenerateSequence([FromBody] GenerateSequenceRequestDto dto)
    {
        var site = GetCurrentSite();
        var generatedNo = await _sequenceRuleService.GenerateSequenceAsync(dto.ModuleType, site);
        return ApiResult<string>.Success(generatedNo);
    }
}
