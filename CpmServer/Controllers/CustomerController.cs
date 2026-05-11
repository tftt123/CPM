using CpmServer.Common;
using CpmServer.DTOs.Customer;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("list")]
    public async Task<ApiResult<PagedResult<CustomerDto>>> List(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _customerService.GetListAsync(pageNum, pageSize, keyword);
        return ApiResult<PagedResult<CustomerDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<CustomerDto?>> GetById(long id)
    {
        var result = await _customerService.GetByIdAsync(id);
        return ApiResult<CustomerDto?>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Add([FromBody] CustomerDto dto)
    {
        var id = await _customerService.AddAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> Update(long id, [FromBody] CustomerDto dto)
    {
        await _customerService.UpdateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _customerService.DeleteAsync(id);
        return ApiResult.Success();
    }
}