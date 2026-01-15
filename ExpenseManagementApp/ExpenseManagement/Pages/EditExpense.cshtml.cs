using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Pages;

public class EditExpenseModel : PageModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<EditExpenseModel> _logger;

    public EditExpenseModel(IDatabaseService databaseService, ILogger<EditExpenseModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public Expense? Expense { get; set; }
    public List<ExpenseCategory> Categories { get; set; } = new();
    public List<ExpenseStatus> Statuses { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int ExpenseId { get; set; }

    [BindProperty]
    [Required]
    public int CategoryId { get; set; }

    [BindProperty]
    [Required]
    public int StatusId { get; set; }

    [BindProperty]
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [BindProperty]
    [Required]
    public DateTime ExpenseDate { get; set; }

    [BindProperty]
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Expense = await _databaseService.GetExpenseByIdAsync(ExpenseId);
            
            if (Expense == null)
            {
                return NotFound();
            }

            Categories = await _databaseService.GetCategoriesAsync();
            Statuses = await _databaseService.GetStatusesAsync();

            CategoryId = Expense.CategoryId;
            StatusId = Expense.StatusId;
            Amount = Expense.AmountInPounds;
            ExpenseDate = Expense.ExpenseDate;
            Description = Expense.Description ?? string.Empty;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expense {ExpenseId}", ExpenseId);
            ErrorMessage = $"Error loading expense: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Expense = await _databaseService.GetExpenseByIdAsync(ExpenseId);
            Categories = await _databaseService.GetCategoriesAsync();
            Statuses = await _databaseService.GetStatusesAsync();
            return Page();
        }

        try
        {
            int amountMinor = (int)(Amount * 100);

            await _databaseService.UpdateExpenseAsync(
                ExpenseId,
                CategoryId,
                StatusId,
                amountMinor,
                ExpenseDate,
                Description);

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", ExpenseId);
            ErrorMessage = $"Error updating expense: {ex.Message}";
            Expense = await _databaseService.GetExpenseByIdAsync(ExpenseId);
            Categories = await _databaseService.GetCategoriesAsync();
            Statuses = await _databaseService.GetStatusesAsync();
            return Page();
        }
    }
}
