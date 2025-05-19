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
using System.Threading;

namespace Steps;

/// <summary>
/// Executes RAG response generation for a list of triaged questions.
/// </summary>
public sealed class RagAgent(PersistentAgentsClient client) : KernelProcessStep<BaseEmailWorkflowStepState>
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
    /// Generates answers for a list of questions using the RAG agent.
    /// </summary>
    [KernelFunction("execute")]
    public async ValueTask<List<QuestionAnswer>> ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> questions)
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        var ragPrinter = TreePrinter.CreateSubtree("RAG Agent", ConsoleColor.Cyan);

        var ragAgentId = Environment.GetEnvironmentVariable("RAG_AGENT_ID");
        if (string.IsNullOrEmpty(ragAgentId))
            throw new InvalidOperationException("RAG_AGENT_ID environment variable must be set.");

        var agent = (await client.Administration.GetAgentAsync(ragAgentId)).Value;
        var thread = (await client.Threads.GetThreadAsync(_state.Threads.RagThreadId)).Value;

        var messageBuilder = new StringBuilder();
        foreach (var qa in questions)
        {
            messageBuilder.AppendLine($"{qa.QuestionId}: {qa.Question}");
        }

        var ragOutputPrinter = ragPrinter.CreateSubtree("Output:", ConsoleColor.White);

        var fullMessage = messageBuilder.ToString();

        await client.Messages.CreateMessageAsync(
            threadId: thread.Id,
            role: MessageRole.User,
            content: fullMessage
        );

        var run = await client.Runs.CreateRunAsync(
            thread: thread,
            agent: agent
        );

        //Poll for completion.
        do
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            run = client.Runs.GetRun(thread.Id, run.Value.Id);
        }
        while (run.Value.Status == RunStatus.Queued
            || run.Value.Status == RunStatus.InProgress
            || run.Value.Status == RunStatus.RequiresAction);


        var toJsonAgentId = Environment.GetEnvironmentVariable("RAG_AGENT_TO_JSON_ID");
        if (string.IsNullOrEmpty(toJsonAgentId))
            throw new InvalidOperationException("RAG_AGENT_TO_JSON_ID environment variable must be set.");

        var jsonAgent = new AzureAIAgent(await client.Administration.GetAgentAsync(toJsonAgentId), client);
        var skThread = new AzureAIAgentThread(client, _state.Threads.RagThreadId);

        var responseBuilder = new StringBuilder();
        await foreach (var response in jsonAgent.InvokeStreamingAsync("convert to json", skThread))
        {
            if (!string.IsNullOrWhiteSpace(response.Message.Content))
            {
                responseBuilder.Append(response.Message.Content);
                ragOutputPrinter.Append(response.Message.Content, ConsoleColor.DarkGray);
            }
        }

        string messageContent = responseBuilder.ToString();

        // Parse the JSON result into structured questions
        AgentAnswerResults ragResult;
        try
        {
            ragResult = JsonSerializer.Deserialize<AgentAnswerResults>(messageContent)
                ?? throw new InvalidOperationException("Failed to parse RAG result.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("RAG agent returned invalid JSON.", ex);
        }

        foreach (var question in ragResult.AnsweredQuestions)
        {
            questions.Find(q => q.QuestionId == question.QuestionId).Answer = question.Answer;
        }

        return questions;
    }
}
