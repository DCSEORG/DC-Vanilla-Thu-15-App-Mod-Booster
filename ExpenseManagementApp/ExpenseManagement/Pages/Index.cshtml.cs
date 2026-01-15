using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagement.Pages;

public class IndexModel : PageModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IDatabaseService databaseService, ILogger<IndexModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public List<Expense> Expenses { get; set; } = new();
    public List<ExpenseStatus> Statuses { get; set; } = new();
    public List<ExpenseCategory> Categories { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterCategory { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Statuses = await _databaseService.GetStatusesAsync();
            Categories = await _databaseService.GetCategoriesAsync();

            if (!string.IsNullOrEmpty(FilterStatus) || !string.IsNullOrEmpty(FilterCategory))
            {
                Expenses = await _databaseService.FilterExpensesAsync(
                    statusName: FilterStatus,
                    categoryName: FilterCategory);
            }
            else
            {
                Expenses = await _databaseService.GetExpensesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
            ErrorMessage = $"Database connection failed: {ex.Message}";
            Expenses = new List<Expense>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _databaseService.DeleteExpenseAsync(id);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", id);
            ErrorMessage = $"Error deleting expense: {ex.Message}";
            return Page();
        }
    }
}
