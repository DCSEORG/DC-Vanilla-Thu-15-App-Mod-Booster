using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IDatabaseService databaseService, ILogger<ExpensesController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetExpenses()
    {
        try
        {
            var expenses = await _databaseService.GetExpensesAsync();
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses");
            return StatusCode(500, "Error retrieving expenses");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        try
        {
            var expense = await _databaseService.GetExpenseByIdAsync(id);
            if (expense == null)
                return NotFound();

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense {ExpenseId}", id);
            return StatusCode(500, "Error retrieving expense");
        }
    }

    [HttpGet("status/{statusName}")]
    public async Task<ActionResult<List<Expense>>> GetExpensesByStatus(string statusName)
    {
        try
        {
            var expenses = await _databaseService.GetExpensesByStatusAsync(statusName);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses by status {StatusName}", statusName);
            return StatusCode(500, "Error retrieving expenses");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Expense>>> GetExpensesByUser(int userId)
    {
        try
        {
            var expenses = await _databaseService.GetExpensesByUserIdAsync(userId);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses for user {UserId}", userId);
            return StatusCode(500, "Error retrieving expenses");
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<Expense>>> GetPendingExpenses()
    {
        try
        {
            var expenses = await _databaseService.GetPendingExpensesAsync();
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending expenses");
            return StatusCode(500, "Error retrieving pending expenses");
        }
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<Expense>>> FilterExpenses(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            var expenses = await _databaseService.FilterExpensesAsync(status, category, userId, dateFrom, dateTo);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering expenses");
            return StatusCode(500, "Error filtering expenses");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        try
        {
            var expenseId = await _databaseService.CreateExpenseAsync(
                request.UserId,
                request.CategoryId,
                request.StatusId,
                request.AmountMinor,
                request.Currency,
                request.ExpenseDate,
                request.Description,
                request.ReceiptFile);

            return CreatedAtAction(nameof(GetExpense), new { id = expenseId }, expenseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, "Error creating expense");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        try
        {
            var rowsAffected = await _databaseService.UpdateExpenseAsync(
                id,
                request.CategoryId,
                request.StatusId,
                request.AmountMinor,
                request.ExpenseDate,
                request.Description);

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", id);
            return StatusCode(500, "Error updating expense");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExpense(int id)
    {
        try
        {
            var rowsAffected = await _databaseService.DeleteExpenseAsync(id);
            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", id);
            return StatusCode(500, "Error deleting expense");
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult> SubmitExpense(int id)
    {
        try
        {
            var rowsAffected = await _databaseService.SubmitExpenseAsync(id);
            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense {ExpenseId}", id);
            return StatusCode(500, "Error submitting expense");
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveExpense(int id, [FromBody] ReviewRequest request)
    {
        try
        {
            var rowsAffected = await _databaseService.ApproveExpenseAsync(id, request.ReviewedBy);
            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense {ExpenseId}", id);
            return StatusCode(500, "Error approving expense");
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectExpense(int id, [FromBody] ReviewRequest request)
    {
        try
        {
            var rowsAffected = await _databaseService.RejectExpenseAsync(id, request.ReviewedBy);
            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense {ExpenseId}", id);
            return StatusCode(500, "Error rejecting expense");
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<ExpenseCategory>>> GetCategories()
    {
        try
        {
            var categories = await _databaseService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "Error retrieving categories");
        }
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<List<ExpenseStatus>>> GetStatuses()
    {
        try
        {
            var statuses = await _databaseService.GetStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statuses");
            return StatusCode(500, "Error retrieving statuses");
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        try
        {
            var users = await _databaseService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Error retrieving users");
        }
    }
}

public record CreateExpenseRequest(
    int UserId,
    int CategoryId,
    int StatusId,
    int AmountMinor,
    string Currency,
    DateTime ExpenseDate,
    string Description,
    string? ReceiptFile);

public record UpdateExpenseRequest(
    int CategoryId,
    int StatusId,
    int AmountMinor,
    DateTime ExpenseDate,
    string Description);

public record ReviewRequest(int ReviewedBy);
