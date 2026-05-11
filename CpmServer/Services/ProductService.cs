using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.Product;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class ProductService : IProductService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ProductService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProductDto>> GetListAsync(int pageNum, int pageSize, string? keyword)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(p => p.Site == _currentUser.Site);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.ProductName.Contains(keyword) || p.ProductCode.Contains(keyword));
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(p => p.Id)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Material = p.Material,
                SurfaceTreatment = p.SurfaceTreatment,
                Site = p.Site
            })
            .ToListAsync();

        return PagedResult<ProductDto>.Of(list, total, pageNum, pageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(long id)
    {
        var query = _db.Products.AsNoTracking()
            .Where(p => p.Id == id);

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(p => p.Site == _currentUser.Site);
        }

        return await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Material = p.Material,
                SurfaceTreatment = p.SurfaceTreatment,
                Site = p.Site
            })
            .FirstOrDefaultAsync();
    }

    public async Task<long> AddAsync(ProductDto dto)
    {
        if (await _db.Products.AnyAsync(p => p.ProductCode == dto.ProductCode))
            throw new BusinessException("产品编码已存在");

        var entity = new CrmProduct
        {
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            Material = dto.Material,
            SurfaceTreatment = dto.SurfaceTreatment,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(long id, ProductDto dto)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null)
            throw new BusinessException("产品不存在");

        entity.ProductName = dto.ProductName;
        entity.Material = dto.Material;
        entity.SurfaceTreatment = dto.SurfaceTreatment;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity != null)
        {
            _db.Products.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
