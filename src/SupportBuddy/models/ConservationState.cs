using System.Collections.Generic;
using Microsoft.Agents.Core.Models;

#nullable enable

public class AzureAIAgentConversationState
{
    public ConversationReference? ConversationReference { get; set; }
    public string? ThreadId { get; set; }
    public List<QuestionAnswer> QuestionAnswer { get; set; } = [];
}