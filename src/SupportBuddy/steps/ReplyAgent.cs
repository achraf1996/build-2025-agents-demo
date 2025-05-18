// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;

namespace Steps;

public sealed class ReplyAgent() : KernelProcessStep<BaseEmailWorkflowStepState>
{
    private BaseEmailWorkflowStepState _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<BaseEmailWorkflowStepState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> QuestionAnswer)
    {
        TreePrinter.Print("Reply agent", ConsoleColor.Cyan);

        // temp await
        await Task.Delay(1);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Run RAG agent on RAG thread
        // Update main thread with the RAG agent's response
    }
}