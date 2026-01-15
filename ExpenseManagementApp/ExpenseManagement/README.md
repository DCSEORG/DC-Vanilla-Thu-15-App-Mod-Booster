# Expense Management System

A modern ASP.NET Core 8 Razor Pages application for managing employee expenses with Azure OpenAI integration.

## Features

- **Expense Management**: Create, view, edit, and delete expenses
- **Status Tracking**: Draft, Submitted, Approved, and Rejected statuses
- **Category Support**: Travel, Meals, Supplies, Accommodation, and Other
- **Manager Approval**: Dedicated page for managers to approve/reject expenses
- **Advanced Filtering**: Filter expenses by status, category, user, and date range
- **AI Chat Assistant**: Azure OpenAI-powered chat for expense queries and operations
- **REST API**: Full CRUD API with Swagger documentation
- **Modern UI**: Responsive design with blue/gray color scheme

## Architecture

### Models
- **Expense**: Main expense entity (AmountMinor stored as int in pence)
- **ExpenseCategory**: Expense categories
- **ExpenseStatus**: Status lookup (Draft, Submitted, Approved, Rejected)
- **User**: User entity with role support

### Services
- **IDatabaseService/DatabaseService**: Database operations via stored procedures
  - All database access through stored procedures in `stored-procedures.sql`
  - Automatic fallback to dummy data if database connection fails
  - Uses Microsoft.Data.SqlClient with managed identity authentication
  
- **ChatService**: Azure OpenAI integration with function calling
  - Get expenses with filtering
  - Create expenses
  - Approve/reject expenses

### Pages
- **Index**: Main expense list with filtering
- **AddExpense**: Create new expense form
- **EditExpense**: Edit existing expense
- **ApproveExpenses**: Manager approval interface
- **Chat**: AI chat assistant

### API Controllers
- **ExpensesController**: Full REST API at `/api/expenses`
  - GET /api/expenses - Get all expenses
  - GET /api/expenses/{id} - Get expense by ID
  - GET /api/expenses/status/{status} - Filter by status
  - GET /api/expenses/user/{userId} - Filter by user
  - GET /api/expenses/pending - Get pending expenses
  - GET /api/expenses/filter - Advanced filtering
  - POST /api/expenses - Create expense
  - PUT /api/expenses/{id} - Update expense
  - DELETE /api/expenses/{id} - Delete expense
  - POST /api/expenses/{id}/submit - Submit for approval
  - POST /api/expenses/{id}/approve - Approve expense
  - POST /api/expenses/{id}/reject - Reject expense
  - GET /api/expenses/categories - Get categories
  - GET /api/expenses/statuses - Get statuses
  - GET /api/expenses/users - Get users

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net;Database=ExpenseManagement;Authentication=Active Directory Managed Identity;User Id=your-client-id;"
  },
  "OpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o"
  },
  "ManagedIdentityClientId": "your-managed-identity-client-id"
}
```

### Database Connection
- Uses Azure Managed Identity for secure authentication
- Connection string format: `Server=tcp:server.database.windows.net;Database=db;Authentication=Active Directory Managed Identity;User Id=client-id;`
- Automatic fallback to dummy data if connection fails
- Error messages displayed in header

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- Azure SQL Database with schema from `Database-Schema/database_schema.sql`
- Stored procedures deployed from `stored-procedures.sql`
- Azure OpenAI resource (optional, for chat feature)
- Azure Managed Identity configured

### Development
```bash
cd ExpenseManagementApp/ExpenseManagement
dotnet run
```

Access:
- Application: https://localhost:7000
- Swagger UI: https://localhost:7000/swagger

### Production
Configure the following:
1. Azure SQL connection string with managed identity
2. Azure OpenAI endpoint and deployment name
3. Managed identity client ID

## Error Handling

- Database connection failures show error banner with details
- Returns dummy data if database unavailable (for development)
- All exceptions logged
- User-friendly error messages

## Security

- Managed Identity authentication (no connection strings with passwords)
- Input validation on all forms
- Parameterized SQL queries via stored procedures
- HTTPS enforcement in production
- Session-based chat history

## Styling

Modern design inspired by contemporary expense management applications:
- Primary blue (#4A90E2) and gray color scheme
- Card-based layout
- Responsive design
- Smooth animations and transitions
- Status badges with color coding
- Mobile-friendly interface

## Dependencies

- Microsoft.Data.SqlClient 6.1.3
- Azure.AI.OpenAI 2.1.0
- Azure.Identity 1.17.1
- Swashbuckle.AspNetCore 10.1.0

## Database Schema

All database access is through stored procedures:
- sp_GetExpenses
- sp_GetExpenseById
- sp_GetExpensesByStatus
- sp_GetExpensesByUserId
- sp_GetPendingExpenses
- sp_FilterExpenses
- sp_CreateExpense
- sp_UpdateExpense
- sp_DeleteExpense
- sp_SubmitExpense
- sp_ApproveExpense
- sp_RejectExpense
- sp_GetCategories
- sp_GetStatuses
- sp_GetUsers

See `stored-procedures.sql` for complete definitions.
