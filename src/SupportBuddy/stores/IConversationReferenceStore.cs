using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;

#nullable enable

public interface IConversationStateStore
{
    Task SaveAsync(string key, AzureAIAgentConversationState reference);
    Task<AzureAIAgentConversationState> GetAsync(string key);
    Task<AzureAIAgentConversationState> GetDefaultAsync();

    Task AnswerQuestion(string key, string emailId, string questionId, string answer);
}
