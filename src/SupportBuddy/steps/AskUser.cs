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

public sealed class AskUser(SendUserMessageService service) : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: AskUser");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context, List<UnansweredQuestions> unansweredQuestions)
    {
        Console.WriteLine("Asking user for details");

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
        
        await service.AskUserToAnswerQuestionsAsync(unansweredQuestions);
    }
}
