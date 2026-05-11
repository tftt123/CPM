using CpmServer.Common;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet("config")]
    public async Task<ApiResult<EmailConfigDto?>> GetConfig()
    {
        var config = await _emailService.GetActiveConfigAsync();
        return ApiResult<EmailConfigDto?>.Success(config);
    }

    [HttpPost("config")]
    public async Task<ApiResult> SaveConfig([FromBody] EmailConfigDto dto)
    {
        await _emailService.SaveConfigAsync(dto);
        return ApiResult.Success();
    }

    [HttpPost("test")]
    public async Task<ApiResult> SendTestEmail([FromBody] TestEmailRequest dto)
    {
        await _emailService.SendTestEmailAsync(dto.ToAddress, dto.Subject, dto.Body);
        return ApiResult.Success();
    }
}

public class TestEmailRequest
{
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
