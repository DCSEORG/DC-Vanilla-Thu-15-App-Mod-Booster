using ExpenseManagement.Models;

namespace ExpenseManagement.Services;

public interface IDatabaseService
{
    Task<List<Expense>> GetExpensesAsync();
    Task<Expense?> GetExpenseByIdAsync(int expenseId);
    Task<List<Expense>> GetExpensesByStatusAsync(string statusName);
    Task<List<Expense>> GetExpensesByUserIdAsync(int userId);
    Task<List<Expense>> GetPendingExpensesAsync();
    Task<List<Expense>> FilterExpensesAsync(string? statusName = null, string? categoryName = null, int? userId = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<int> CreateExpenseAsync(int userId, int categoryId, int statusId, int amountMinor, string currency, DateTime expenseDate, string description, string? receiptFile);
    Task<int> UpdateExpenseAsync(int expenseId, int categoryId, int statusId, int amountMinor, DateTime expenseDate, string description);
    Task<int> DeleteExpenseAsync(int expenseId);
    Task<int> SubmitExpenseAsync(int expenseId);
    Task<int> ApproveExpenseAsync(int expenseId, int reviewedBy);
    Task<int> RejectExpenseAsync(int expenseId, int reviewedBy);
    Task<List<ExpenseCategory>> GetCategoriesAsync();
    Task<List<ExpenseStatus>> GetStatusesAsync();
    Task<List<User>> GetUsersAsync();
}
