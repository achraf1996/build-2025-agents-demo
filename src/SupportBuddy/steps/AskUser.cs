// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using System.Collections.Generic;
using Azure.AI.Agents.Persistent;

namespace Steps;

public sealed class AskUser(SendUserMessageService service) : KernelProcessStep<BaseEmailWorkflowStepState>
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
        TreePrinter.Print("Asking user for details", ConsoleColor.White);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
        
        await service.AskUserToAnswerQuestionsAsync(QuestionAnswer);
    }
}