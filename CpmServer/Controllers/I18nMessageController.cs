using CpmServer.Authorization;
using CpmServer.Models;
using CpmServer.Common;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class I18nMessageController : ControllerBase
{
    private readonly II18nMessageService _service;

    public I18nMessageController(II18nMessageService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<List<SysI18nMessage>>> GetList([FromQuery] string? category, [FromQuery] string? keyword)
    {
        var list = await _service.GetListAsync(category, keyword);
        return ApiResult<List<SysI18nMessage>>.Success(list);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ApiResult<Dictionary<string, Dictionary<string, string>>>> GetActiveMessages()
    {
        var messages = await _service.GetActiveMessagesAsync();
        return ApiResult<Dictionary<string, Dictionary<string, string>>>.Success(messages);
    }

    [HttpPost]
    [Authorize(Policy = Policies.CanManageSystem)]
    public async Task<ApiResult<SysI18nMessage>> Create([FromBody] SysI18nMessage message)
    {
        var created = await _service.CreateAsync(message);
        return ApiResult<SysI18nMessage>.Success(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.CanManageSystem)]
    public async Task<ApiResult> Update(long id, [FromBody] SysI18nMessage message)
    {
        await _service.UpdateAsync(id, message);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanManageSystem)]
    public async Task<ApiResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success();
    }
}
