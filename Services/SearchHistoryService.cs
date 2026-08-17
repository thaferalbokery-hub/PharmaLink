using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Services;

public class SearchHistoryService : ISearchHistoryService
{
    private readonly ApplicationDbContext _context;

    public SearchHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecordSearchAsync(string userId, string searchTerm, string searchType)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return;

        var entry = new SearchHistory
        {
            UserId = userId,
            SearchTerm = searchTerm.Trim(),
            SearchType = searchType,
            SearchDate = DateTime.UtcNow
        };

        _context.SearchHistories.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SearchHistory>> GetUserSearchHistoryAsync(string userId)
    {
        return await _context.SearchHistories
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SearchDate)
            .Take(50)
            .ToListAsync();
    }

    public async Task ClearSearchHistoryAsync(string userId)
    {
        var entries = await _context.SearchHistories
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _context.SearchHistories.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSearchEntryAsync(int id)
    {
        var entry = await _context.SearchHistories.FindAsync(id);
        if (entry != null)
        {
            _context.SearchHistories.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}