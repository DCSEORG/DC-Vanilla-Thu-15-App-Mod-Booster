using ExpenseManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpenseManagement.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;
    private readonly bool _isConnected = false;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            _isConnected = true;
            _logger.LogInformation("Database connection successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database. Using dummy data mode.");
            _isConnected = false;
        }
    }

    public async Task<List<Expense>> GetExpensesAsync()
    {
        if (!_isConnected) return GetDummyExpenses();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetExpenses", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var expenses = new List<Expense>();

            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpense(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses");
            return GetDummyExpenses();
        }
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
    {
        if (!_isConnected) return GetDummyExpenses().FirstOrDefault(e => e.ExpenseId == expenseId);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetExpenseById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapExpense(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense by ID: {ExpenseId}", expenseId);
            return GetDummyExpenses().FirstOrDefault(e => e.ExpenseId == expenseId);
        }
    }

    public async Task<List<Expense>> GetExpensesByStatusAsync(string statusName)
    {
        if (!_isConnected) return GetDummyExpenses().Where(e => e.StatusName == statusName).ToList();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetExpensesByStatus", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@StatusName", statusName);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var expenses = new List<Expense>();

            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpense(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses by status: {StatusName}", statusName);
            return GetDummyExpenses().Where(e => e.StatusName == statusName).ToList();
        }
    }

    public async Task<List<Expense>> GetExpensesByUserIdAsync(int userId)
    {
        if (!_isConnected) return GetDummyExpenses().Where(e => e.UserId == userId).ToList();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetExpensesByUserId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var expenses = new List<Expense>();

            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpense(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses by user ID: {UserId}", userId);
            return GetDummyExpenses().Where(e => e.UserId == userId).ToList();
        }
    }

    public async Task<List<Expense>> GetPendingExpensesAsync()
    {
        if (!_isConnected) return GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetPendingExpenses", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var expenses = new List<Expense>();

            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpense(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending expenses");
            return GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();
        }
    }

    public async Task<List<Expense>> FilterExpensesAsync(string? statusName = null, string? categoryName = null, int? userId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        if (!_isConnected)
        {
            var filtered = GetDummyExpenses().AsEnumerable();
            if (!string.IsNullOrEmpty(statusName)) filtered = filtered.Where(e => e.StatusName == statusName);
            if (!string.IsNullOrEmpty(categoryName)) filtered = filtered.Where(e => e.CategoryName == categoryName);
            if (userId.HasValue) filtered = filtered.Where(e => e.UserId == userId.Value);
            if (dateFrom.HasValue) filtered = filtered.Where(e => e.ExpenseDate >= dateFrom.Value);
            if (dateTo.HasValue) filtered = filtered.Where(e => e.ExpenseDate <= dateTo.Value);
            return filtered.ToList();
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_FilterExpenses", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@StatusName", (object?)statusName ?? DBNull.Value);
            command.Parameters.AddWithValue("@CategoryName", (object?)categoryName ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DateFrom", (object?)dateFrom ?? DBNull.Value);
            command.Parameters.AddWithValue("@DateTo", (object?)dateTo ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var expenses = new List<Expense>();

            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpense(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering expenses");
            return GetDummyExpenses();
        }
    }

    public async Task<int> CreateExpenseAsync(int userId, int categoryId, int statusId, int amountMinor, string currency, DateTime expenseDate, string description, string? receiptFile)
    {
        if (!_isConnected) return new Random().Next(1000, 9999);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_CreateExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            command.Parameters.AddWithValue("@StatusId", statusId);
            command.Parameters.AddWithValue("@AmountMinor", amountMinor);
            command.Parameters.AddWithValue("@Currency", currency);
            command.Parameters.AddWithValue("@ExpenseDate", expenseDate);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@ReceiptFile", (object?)receiptFile ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            throw;
        }
    }

    public async Task<int> UpdateExpenseAsync(int expenseId, int categoryId, int statusId, int amountMinor, DateTime expenseDate, string description)
    {
        if (!_isConnected) return 1;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_UpdateExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            command.Parameters.AddWithValue("@StatusId", statusId);
            command.Parameters.AddWithValue("@AmountMinor", amountMinor);
            command.Parameters.AddWithValue("@ExpenseDate", expenseDate);
            command.Parameters.AddWithValue("@Description", description);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense");
            throw;
        }
    }

    public async Task<int> DeleteExpenseAsync(int expenseId)
    {
        if (!_isConnected) return 1;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_DeleteExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense");
            throw;
        }
    }

    public async Task<int> SubmitExpenseAsync(int expenseId)
    {
        if (!_isConnected) return 1;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_SubmitExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense");
            throw;
        }
    }

    public async Task<int> ApproveExpenseAsync(int expenseId, int reviewedBy)
    {
        if (!_isConnected) return 1;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_ApproveExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@ReviewedBy", reviewedBy);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense");
            throw;
        }
    }

    public async Task<int> RejectExpenseAsync(int expenseId, int reviewedBy)
    {
        if (!_isConnected) return 1;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_RejectExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@ReviewedBy", reviewedBy);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense");
            throw;
        }
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        if (!_isConnected) return GetDummyCategories();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetCategories", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var categories = new List<ExpenseCategory>();

            while (await reader.ReadAsync())
            {
                categories.Add(new ExpenseCategory
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return GetDummyCategories();
        }
    }

    public async Task<List<ExpenseStatus>> GetStatusesAsync()
    {
        if (!_isConnected) return GetDummyStatuses();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetStatuses", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var statuses = new List<ExpenseStatus>();

            while (await reader.ReadAsync())
            {
                statuses.Add(new ExpenseStatus
                {
                    StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                    StatusName = reader.GetString(reader.GetOrdinal("StatusName"))
                });
            }

            return statuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statuses");
            return GetDummyStatuses();
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        if (!_isConnected) return GetDummyUsers();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.sp_GetUsers", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var users = new List<User>();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                    ManagerId = reader.IsDBNull(reader.GetOrdinal("ManagerId")) ? null : reader.GetInt32(reader.GetOrdinal("ManagerId")),
                    ManagerName = reader.IsDBNull(reader.GetOrdinal("ManagerName")) ? null : reader.GetString(reader.GetOrdinal("ManagerName")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return GetDummyUsers();
        }
    }

    private Expense MapExpense(SqlDataReader reader)
    {
        return new Expense
        {
            ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
            StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
            AmountMinor = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
            Currency = reader.GetString(reader.GetOrdinal("Currency")),
            ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            ReceiptFile = reader.IsDBNull(reader.GetOrdinal("ReceiptFile")) ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
            SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
            ReviewedBy = reader.IsDBNull(reader.GetOrdinal("ReviewedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ReviewedBy")),
            ReviewedByName = reader.IsDBNull(reader.GetOrdinal("ReviewedByName")) ? null : reader.GetString(reader.GetOrdinal("ReviewedByName")),
            ReviewedAt = reader.IsDBNull(reader.GetOrdinal("ReviewedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }

    private List<Expense> GetDummyExpenses()
    {
        return new List<Expense>
        {
            new Expense
            {
                ExpenseId = 1,
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                CategoryId = 1,
                CategoryName = "Travel",
                StatusId = 2,
                StatusName = "Submitted",
                AmountMinor = 2540,
                Currency = "GBP",
                ExpenseDate = new DateTime(2025, 10, 20),
                Description = "Taxi from airport to client site",
                ReceiptFile = "/receipts/alice/taxi_oct20.jpg",
                SubmittedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Expense
            {
                ExpenseId = 2,
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                CategoryId = 2,
                CategoryName = "Meals",
                StatusId = 3,
                StatusName = "Approved",
                AmountMinor = 1425,
                Currency = "GBP",
                ExpenseDate = new DateTime(2025, 9, 15),
                Description = "Client lunch meeting",
                ReceiptFile = "/receipts/alice/lunch_sep15.jpg",
                SubmittedAt = DateTime.UtcNow.AddDays(-30),
                ReviewedBy = 2,
                ReviewedByName = "Bob Manager",
                ReviewedAt = DateTime.UtcNow.AddDays(-29),
                CreatedAt = DateTime.UtcNow.AddDays(-31)
            },
            new Expense
            {
                ExpenseId = 3,
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                CategoryId = 3,
                CategoryName = "Supplies",
                StatusId = 1,
                StatusName = "Draft",
                AmountMinor = 799,
                Currency = "GBP",
                ExpenseDate = new DateTime(2025, 11, 1),
                Description = "Office stationery",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Expense
            {
                ExpenseId = 4,
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                CategoryId = 4,
                CategoryName = "Accommodation",
                StatusId = 3,
                StatusName = "Approved",
                AmountMinor = 12300,
                Currency = "GBP",
                ExpenseDate = new DateTime(2025, 8, 10),
                Description = "Hotel during client visit",
                ReceiptFile = "/receipts/alice/hotel_aug10.jpg",
                SubmittedAt = DateTime.UtcNow.AddDays(-60),
                ReviewedBy = 2,
                ReviewedByName = "Bob Manager",
                ReviewedAt = DateTime.UtcNow.AddDays(-59),
                CreatedAt = DateTime.UtcNow.AddDays(-61)
            }
        };
    }

    private List<ExpenseCategory> GetDummyCategories()
    {
        return new List<ExpenseCategory>
        {
            new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
            new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
            new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
            new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
            new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
        };
    }

    private List<ExpenseStatus> GetDummyStatuses()
    {
        return new List<ExpenseStatus>
        {
            new ExpenseStatus { StatusId = 1, StatusName = "Draft" },
            new ExpenseStatus { StatusId = 2, StatusName = "Submitted" },
            new ExpenseStatus { StatusId = 3, StatusName = "Approved" },
            new ExpenseStatus { StatusId = 4, StatusName = "Rejected" }
        };
    }

    private List<User> GetDummyUsers()
    {
        return new List<User>
        {
            new User
            {
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                RoleId = 1,
                RoleName = "Employee",
                ManagerId = 2,
                ManagerName = "Bob Manager",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            },
            new User
            {
                UserId = 2,
                UserName = "Bob Manager",
                Email = "bob.manager@example.co.uk",
                RoleId = 2,
                RoleName = "Manager",
                ManagerId = null,
                ManagerName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-200)
            }
        };
    }
}
