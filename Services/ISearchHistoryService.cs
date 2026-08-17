using PharmaLink.Models;

namespace PharmaLink.Services;

public interface ISearchHistoryService
{
    Task RecordSearchAsync(string userId, string searchTerm, string searchType);
    Task<List<SearchHistory>> GetUserSearchHistoryAsync(string userId);
    Task ClearSearchHistoryAsync(string userId);
    Task DeleteSearchEntryAsync(int id);
}