// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Events;
using DotNetEnv;
using System.Threading.Tasks;
using System.IO;
using System;
using YamlDotNet.Core.Tokens;

namespace Steps;

public sealed class RagAgent() : KernelProcessStep
{
    private ThreadsCollection _threads;

    [KernelFunction("init")]
    public void Init(KernelProcessStepContext context, ThreadsCollection threads)
    {
        Console.WriteLine("Init: RagAgent");
        _threads = threads;
    }

    [KernelFunction("execute")]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("RagAgent");

        // temp await
        await Task.Delay(1000);

        Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Run RAG agent on RAG thread
        // Update main thread with the RAG agent's response

        return "RAG response";
    }
}