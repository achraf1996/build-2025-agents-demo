// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Steps;

public sealed class OrchestratorAgent() : KernelProcessStep<OrchestratorState>
{
    private OrchestratorState _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<OrchestratorState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        // TreePrinter.Print("Initializing Orchestrator agent", ConsoleColor.White);
        _state.Threads = threads;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context, string faqAnswers, string ragAnswers)
    {
        TreePrinter.Print("Orchestrator Agent", ConsoleColor.Cyan);
        TreePrinter.Indent();

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // temp await
        await Task.Delay(1);

        _state.QuestionAnswers = [
            new QuestionAnswer
            {
                EmailId = "12345",
                QuestionId = "abcde",
                Question = "What is your name?",
            },
            new QuestionAnswer
            {
                EmailId = "12345",
                QuestionId = "abcde",
                Question = "What is your favorite color?",
                Answer = "Blue"
            }
        ];

        // Get unanswered questions from the FAQ and RAG threads
        var unansweredQuestions = _state.QuestionAnswers
            .Where(qa => string.IsNullOrEmpty(qa.Answer))
            .ToList();

        if (unansweredQuestions.Count == 0)
        {
            TreePrinter.Print("Send email to customer", ConsoleColor.DarkYellow);
            TreePrinter.Unindent();

            // If there are no unanswered questions, emit an event to notify that the user has responded
            await context.EmitEventAsync(ProcessEvents.SendEmailToCustomer, _state.QuestionAnswers);
            return;
        }

        TreePrinter.Print("Ask user to answer questions", ConsoleColor.DarkYellow);
        TreePrinter.Unindent();

        // Emit an event to ask the user for details
        await context.EmitEventAsync(ProcessEvents.AskUserForDetails, unansweredQuestions);
    }

    [KernelFunction("receiveUserResponse")]
    public async Task ExecuteWithUserResponseAsync(KernelProcessStepContext context, List<QuestionAnswer> questionAnswers)
    {
        TreePrinter.Print("Orchestrator Agent", ConsoleColor.Cyan);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // temp await
        await Task.Delay(1);

        // Process the user response
        foreach (var qa in questionAnswers)
        {
            // Update the corresponding QuestionAnswer object with the user's response
            var existingQa = _state.QuestionAnswers.FirstOrDefault(x => x.QuestionId == qa.QuestionId);
            if (existingQa != null)
            {
                existingQa.Answer = qa.Answer;
            }
        }

        // Check if there are any unanswered questions left
        var unansweredQuestions = _state.QuestionAnswers
            .Where(qa => string.IsNullOrEmpty(qa.Answer))
            .ToList();
            
        if (unansweredQuestions.Count == 0)
        {
            TreePrinter.Print("Send email to customer", ConsoleColor.DarkYellow);
            TreePrinter.Unindent();

            // If there are no unanswered questions, emit an event to notify that the user has responded
            await context.EmitEventAsync(ProcessEvents.SendEmailToCustomer, _state.QuestionAnswers);
            return;
        }

        TreePrinter.Print("Ask user to answer questions", ConsoleColor.DarkYellow);
        TreePrinter.Unindent();

        // Emit an event to notify that the user has responded
        await context.EmitEventAsync(ProcessEvents.AskUserForDetails, unansweredQuestions);
    }
}

public class OrchestratorState : BaseEmailWorkflowStepState
{
    public List<QuestionAnswer> QuestionAnswers { get; set; } = [];

    public OrchestratorState()
    {
        Threads = new ThreadsCollection();
        QuestionAnswers = [];
    }

    public OrchestratorState(BaseEmailWorkflowStepState state)
    {
        Threads = state.Threads;
    }
}
