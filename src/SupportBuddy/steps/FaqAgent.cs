// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Steps;

public sealed class FaqAgent() : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: FaqAgent");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("FaqAgent");

        // temp await
        await Task.Delay(1000);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Run FAQ agent on FAQ thread
        // Update main thread with the FAQ agent's response

        return "FAQ response";
    }
}