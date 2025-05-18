// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using YamlDotNet.Core.Tokens;
using System.Collections.Generic;

namespace Steps;

public sealed class RagAgent() : KernelProcessStep<BaseEmailWorkflowStepState>
{
    private BaseEmailWorkflowStepState _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<BaseEmailWorkflowStepState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    [KernelFunction("execute")]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, List<QuestionAnswer> questions)
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        TreePrinter.Print("RAG Agent", ConsoleColor.Cyan);
        TreePrinter.Indent();

        // temp await
        await Task.Delay(1);

        // Run RAG agent on FAQ thread
        // Update main thread with the FAQ agent's response

        TreePrinter.Print("Output: ", ConsoleColor.White);
        TreePrinter.Unindent();

        return "RAG response";
    }
}