// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Steps;

public sealed class ReplyAgent() : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: ReplyAgent");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async Task ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("ReplyAgent");

        // temp await
        await Task.Delay(1000);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Run RAG agent on RAG thread
        // Update main thread with the RAG agent's response
    }
}