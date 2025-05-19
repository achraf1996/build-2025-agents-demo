// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;
using System.Text;
using System.Text.Json;

namespace Steps;

/// <summary>
/// Composes an email reply based on a list of answered questions using the Reply Agent.
/// </summary>
public sealed class ReplyAgent(PersistentAgentsClient client) : KernelProcessStep<BaseEmailWorkflowStepState>
{
    private BaseEmailWorkflowStepState _state;

    /// <summary>
    /// Binds the current process state to the step.
    /// </summary>
    public override ValueTask ActivateAsync(KernelProcessStepState<BaseEmailWorkflowStepState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Generates a final email response using the Reply Agent based on previously answered questions.
    /// </summary>
    [KernelFunction("execute")]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> answeredQuestions)
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        var replyPrinter = TreePrinter.CreateSubtree("Reply Agent", ConsoleColor.Green);

        var replyAgentId = Environment.GetEnvironmentVariable("REPLY_AGENT_ID");
        if (string.IsNullOrEmpty(replyAgentId))
            throw new InvalidOperationException("REPLY_AGENT_ID environment variable must be set.");

        var agent = new AzureAIAgent(await client.Administration.GetAgentAsync(replyAgentId), client);
        var thread = new AzureAIAgentThread(client, _state.Threads.MainThreadId);

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Please compose a response email using the following answers:");

        foreach (var qa in answeredQuestions)
        {
            messageBuilder.AppendLine($"Q: {qa.Question}");
            messageBuilder.AppendLine($"A: {qa.Answer}");
            messageBuilder.AppendLine();
        }

        var replyOutputPrinter = replyPrinter.CreateSubtree("Output: ", ConsoleColor.White);

        var fullMessage = messageBuilder.ToString();
        var responseBuilder = new StringBuilder();

        await foreach (var response in agent.InvokeStreamingAsync(fullMessage, thread))
        {
            if (!string.IsNullOrWhiteSpace(response.Message.Content))
            {
                responseBuilder.Append(response.Message.Content);
                replyOutputPrinter.Append(response.Message.Content, ConsoleColor.DarkGray);
            }
        }

        return responseBuilder.ToString();
    }
}
