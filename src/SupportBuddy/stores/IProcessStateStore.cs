using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;
using Microsoft.SemanticKernel;

#nullable enable

public interface IProcessStateStore
{
    Task SaveAsync(string key, KernelProcess processState);
    Task<KernelProcess?> GetAsync(string key);
    Task<KernelProcess?> GetDefaultAsync();
}
