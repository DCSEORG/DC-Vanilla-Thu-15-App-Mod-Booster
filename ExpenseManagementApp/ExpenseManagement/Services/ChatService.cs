using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using ExpenseManagement.Models;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace ExpenseManagement.Services;

public class ChatService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IConfiguration configuration, IDatabaseService databaseService, ILogger<ChatService> logger)
    {
        _logger = logger;
        _databaseService = databaseService;
        
        var endpoint = configuration["OpenAI:Endpoint"] ?? "";
        _deploymentName = configuration["OpenAI:DeploymentName"] ?? "gpt-4o";
        var clientId = configuration["ManagedIdentityClientId"];

        try
        {
            var credential = string.IsNullOrEmpty(clientId) 
                ? new DefaultAzureCredential() 
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId });
            
            _client = new AzureOpenAIClient(new Uri(endpoint), credential);
            _logger.LogInformation("Azure OpenAI client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure OpenAI client");
            throw;
        }
    }

    public async Task<string> ChatAsync(string userMessage, List<ChatMessage> conversationHistory)
    {
        try
        {
            var chatClient = _client.GetChatClient(_deploymentName);
            
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are a helpful assistant for an Expense Management System. You can help users with:
- Getting information about expenses
- Filtering expenses by status, category, user, or date range
- Creating new expenses
- Updating existing expenses
- Approving or rejecting expenses

When users ask about expenses, use the available functions to retrieve or modify data.
Always be concise and professional in your responses.")
            };

            messages.AddRange(conversationHistory);
            messages.Add(new UserChatMessage(userMessage));

            var tools = new List<ChatTool>
            {
                ChatTool.CreateFunctionTool(
                    functionName: "get_expenses",
                    functionDescription: "Get all expenses or filter them by criteria",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "status": { "type": "string", "description": "Filter by status: Draft, Submitted, Approved, Rejected" },
                            "category": { "type": "string", "description": "Filter by category name" },
                            "userId": { "type": "integer", "description": "Filter by user ID" }
                        }
                    }
                    """)
                ),
                ChatTool.CreateFunctionTool(
                    functionName: "create_expense",
                    functionDescription: "Create a new expense",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "required": ["userId", "categoryId", "amount", "expenseDate", "description"],
                        "properties": {
                            "userId": { "type": "integer", "description": "The user ID creating the expense" },
                            "categoryId": { "type": "integer", "description": "Category ID (1=Travel, 2=Meals, 3=Supplies, 4=Accommodation, 5=Other)" },
                            "amount": { "type": "number", "description": "Amount in pounds (e.g., 25.40)" },
                            "expenseDate": { "type": "string", "description": "Date of expense in YYYY-MM-DD format" },
                            "description": { "type": "string", "description": "Description of the expense" }
                        }
                    }
                    """)
                ),
                ChatTool.CreateFunctionTool(
                    functionName: "approve_expense",
                    functionDescription: "Approve a pending expense",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "required": ["expenseId", "reviewerId"],
                        "properties": {
                            "expenseId": { "type": "integer", "description": "The expense ID to approve" },
                            "reviewerId": { "type": "integer", "description": "The manager's user ID approving the expense" }
                        }
                    }
                    """)
                )
            };

            var options = new ChatCompletionOptions();
            foreach (var tool in tools)
            {
                options.Tools.Add(tool);
            }

            var response = await chatClient.CompleteChatAsync(messages, options);

            if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(response.Value));

                foreach (var toolCall in response.Value.ToolCalls)
                {
                    if (toolCall is ChatToolCall functionToolCall)
                    {
                        var functionResult = await ExecuteFunctionAsync(functionToolCall.FunctionName, functionToolCall.FunctionArguments.ToString());
                        messages.Add(new ToolChatMessage(functionToolCall.Id, functionResult));
                    }
                }

                var finalResponse = await chatClient.CompleteChatAsync(messages, options);
                return finalResponse.Value.Content[0].Text;
            }

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat processing");
            return "I'm sorry, I encountered an error processing your request. Please try again.";
        }
    }

    private async Task<string> ExecuteFunctionAsync(string functionName, string functionArguments)
    {
        try
        {
            _logger.LogInformation("Executing function: {FunctionName} with args: {Args}", functionName, functionArguments);

            switch (functionName)
            {
                case "get_expenses":
                    var getArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(functionArguments);
                    string? status = getArgs?.ContainsKey("status") == true ? getArgs["status"].GetString() : null;
                    string? category = getArgs?.ContainsKey("category") == true ? getArgs["category"].GetString() : null;
                    int? userId = getArgs?.ContainsKey("userId") == true ? getArgs["userId"].GetInt32() : null;

                    var expenses = await _databaseService.FilterExpensesAsync(status, category, userId);
                    return JsonSerializer.Serialize(expenses.Select(e => new
                    {
                        e.ExpenseId,
                        e.UserName,
                        e.CategoryName,
                        e.StatusName,
                        Amount = e.FormattedAmount,
                        e.ExpenseDate,
                        e.Description
                    }));

                case "create_expense":
                    var createArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(functionArguments);
                    if (createArgs == null) return "Invalid arguments";

                    int userId2 = createArgs["userId"].GetInt32();
                    int categoryId = createArgs["categoryId"].GetInt32();
                    decimal amount = createArgs["amount"].GetDecimal();
                    DateTime expenseDate = DateTime.Parse(createArgs["expenseDate"].GetString()!);
                    string description = createArgs["description"].GetString()!;

                    int amountMinor = (int)(amount * 100);
                    int expenseId = await _databaseService.CreateExpenseAsync(userId2, categoryId, 1, amountMinor, "GBP", expenseDate, description, null);
                    return JsonSerializer.Serialize(new { expenseId, message = "Expense created successfully" });

                case "approve_expense":
                    var approveArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(functionArguments);
                    if (approveArgs == null) return "Invalid arguments";

                    int expenseId2 = approveArgs["expenseId"].GetInt32();
                    int reviewerId = approveArgs["reviewerId"].GetInt32();

                    await _databaseService.ApproveExpenseAsync(expenseId2, reviewerId);
                    return JsonSerializer.Serialize(new { message = "Expense approved successfully" });

                default:
                    return "Unknown function";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function: {FunctionName}", functionName);
            return $"Error: {ex.Message}";
        }
    }
}
