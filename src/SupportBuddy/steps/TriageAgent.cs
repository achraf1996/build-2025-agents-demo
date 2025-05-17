// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Steps;

public sealed class TriageAgent() : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: TriageAgent");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("TriageAgent");

        // temp await
        await Task.Delay(1000);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Run triage agent on main thread
        // Update faq and rag threads with the triage agent's response
    }
}