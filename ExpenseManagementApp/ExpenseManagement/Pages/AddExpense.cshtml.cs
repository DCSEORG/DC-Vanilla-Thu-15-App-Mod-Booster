using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Pages;

public class AddExpenseModel : PageModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<AddExpenseModel> _logger;

    public AddExpenseModel(IDatabaseService databaseService, ILogger<AddExpenseModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public List<User> Users { get; set; } = new();
    public List<ExpenseCategory> Categories { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty]
    [Required]
    public int UserId { get; set; }

    [BindProperty]
    [Required]
    public int CategoryId { get; set; }

    [BindProperty]
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [BindProperty]
    [Required]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [BindProperty]
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        try
        {
            Users = await _databaseService.GetUsersAsync();
            Categories = await _databaseService.GetCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for add expense");
            ErrorMessage = $"Error loading form data: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Users = await _databaseService.GetUsersAsync();
            Categories = await _databaseService.GetCategoriesAsync();
            return Page();
        }

        try
        {
            int amountMinor = (int)(Amount * 100);
            var statuses = await _databaseService.GetStatusesAsync();
            var draftStatus = statuses.FirstOrDefault(s => s.StatusName == "Draft");

            if (draftStatus == null)
            {
                ErrorMessage = "Could not find Draft status";
                return Page();
            }

            await _databaseService.CreateExpenseAsync(
                UserId,
                CategoryId,
                draftStatus.StatusId,
                amountMinor,
                "GBP",
                ExpenseDate,
                Description,
                null);

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            ErrorMessage = $"Error creating expense: {ex.Message}";
            Users = await _databaseService.GetUsersAsync();
            Categories = await _databaseService.GetCategoriesAsync();
            return Page();
        }
    }
}
