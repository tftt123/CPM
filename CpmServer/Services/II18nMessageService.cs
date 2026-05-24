using CpmServer.Models;

namespace CpmServer.Services;

public interface II18nMessageService
{
    Task<List<SysI18nMessage>> GetListAsync(string? category = null, string? keyword = null, string? site = null);
    Task<Dictionary<string, Dictionary<string, string>>> GetActiveMessagesAsync(string? site = null);
    // Returns: key -> locale -> value, where key is full dot-notation like "common.required"
    Task<SysI18nMessage> CreateAsync(SysI18nMessage message, string? site = null);
    Task UpdateAsync(long id, SysI18nMessage message, string? site = null);
    Task DeleteAsync(long id);
}
