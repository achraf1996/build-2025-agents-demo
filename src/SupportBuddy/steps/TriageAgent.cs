// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace Steps;

/// <summary>
/// Runs triage on incoming emails using a streaming Azure AI agent and extracts structured questions.
/// </summary>
public sealed class TriageAgent(PersistentAgentsClient client) : KernelProcessStep<BaseEmailWorkflowStepState>
{
    private BaseEmailWorkflowStepState _state;

    /// <summary>
    /// Binds the state when the process step is activated.
    /// </summary>
    public override ValueTask ActivateAsync(KernelProcessStepState<BaseEmailWorkflowStepState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Executes triage on an incoming email using the TRIAGE_AGENT_ID-defined agent.
    /// It returns a list of extracted questions with unfilled answers.
    /// </summary>
    [KernelFunction("execute")]
    public async Task<List<QuestionAnswer>> ExecuteAsync(KernelProcessStepContext context, Email email)
    {
        // Load .env file (if present) to access environment variables
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        var triagePrinter = TreePrinter.CreateSubtree("Triage Agent", ConsoleColor.Cyan);

        var thread = new AzureAIAgentThread(client, _state.Threads.MainThreadId);

        // Ensure TRIAGE_AGENT_ID is defined
        var triageAgentId = Environment.GetEnvironmentVariable("TRIAGE_AGENT_ID");
        if (string.IsNullOrEmpty(triageAgentId))
            throw new InvalidOperationException("TRIAGE_AGENT_ID environment variable must be set.");

        var agent = new AzureAIAgent(await client.Administration.GetAgentAsync(triageAgentId), client);

        var messageContent = new StringBuilder();

        var triageOutputPrinter = triagePrinter.CreateSubtree("Output: ", ConsoleColor.White);

        await foreach (var response in agent.InvokeStreamingAsync(
            message: $"ID: {email.Id}\nFrom: {email.From}\nTo: {email.To}\nSubject: {email.Subject}\n\n{email.Body}",
            thread: thread))
        {
            if (!string.IsNullOrWhiteSpace(response.Message.Content))
            {
                messageContent.Append(response.Message.Content);
                triageOutputPrinter.Append(response.Message.Content, ConsoleColor.DarkGray);
            }
        }

        // Parse the JSON result into structured questions
        TriageResult triageResult;
        try
        {
            triageResult = JsonSerializer.Deserialize<TriageResult>(messageContent.ToString()) 
                ?? throw new InvalidOperationException("Failed to parse triage result.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Triage agent returned invalid JSON.", ex);
        }

        var questions = new List<QuestionAnswer>();
        foreach (var question in triageResult.Questions)
        {
            questions.Add(new QuestionAnswer
            {
                EmailId = email.Id,
                QuestionId = RandomStringGenerator.Generate(),
                Question = question,
                Answer = null
            });
        }

        return questions;
    }
}
