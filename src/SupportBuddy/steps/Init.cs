using System;
using System.Threading.Tasks;
using Azure.AI.Agents.Persistent;
using Events;
using Microsoft.SemanticKernel;

public sealed class Init(PersistentAgentsClient client) : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask<ThreadsCollection> ExecuteAsync(KernelProcessStepContext context, Email email)
    {


        Console.WriteLine("Init");

        // Create all threads in parallel
        var createThreadsTask = Task.WhenAll(
            client.Threads.CreateThreadAsync(), // mainThread
            client.Threads.CreateThreadAsync(), // faqThread
            client.Threads.CreateThreadAsync()  // ragThread
        );

        var threads = await createThreadsTask;
        var mainThread = threads[0].Value;
        var faqThread = threads[1].Value;
        var ragThread = threads[2].Value;

        // Send message to main thread
        await client.Messages.CreateMessageAsync(
            mainThread.Id,
            MessageRole.User,
            $"From: {email.From}\nTo: {email.To}\nSubject: {email.Subject}\n\n{email.Body}"
        );

        return new ThreadsCollection
        {
            MainThreadId = mainThread.Id,
            FaqThreadId = faqThread.Id,
            RagThreadId = ragThread.Id
        };
    }
}
