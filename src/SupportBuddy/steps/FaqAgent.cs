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

namespace Steps;

/// <summary>
/// Executes FAQ response generation for a list of triaged questions.
/// </summary>
public sealed class FaqAgent(PersistentAgentsClient client) : KernelProcessStep<BaseEmailWorkflowStepState>
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
    /// Generates FAQ answers for a list of questions using the FAQ agent.
    /// </summary>
    [KernelFunction("execute")]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> questions)
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        TreePrinter.Print("FAQ Agent", ConsoleColor.Cyan);
        TreePrinter.Indent();

        var faqAgentId = Environment.GetEnvironmentVariable("FAQ_AGENT_ID");
        if (string.IsNullOrEmpty(faqAgentId))
            throw new InvalidOperationException("FAQ_AGENT_ID environment variable must be set.");

        var agent = new AzureAIAgent(await client.Administration.GetAgentAsync(faqAgentId), client);
        var thread = new AzureAIAgentThread(client, _state.Threads.FaqThreadId);

        TreePrinter.Print("Output: ", ConsoleColor.White);
        TreePrinter.Indent();

        var messageBuilder = new StringBuilder();
        foreach (var qa in questions)
        {
            messageBuilder.AppendLine($"Q: {qa.Question}");
        }

        var fullMessage = messageBuilder.ToString();

        var responseBuilder = new StringBuilder();
        await foreach (var response in agent.InvokeStreamingAsync(fullMessage, thread))
        {
            if (!string.IsNullOrWhiteSpace(response.Message.Content))
            {
                responseBuilder.Append(response.Message.Content);
                TreePrinter.Append(response.Message.Content, ConsoleColor.DarkGray);
            }
        }

        TreePrinter.Unindent(); // End of output
        TreePrinter.Unindent(); // End of agent block

        return responseBuilder.ToString();
    }
}
