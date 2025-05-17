// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using System.Collections.Generic;

namespace Steps;

public sealed class RelayAgent() : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: RelayAgent");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context, string faqAnswers, string ragAnswers)
    {
        Console.WriteLine("RelayAgent");

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // temp await
        await Task.Delay(1000);

        List<UnansweredQuestions> unansweredQuestions = [
            new UnansweredQuestions
            {
                EmailId = "asdf",
                Question = "What is your name?",
            }
        ];

        await context.EmitEventAsync(ProcessEvents.AskUserForDetails, unansweredQuestions);
        
        Console.WriteLine("RelayAgent: EmitEventAsync");
    }
}
