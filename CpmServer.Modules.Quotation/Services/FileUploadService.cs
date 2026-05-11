using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Modules.Quotation.DTOs;
using CpmServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Quotation.Services;

public interface IFileUploadService
{
    Task<FileRecordDto> UploadAsync(IFormFile file, string moduleType, long? businessId);
    Task<List<FileRecordDto>> GetFilesAsync(string moduleType, long businessId);
    Task DeleteFileAsync(long id);
}

public class FileUploadService : IFileUploadService
{
    private readonly CpmDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(CpmDbContext db, IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<FileRecordDto> UploadAsync(IFormFile file, string moduleType, long? businessId)
    {
        if (file == null || file.Length == 0)
            throw new BusinessException("文件不能为空");

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", moduleType.ToLower());
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var record = new SysFileRecord
        {
            FileName = fileName,
            OriginalName = file.FileName,
            FilePath = $"/uploads/{moduleType.ToLower()}/{fileName}",
            FileType = file.ContentType,
            FileSize = file.Length,
            ModuleType = moduleType,
            BusinessId = businessId,
            CreatedAt = DateTime.Now
        };

        _db.FileRecords.Add(record);
        await _db.SaveChangesAsync();

        _logger.LogInformation("文件已上传: {FileName}, Module={ModuleType}, BusinessId={BusinessId}", fileName, moduleType, businessId);

        return ToDto(record);
    }

    public async Task<List<FileRecordDto>> GetFilesAsync(string moduleType, long businessId)
    {
        return await _db.FileRecords
            .Where(f => f.ModuleType == moduleType && f.BusinessId == businessId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => ToDto(f))
            .ToListAsync();
    }

    public async Task DeleteFileAsync(long id)
    {
        var record = await _db.FileRecords.FindAsync(id);
        if (record == null) return;

        var fullPath = Path.Combine(_env.WebRootPath, record.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _db.FileRecords.Remove(record);
        await _db.SaveChangesAsync();
    }

    private static FileRecordDto ToDto(SysFileRecord f) => new()
    {
        Id = f.Id,
        FileName = f.FileName,
        OriginalName = f.OriginalName,
        FileType = f.FileType,
        FileSize = f.FileSize,
        ModuleType = f.ModuleType,
        BusinessId = f.BusinessId,
        CreatedAt = f.CreatedAt
    };
}
