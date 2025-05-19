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

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> faqAnswers, List<QuestionAnswer> ragAnswers)
    {
        var orchestratorPrinter = TreePrinter.CreateSubtree("Orchestrator Agent", ConsoleColor.Cyan);

        // Merge the FAQ and RAG answers into the state. 
        foreach (var faqAnswer in faqAnswers)
        {
            var existingQa = _state.QuestionAnswers.FirstOrDefault(x => x.QuestionId == faqAnswer.QuestionId);
            if (existingQa != null)
            {
                existingQa.Answer = faqAnswer.Answer ?? existingQa.Answer;
            }
            else
            {
                _state.QuestionAnswers.Add(faqAnswer);
            }
        }
        foreach (var ragAnswer in ragAnswers)
        {
            var existingQa = _state.QuestionAnswers.FirstOrDefault(x => x.QuestionId == ragAnswer.QuestionId);
            if (existingQa != null)
            {
                existingQa.Answer = ragAnswer.Answer ?? existingQa.Answer;
            }
            else
            {
                _state.QuestionAnswers.Add(ragAnswer);
            }
        }

        // Get unanswered questions from the FAQ and RAG threads
        var unansweredQuestions = _state.QuestionAnswers
            .Where(qa => string.IsNullOrEmpty(qa.Answer))
            .ToList();

        if (unansweredQuestions.Count == 0)
        {
            // If there are no unanswered questions, emit an event to notify that the user has responded
            orchestratorPrinter.CreateSubtree("Send email to customer", ConsoleColor.DarkYellow);
            await context.EmitEventAsync(ProcessEvents.SendEmailToCustomer, _state.QuestionAnswers);
            return;
        }

        // Emit an event to ask the user for details
        orchestratorPrinter.CreateSubtree("Ask user to answer questions", ConsoleColor.DarkYellow);
        await context.EmitEventAsync(ProcessEvents.AskUserForDetails, unansweredQuestions);
    }

    [KernelFunction("receiveUserResponse")]
    public async Task ExecuteWithUserResponseAsync(KernelProcessStepContext context, List<QuestionAnswer> questionAnswers)
    {
        var orchestratorPrinter = TreePrinter.CreateSubtree("Orchestrator Agent", ConsoleColor.Cyan);

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
            // TreePrinter.Print("Send email to customer", ConsoleColor.DarkYellow);
            // TreePrinter.Unindent();

            // If there are no unanswered questions, emit an event to notify that the user has responded
            orchestratorPrinter.CreateSubtree("Send email to customer", ConsoleColor.DarkYellow);
            await context.EmitEventAsync(ProcessEvents.SendEmailToCustomer, _state.QuestionAnswers);
            return;
        }

        // TreePrinter.Print("Ask user to answer questions", ConsoleColor.DarkYellow);
        // TreePrinter.Unindent();

        // Emit an event to notify that the user has responded
        orchestratorPrinter.CreateSubtree("Ask user to answer questions", ConsoleColor.DarkYellow);
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
