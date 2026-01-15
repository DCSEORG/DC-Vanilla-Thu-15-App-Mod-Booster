using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagement.Pages;

public class ApproveExpensesModel : PageModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<ApproveExpensesModel> _logger;

    public ApproveExpensesModel(IDatabaseService databaseService, ILogger<ApproveExpensesModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public List<Expense> PendingExpenses { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            PendingExpenses = await _databaseService.GetPendingExpensesAsync();
            Users = await _databaseService.GetUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending expenses");
            ErrorMessage = $"Error loading expenses: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, int reviewerId)
    {
        if (reviewerId == 0)
        {
            ErrorMessage = "Please select a reviewer";
            await OnGetAsync();
            return Page();
        }

        try
        {
            await _databaseService.ApproveExpenseAsync(id, reviewerId);
            SuccessMessage = "Expense approved successfully";
            await OnGetAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense {ExpenseId}", id);
            ErrorMessage = $"Error approving expense: {ex.Message}";
            await OnGetAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, int reviewerId)
    {
        if (reviewerId == 0)
        {
            ErrorMessage = "Please select a reviewer";
            await OnGetAsync();
            return Page();
        }

        try
        {
            await _databaseService.RejectExpenseAsync(id, reviewerId);
            SuccessMessage = "Expense rejected successfully";
            await OnGetAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense {ExpenseId}", id);
            ErrorMessage = $"Error rejecting expense: {ex.Message}";
            await OnGetAsync();
            return Page();
        }
    }
}
