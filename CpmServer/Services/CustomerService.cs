using CpmServer.Common;
using CpmServer.Data;
using CpmServer.DTOs.Customer;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class CustomerService : ICustomerService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CustomerService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CustomerDto>> GetListAsync(int pageNum, int pageSize, string? keyword)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(c => c.Site == _currentUser.Site);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c => c.CustomerName.Contains(keyword) || c.CustomerCode.Contains(keyword));
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Industry = c.Industry,
                ContactName = c.ContactName,
                Site = c.Site
            })
            .ToListAsync();

        return PagedResult<CustomerDto>.Of(list, total, pageNum, pageSize);
    }

    public async Task<CustomerDto?> GetByIdAsync(long id)
    {
        var query = _db.Customers.AsNoTracking().Where(c => c.Id == id);
        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(c => c.Site == _currentUser.Site);
        }
        return await query
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Industry = c.Industry,
                ContactName = c.ContactName,
                Site = c.Site
            })
            .FirstOrDefaultAsync();
    }

    public async Task<long> AddAsync(CustomerDto dto)
    {
        var exists = await _db.Customers.AnyAsync(c => c.CustomerCode == dto.CustomerCode);
        if (exists)
            throw new BusinessException("客户编码已存在");

        var entity = new CrmCustomer
        {
            CustomerCode = dto.CustomerCode,
            CustomerName = dto.CustomerName,
            Industry = dto.Industry,
            ContactName = dto.ContactName,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(long id, CustomerDto dto)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity == null)
            throw new BusinessException("客户不存在");

        entity.CustomerName = dto.CustomerName;
        entity.Industry = dto.Industry;
        entity.ContactName = dto.ContactName;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity != null)
        {
            _db.Customers.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}