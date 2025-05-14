using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;

#nullable enable

public interface IConversationReferenceStore
{
    Task SaveAsync(string key, ConversationReference reference);
    Task<ConversationReference?> GetAsync(string key);
    Task<ConversationReference?> GetDefaultAsync();
}
