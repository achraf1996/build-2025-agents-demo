using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;

#nullable enable

public class InMemoryConversationReferenceStore : IConversationReferenceStore
{
    private readonly Dictionary<string, ConversationReference> _store = new();

    public Task SaveAsync(string key, ConversationReference reference)
    {
        _store[key] = reference;
        return Task.CompletedTask;
    }

    public Task<ConversationReference?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var reference);
        return Task.FromResult(reference);
    }

    public Task<ConversationReference?> GetDefaultAsync()
    {
        return Task.FromResult(_store.Values.LastOrDefault());
    }
}
