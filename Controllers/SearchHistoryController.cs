using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink.Models;
using PharmaLink.Services;

namespace PharmaLink.Controllers;

[Authorize]
public class SearchHistoryController : Controller
{
    private readonly ISearchHistoryService _searchHistoryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SearchHistoryController(
        ISearchHistoryService searchHistoryService,
        UserManager<ApplicationUser> userManager)
    {
        _searchHistoryService = searchHistoryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Search History";
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var history = await _searchHistoryService.GetUserSearchHistoryAsync(userId);
        return View(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
            await _searchHistoryService.ClearSearchHistoryAsync(userId);

        TempData["Success"] = "Search history cleared.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _searchHistoryService.DeleteSearchEntryAsync(id);
        return RedirectToAction("Index");
    }
}