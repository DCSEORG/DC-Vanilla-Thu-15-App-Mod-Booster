using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.Chat;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Pages;

public class ChatModel : PageModel
{
    private readonly ChatService _chatService;
    private readonly ILogger<ChatModel> _logger;

    public ChatModel(ChatService chatService, ILogger<ChatModel> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public List<ChatMessageModel> Messages { get; set; } = new();

    [BindProperty]
    [Required]
    public string UserMessage { get; set; } = string.Empty;

    public void OnGet()
    {
        LoadMessages();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadMessages();
            return Page();
        }

        try
        {
            LoadMessages();
            
            Messages.Add(new ChatMessageModel { Content = UserMessage, IsUser = true });

            var conversationHistory = Messages
                .Where(m => !string.IsNullOrEmpty(m.Content))
                .Select(m => m.IsUser 
                    ? (ChatMessage)new UserChatMessage(m.Content) 
                    : (ChatMessage)new AssistantChatMessage(m.Content))
                .ToList();

            var response = await _chatService.ChatAsync(UserMessage, conversationHistory);
            
            Messages.Add(new ChatMessageModel { Content = response, IsUser = false });

            SaveMessages();
            
            UserMessage = string.Empty;
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            Messages.Add(new ChatMessageModel 
            { 
                Content = "I'm sorry, I encountered an error processing your request. Please try again.", 
                IsUser = false 
            });
            SaveMessages();
            return Page();
        }
    }

    private void LoadMessages()
    {
        var messagesJson = HttpContext.Session.GetString("ChatMessages");
        if (!string.IsNullOrEmpty(messagesJson))
        {
            Messages = System.Text.Json.JsonSerializer.Deserialize<List<ChatMessageModel>>(messagesJson) ?? new();
        }
    }

    private void SaveMessages()
    {
        var messagesJson = System.Text.Json.JsonSerializer.Serialize(Messages);
        HttpContext.Session.SetString("ChatMessages", messagesJson);
    }
}

public class ChatMessageModel
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
}
