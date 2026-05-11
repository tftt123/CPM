using CpmServer.Common;
using CpmServer.DTOs.Product;
using CpmServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("list")]
    public async Task<ApiResult<PagedResult<ProductDto>>> List(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _productService.GetListAsync(pageNum, pageSize, keyword);
        return ApiResult<PagedResult<ProductDto>>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Add([FromBody] ProductDto dto)
    {
        var id = await _productService.AddAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> Update(long id, [FromBody] ProductDto dto)
    {
        await _productService.UpdateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _productService.DeleteAsync(id);
        return ApiResult.Success();
    }
}
