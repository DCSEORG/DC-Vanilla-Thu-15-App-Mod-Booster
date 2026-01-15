-- Stored Procedures for Expense Management System
-- All application data access goes through these stored procedures

-- =============================================
-- Get All Expenses with full details
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetExpenses
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        reviewer.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users reviewer ON e.ReviewedBy = reviewer.UserId
    ORDER BY e.CreatedAt DESC;
END
GO

-- =============================================
-- Get Expense by ID
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetExpenseById
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        reviewer.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users reviewer ON e.ReviewedBy = reviewer.UserId
    WHERE e.ExpenseId = @ExpenseId;
END
GO

-- =============================================
-- Get Expenses by Status
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetExpensesByStatus
    @StatusName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        reviewer.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users reviewer ON e.ReviewedBy = reviewer.UserId
    WHERE s.StatusName = @StatusName
    ORDER BY e.CreatedAt DESC;
END
GO

-- =============================================
-- Get Expenses by User ID
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetExpensesByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        reviewer.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users reviewer ON e.ReviewedBy = reviewer.UserId
    WHERE e.UserId = @UserId
    ORDER BY e.CreatedAt DESC;
END
GO

-- =============================================
-- Create Expense
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_CreateExpense
    @UserId INT,
    @CategoryId INT,
    @StatusId INT,
    @AmountMinor INT,
    @Currency NVARCHAR(3),
    @ExpenseDate DATE,
    @Description NVARCHAR(1000),
    @ReceiptFile NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.Expenses (
        UserId, 
        CategoryId, 
        StatusId, 
        AmountMinor, 
        Currency, 
        ExpenseDate, 
        Description, 
        ReceiptFile,
        CreatedAt
    )
    VALUES (
        @UserId,
        @CategoryId,
        @StatusId,
        @AmountMinor,
        @Currency,
        @ExpenseDate,
        @Description,
        @ReceiptFile,
        SYSUTCDATETIME()
    );
    
    SELECT SCOPE_IDENTITY() AS ExpenseId;
END
GO

-- =============================================
-- Update Expense
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_UpdateExpense
    @ExpenseId INT,
    @CategoryId INT,
    @StatusId INT,
    @AmountMinor INT,
    @ExpenseDate DATE,
    @Description NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Expenses
    SET 
        CategoryId = @CategoryId,
        StatusId = @StatusId,
        AmountMinor = @AmountMinor,
        ExpenseDate = @ExpenseDate,
        Description = @Description
    WHERE ExpenseId = @ExpenseId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Delete Expense
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_DeleteExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM dbo.Expenses
    WHERE ExpenseId = @ExpenseId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Submit Expense (change status to Submitted)
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_SubmitExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Expenses
    SET 
        StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted'),
        SubmittedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Approve Expense
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_ApproveExpense
    @ExpenseId INT,
    @ReviewedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Expenses
    SET 
        StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved'),
        ReviewedBy = @ReviewedBy,
        ReviewedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Reject Expense
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_RejectExpense
    @ExpenseId INT,
    @ReviewedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Expenses
    SET 
        StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Rejected'),
        ReviewedBy = @ReviewedBy,
        ReviewedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Get All Categories
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetCategories
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CategoryId,
        CategoryName,
        IsActive
    FROM dbo.ExpenseCategories
    WHERE IsActive = 1
    ORDER BY CategoryName;
END
GO

-- =============================================
-- Get All Statuses
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetStatuses
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        StatusId,
        StatusName
    FROM dbo.ExpenseStatus
    ORDER BY StatusId;
END
GO

-- =============================================
-- Get All Users
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.UserName,
        u.Email,
        u.RoleId,
        r.RoleName,
        u.ManagerId,
        manager.UserName AS ManagerName,
        u.IsActive,
        u.CreatedAt
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users manager ON u.ManagerId = manager.UserId
    WHERE u.IsActive = 1
    ORDER BY u.UserName;
END
GO

-- =============================================
-- Get Pending Expenses for Managers
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetPendingExpenses
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    WHERE s.StatusName = 'Submitted'
    ORDER BY e.SubmittedAt ASC;
END
GO

-- =============================================
-- Filter Expenses (flexible search)
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_FilterExpenses
    @StatusName NVARCHAR(50) = NULL,
    @CategoryName NVARCHAR(100) = NULL,
    @UserId INT = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExpenseId,
        e.UserId,
        u.UserName,
        u.Email,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        reviewer.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    INNER JOIN dbo.Users u ON e.UserId = u.UserId
    INNER JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    INNER JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users reviewer ON e.ReviewedBy = reviewer.UserId
    WHERE 
        (@StatusName IS NULL OR s.StatusName = @StatusName)
        AND (@CategoryName IS NULL OR c.CategoryName = @CategoryName)
        AND (@UserId IS NULL OR e.UserId = @UserId)
        AND (@DateFrom IS NULL OR e.ExpenseDate >= @DateFrom)
        AND (@DateTo IS NULL OR e.ExpenseDate <= @DateTo)
    ORDER BY e.CreatedAt DESC;
END
GO
