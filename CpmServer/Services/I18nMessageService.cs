using CpmServer.Models;
using CpmServer.Data;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Services;

public class I18nMessageService : II18nMessageService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public I18nMessageService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private string? CurrentSite => _currentUser.Site;

    public async Task<List<SysI18nMessage>> GetListAsync(string? category = null, string? keyword = null, string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        var query = _db.I18nMessages.AsQueryable();

        if (!string.IsNullOrEmpty(effectiveSite))
            query = query.Where(m => m.Site == effectiveSite);
        else
            query = query.Where(m => string.IsNullOrEmpty(m.Site));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(m => m.Category == category);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(m =>
                m.MessageKey.Contains(keyword) ||
                (m.ZhValue != null && m.ZhValue.Contains(keyword)) ||
                (m.EnValue != null && m.EnValue.Contains(keyword)));

        return await query.OrderBy(m => m.Category).ThenBy(m => m.MessageKey).ToListAsync();
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> GetActiveMessagesAsync(string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        var query = _db.I18nMessages.Where(m => m.IsActive);

        if (!string.IsNullOrEmpty(effectiveSite))
            query = query.Where(m => m.Site == effectiveSite);
        else
            query = query.Where(m => string.IsNullOrEmpty(m.Site));

        var messages = await query.ToListAsync();

        // Returns: key -> locale -> value, where key is full dot-notation like "common.required"
        var result = new Dictionary<string, Dictionary<string, string>>();
        foreach (var msg in messages)
        {
            result[msg.MessageKey] = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(msg.ZhValue))
                result[msg.MessageKey]["zh"] = msg.ZhValue;
            if (!string.IsNullOrEmpty(msg.EnValue))
                result[msg.MessageKey]["en"] = msg.EnValue;
        }

        return result;
    }

    public async Task<SysI18nMessage> CreateAsync(SysI18nMessage message, string? site = null)
    {
        var effectiveSite = site ?? CurrentSite;
        message.Site = effectiveSite;
        message.CreatedAt = DateTime.Now;
        message.UpdatedAt = DateTime.Now;

        _db.I18nMessages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public async Task UpdateAsync(long id, SysI18nMessage message, string? site = null)
    {
        var existing = await _db.I18nMessages.FindAsync(id);
        if (existing == null) throw new Exception("Message not found");

        existing.MessageKey = message.MessageKey;
        existing.Category = message.Category;
        existing.ZhValue = message.ZhValue;
        existing.EnValue = message.EnValue;
        existing.IsActive = message.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var existing = await _db.I18nMessages.FindAsync(id);
        if (existing != null)
        {
            _db.I18nMessages.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}
