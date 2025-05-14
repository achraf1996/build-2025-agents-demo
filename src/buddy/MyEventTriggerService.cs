using Microsoft.Agents.Hosting.AspNetCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;
using System;

public class MyEventTriggerService
{
    private readonly CloudAdapter _adapter;
    private readonly IConversationReferenceStore _store;

    public MyEventTriggerService(CloudAdapter adapter, IConversationReferenceStore store)
    {
        _adapter = adapter;
        _store = store;
    }

    public async Task SendCustomEventAsync(CancellationToken cancellationToken)
    {
        var reference = await _store.GetDefaultAsync();;

        if (reference == null)
        {
            Console.WriteLine("No conversation reference found for user.");
            return;
        }

        await _adapter.ContinueConversationAsync(
            agentAppId: "de8fc81a-145b-472a-bed5-f6348924c7db",
            reference: reference,
            callback: async (turnContext, ct) =>
            {
                var results = await turnContext.SendActivityAsync(
                    new Activity
                    {
                        Type = ActivityTypes.Event,
                        Conversation = reference.Conversation,
                        Name = "customEvent",
                        Value = new { Message = "Hello from CloudAdapter!" }
                    }, ct);

                Console.WriteLine($"Sent event: {results.Id}");
            },
            cancellationToken);
    }
}
