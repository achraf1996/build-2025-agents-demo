using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;
using Microsoft.SemanticKernel;

#nullable enable

public class ProcessStateStore : IProcessStateStore
{
    private readonly Dictionary<string, KernelProcess> _store = [];

    public Task SaveAsync(string key, KernelProcess reference)
    {
        _store[key] = reference;
        return Task.CompletedTask;
    }

    public Task<KernelProcess?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var reference);
        return Task.FromResult(reference);
    }

    public Task<KernelProcess?> GetDefaultAsync()
    {
        return Task.FromResult(_store.Values.LastOrDefault());
    }
}
