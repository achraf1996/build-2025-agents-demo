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
using System.Text.Json;
using System.Linq;
using DotNetEnv;
using System.IO;

#nullable enable

public class SendUserMessageService
{
    private readonly CloudAdapter _adapter;
    private readonly IConversationStateStore _store;
    private readonly PersistentAgentsClient _agentsClient;
    private AzureAIAgent? _cachedAnswerSavingAgent;
    private AzureAIAgent? _cachedRequestAnswerAgent;
    private readonly Lazy<RespondToEmailWorkflowService> _respondToEmailWorkflowService;

    public SendUserMessageService(
        CloudAdapter adapter,
        IConversationStateStore store,
        PersistentAgentsClient agentsClient,
        Lazy<RespondToEmailWorkflowService> respondToEmailWorkflowService)
    {
        _adapter = adapter;
        _store = store;
        _agentsClient = agentsClient;
        _respondToEmailWorkflowService = respondToEmailWorkflowService;
    }

    private async Task<AzureAIAgent> GetRequestAnswerAgentAsync()
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        if (_cachedRequestAnswerAgent != null) return _cachedRequestAnswerAgent;

        var definition = await _agentsClient.Administration.GetAgentAsync(Environment.GetEnvironmentVariable("BUDDY_AGENT_ID"));
        _cachedRequestAnswerAgent = new AzureAIAgent(definition, _agentsClient);

        return _cachedRequestAnswerAgent;
    }

    private async Task<AzureAIAgent> GetAnswerSavingAgentAsync(string conversationId)
    {
        // if (_cachedAnswerSavingAgent != null) return _cachedAnswerSavingAgent;

        var definition = await _agentsClient.Administration.GetAgentAsync(Environment.GetEnvironmentVariable("BUDDY_AGENT_ID"));
        _cachedAnswerSavingAgent = new AzureAIAgent(definition, _agentsClient);

        _cachedAnswerSavingAgent.Kernel.Plugins.AddFromFunctions("Customer", [
            KernelFunctionFactory.CreateFromMethod((string emailId, string questionId, string answer) =>
            {
                _store.AnswerQuestion(conversationId, emailId, questionId, answer);
            }, "SaveAnswer", "After a user has answered a question from a customer, you can use this function to save the answer to the customer. Continue asking the user for answers until all questions are answered.")
        ]);

        return _cachedAnswerSavingAgent;
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
        var agent = await GetAnswerSavingAgentAsync(conversationId);
        var threadId = await GetOrCreateThreadIdAsync(state);

        try
        {
            await turnContext.StreamingResponse.QueueInformativeUpdateAsync("Thinking", cancellationToken);

            var thread = new AzureAIAgentThread(_agentsClient, threadId);
            var message = new ChatMessageContent(AuthorRole.User, messageText);

            await foreach (var response in agent.InvokeStreamingAsync(message, thread, options: new AzureAIAgentInvokeOptions()
            {
                AdditionalInstructions = "If the user provided an answer, Use the Customer.SaveAnswer function to send the answer to any of the following question to the customer:\n" +
                    JsonSerializer.Serialize(state.QuestionAnswer),
            }))
            {
                turnContext.StreamingResponse.QueueTextChunk(response.Message.Content!);
            }
        }
        finally
        {
            await turnContext.StreamingResponse.EndStreamAsync(cancellationToken);

            // Check if the user answered any questions
            state = await _store.GetAsync(conversationId);

            // Get all questions that were answered
            var answeredQuestions = state.QuestionAnswer
                .Where(x => !string.IsNullOrEmpty(x.Answer))
                .ToList();
            if (answeredQuestions.Count > 0)
            {
                // group the questions by emailId
                var groupedQuestions = answeredQuestions
                    .GroupBy(x => x.EmailId)
                    .ToList();
                foreach (var group in groupedQuestions)
                {
                    await _respondToEmailWorkflowService.Value.ContinueWorkflow(group.Key, [.. group]);
                }
            }

            var reference = turnContext.Activity.GetConversationReference();
            state.ConversationReference = reference;
            state.ThreadId = threadId;

            // Remove all questions that were answered
            state.QuestionAnswer.RemoveAll(x => !string.IsNullOrEmpty(x.Answer));

            await _store.SaveAsync(reference.Conversation.Id, state);
        }
    }

    public async Task AskUserToAnswerQuestionsAsync(List<QuestionAnswer> QuestionAnswer, CancellationToken cancellationToken = default)
    {
        var state = await _store.GetDefaultAsync(); ;
        var agent = await GetRequestAnswerAgentAsync();
        var threadId = await GetOrCreateThreadIdAsync(state);

        state.QuestionAnswer.AddRange(QuestionAnswer);
        await _store.SaveAsync(state.ConversationReference!.Conversation.Id, state);

        var thread = new AzureAIAgentThread(_agentsClient, threadId);

        var response = agent.InvokeAsync(thread, options: new AzureAIAgentInvokeOptions()
        {
            AdditionalInstructions = "Ignore the previous conversation. Instead, ask the user to answer the following questions for the customer:\n" +
                JsonSerializer.Serialize(QuestionAnswer)
        });

        var message = "";
        await foreach (var item in response)
        {
            message += item.Message.Content;
        }


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
                        Text = message,
                    }, ct);
            },
            cancellationToken);


    }

}
