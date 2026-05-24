using CpmServer.Models;

namespace CpmServer.Services;

public interface IFieldControlService
{
    Task<List<SysFieldControl>> GetFieldsAsync(string moduleCode, string pageCode, string? site = null);
    Task<List<SysFieldControl>> InitFieldsAsync(string moduleCode, string pageCode, List<FieldInitDto> fields, string? site = null);
    Task UpdateBatchAsync(List<SysFieldControl> fields, string? site = null);
    List<string> GetModules();
    List<string> GetPages(string moduleCode);
}

public class FieldInitDto
{
    public string FieldCode { get; set; } = string.Empty;
    public bool DefaultRequired { get; set; }
}
