using Microsoft.Agents.Hosting.AspNetCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;
using System;
using System.Collections.Generic;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.Agents.Builder;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class SendUserMessageService
{
    private readonly CloudAdapter _adapter;
    private readonly IConversationStateStore _store;
    private readonly PersistentAgentsClient _agentsClient;
    private AzureAIAgent? _cachedAgent;

    public SendUserMessageService(
        CloudAdapter adapter,
        IConversationStateStore store,
        PersistentAgentsClient agentsClient)
    {
        _adapter = adapter;
        _store = store;
        _agentsClient = agentsClient;
    }

    private async Task<AzureAIAgent> GetAgentAsync()
    {
        if (_cachedAgent != null) return _cachedAgent;

        var definition = await _agentsClient.Administration.GetAgentAsync("asst_kO43b9VVCywAVLTq2GBizsGy");
        _cachedAgent = new AzureAIAgent(definition, _agentsClient);
        return _cachedAgent;
    }

    private async Task<string> GetOrCreateThreadIdAsync(AzureAIAgentConversationState state)
    {
        if (!string.IsNullOrEmpty(state.ThreadId)) return state.ThreadId;

        var thread = await _agentsClient.Threads.CreateThreadAsync();
        state.ThreadId = thread.Value.Id;
        return state.ThreadId;
    }

    public async Task HandleUserMessageAsync(
        string conversationId,
        string messageText,
        ITurnContext turnContext,
        CancellationToken cancellationToken)
    {
        var state = await _store.GetAsync(conversationId);
        var agent = await GetAgentAsync();
        var threadId = await GetOrCreateThreadIdAsync(state);

        try
        {
            await turnContext.StreamingResponse.QueueInformativeUpdateAsync("Thinking", cancellationToken);

            var thread = new AzureAIAgentThread(_agentsClient, threadId);
            var message = new ChatMessageContent(AuthorRole.User, messageText);

            await foreach (var response in agent.InvokeStreamingAsync(message, thread))
            {
                turnContext.StreamingResponse.QueueTextChunk(response.Message.Content);
            }
        }
        finally
        {
            await turnContext.StreamingResponse.EndStreamAsync(cancellationToken);

            var reference = turnContext.Activity.GetConversationReference();
            await _store.SaveAsync(reference.Conversation.Id, new AzureAIAgentConversationState
            {
                ConversationReference = reference,
                ThreadId = threadId
            });
        }
    }

    public async Task AskUserToAnswerQuestionsAsync(List<UnansweredQuestions> unansweredQuestions, CancellationToken cancellationToken = default)
    {
        var state = await _store.GetDefaultAsync(); ;

        if (state == null)
        {
            Console.WriteLine("No conversation reference found for user.");
            return;
        }

        state.UnansweredQuestions.AddRange(unansweredQuestions);

        await _store.SaveAsync(state.ConversationReference.Conversation.Id, state);

        await _adapter.ContinueConversationAsync(
            agentAppId: "de8fc81a-145b-472a-bed5-f6348924c7db",
            reference: state.ConversationReference,
            callback: async (turnContext, ct) =>
            {
                var results = await turnContext.SendActivityAsync(
                    new Activity
                    {
                        Type = ActivityTypes.Message,
                        Conversation = state.ConversationReference.Conversation,
                        Name = "customEvent",
                        Text = "This is a test"
                    }, ct);

                Console.WriteLine($"Sent event: {results.Id}");
            },
            cancellationToken);
    }

    // move agent generation to here
    
}
