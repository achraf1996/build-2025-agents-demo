
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

using Events;
using Steps;
using System;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.DependencyInjection;

public class RespondToEmailWorkflowService(
    SendUserMessageService sendUserMessageService,
    PersistentAgentsClient clientProvider,
    IProcessStateStore processStateStore
)
{

    public async Task StartWorkflow(Email email, CancellationToken cancellationToken)
    {
        Kernel kernel = CreateKernel();
        KernelProcess kernelProcess = CreateWorkflow();

        // Start the process with an initial external event
        var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent()
            {
                Id = ProcessEvents.StartProcess,
                Data = email
            });

        // Get the process state
        var state = await runningProcess.GetStateAsync();

        // Save the process state to the store
        await processStateStore.SaveAsync(
            email.Id,
            state
        );
    }

    public async Task ContinueWorkflow(string emailId, string userMessage, CancellationToken cancellationToken)
    {
        Kernel kernel = CreateKernel();

        // Get the process state from the store
        var processState = await processStateStore.GetAsync(emailId);

        if (processState == null)
        {
            Console.WriteLine($"No process state found for email ID: {emailId}");
            return;
        }

        // Continue the process with an external event
        await processState.StartAsync(
            kernel,
            new KernelProcessEvent()
            {
                Id = ProcessEvents.ReceiveUserMessage,
                Data = userMessage
            });
    }   

    private Kernel CreateKernel()
    {
        // Create a simple kernel 
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(clientProvider);
        kernelBuilder.Services.AddSingleton(sendUserMessageService);
        return kernelBuilder.Build();
    }

    private KernelProcess CreateWorkflow()
    {

        // Create a process that will interact with the chat completion service
        ProcessBuilder process = new("ChatBot");
        var init = process.AddStepFromType<Init>();
        var emailTriageAgent = process.AddStepFromType<TriageAgent>();
        var faqAgent = process.AddStepFromType<FaqAgent>();
        var raqAgent = process.AddStepFromType<RagAgent>();
        var relayAgent = process.AddStepFromType<RelayAgent>();
        var askUserAgent = process.AddStepFromType<AskUser>();
        var replyAgent = process.AddStepFromType<ReplyAgent>();

        // Define the process flow
        process
            .OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(init));

        init
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(emailTriageAgent, "init"))
            .SendEventTo(new ProcessFunctionTargetBuilder(faqAgent, "init"))
            .SendEventTo(new ProcessFunctionTargetBuilder(raqAgent, "init"))
            .SendEventTo(new ProcessFunctionTargetBuilder(relayAgent, "init"))
            .SendEventTo(new ProcessFunctionTargetBuilder(askUserAgent, "init"))
            .SendEventTo(new ProcessFunctionTargetBuilder(replyAgent, "init"));

        emailTriageAgent
            .OnFunctionResult("init")
            .SendEventTo(new ProcessFunctionTargetBuilder(emailTriageAgent, "execute"));

        emailTriageAgent
            .OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(faqAgent, "execute"))
            .SendEventTo(new ProcessFunctionTargetBuilder(raqAgent, "execute"));

        faqAgent
            .OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(relayAgent, "execute", "faqAnswers"));

        raqAgent
            .OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(relayAgent, "execute", "ragAnswers"));

        relayAgent
            .OnEvent(ProcessEvents.AskUserForDetails)
            .SendEventTo(new ProcessFunctionTargetBuilder(askUserAgent, "execute"));

        relayAgent
            .OnEvent(ProcessEvents.SendEmailToCustomer)
            .SendEventTo(new ProcessFunctionTargetBuilder(replyAgent, "execute"));

        replyAgent
            .OnFunctionResult("execute")
            .StopProcess();

        // Build the process to get a handle that can be started
        return process.Build();
    }   
}
