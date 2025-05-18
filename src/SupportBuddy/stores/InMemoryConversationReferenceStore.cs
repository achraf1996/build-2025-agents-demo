using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;

#nullable enable

public class InMemoryConversationStateStore : IConversationStateStore
{
    private readonly Dictionary<string, AzureAIAgentConversationState> _store = new();

    public Task SaveAsync(string key, AzureAIAgentConversationState reference)
    {
        _store[key] = reference;
        return Task.CompletedTask;
    }

    public Task<AzureAIAgentConversationState> GetAsync(string key)
    {
        _store.TryGetValue(key, out var reference);

        if (reference == null)
        {
            return Task.FromResult(
                new AzureAIAgentConversationState()
            );
        }

        return Task.FromResult(reference);
    }

    public Task<AzureAIAgentConversationState> GetDefaultAsync()
    {
        var value = _store.Values.LastOrDefault();
        if (value == null)
        {
            return Task.FromResult(
                new AzureAIAgentConversationState()
            );
        }

        return Task.FromResult(value);
    }

    public Task AnswerQuestion(string key, string emailId, string questionId, string answer)
    {
        if (_store.TryGetValue(key, out var reference))
        {
            var questionAnswer = reference.QuestionAnswer.FirstOrDefault(x => x.QuestionId == questionId && x.EmailId == emailId);
            if (questionAnswer != null)
            {
                questionAnswer.Answer = answer;
            }
        }

        return Task.CompletedTask;
    }
}
