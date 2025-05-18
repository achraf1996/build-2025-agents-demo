using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Azure.AI.Agents.Persistent;

using Events;
using Steps;

public class RespondToEmailWorkflowService(
    SendUserMessageService sendUserMessageService,
    PersistentAgentsClient clientProvider,
    IProcessStateStore processStateStore)
{
    /// <summary>
    /// Starts a new workflow process based on the incoming email.
    /// </summary>
    public async Task StartWorkflow(Email email, CancellationToken cancellationToken)
    {
        var kernel = CreateKernel();
        var kernelProcess = await CreateWorkflowAsync();

        TreePrinter.Print($"Process starting with ID: " + email.Id, ConsoleColor.Blue);
        TreePrinter.Indent();

        // Kick off the workflow with an initial StartProcess event containing the email
        var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent
            {
                Id = ProcessEvents.StartProcess,
                Data = email
            });


        // Save the resulting process state for future continuation
        var state = await runningProcess.GetStateAsync();
        await processStateStore.SaveAsync(email.Id, state);
        var test = await processStateStore.GetAsync(email.Id);

        TreePrinter.Print($"Process finished", ConsoleColor.Green);
        TreePrinter.Unindent();
    }

    /// <summary>
    /// Continues a previously saved workflow with user-provided answers.
    /// </summary>
    public async Task ContinueWorkflow(string emailId, List<QuestionAnswer> answeredQuestions)
    {
        var kernel = CreateKernel();

        TreePrinter.Print($"Process resuming with ID: {emailId}", ConsoleColor.Blue);
        TreePrinter.Indent();

        // Load saved process state by email ID
        var processState = await processStateStore.GetAsync(emailId);
        if (processState == null)
        {
            TreePrinter.Print($"Error: No process found with ID: {emailId}", ConsoleColor.Red);
            return;
        }

        // Resume process with the user's answers
        await processState.StartAsync(
            kernel,
            new KernelProcessEvent
            {
                Id = ProcessEvents.ReceiveUserMessage,
                Data = answeredQuestions
            });

        TreePrinter.Print($"Process finished", ConsoleColor.Green);
        TreePrinter.Unindent();
    }

    /// <summary>
    /// Creates a kernel with the necessary services registered for the workflow.
    /// </summary>
    private Kernel CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(clientProvider);
        kernelBuilder.Services.AddSingleton(sendUserMessageService);
        return kernelBuilder.Build();
    }

    /// <summary>
    /// Constructs the full process definition, including steps and transitions.
    /// </summary>
    private async Task<KernelProcess> CreateWorkflowAsync()
    {
        // Create all threads in parallel
        var createThreadsTask = Task.WhenAll(
            clientProvider.Threads.CreateThreadAsync(), // mainThread
            clientProvider.Threads.CreateThreadAsync(), // faqThread
            clientProvider.Threads.CreateThreadAsync()  // ragThread
        );

        // Send message to main thread
        var threads = await createThreadsTask;
        ThreadsCollection threadsCollection = new ThreadsCollection
        {
            MainThreadId = threads[0].Value.Id,
            FaqThreadId = threads[1].Value.Id,
            RagThreadId = threads[2].Value.Id
        };

        BaseEmailWorkflowStepState state = new()
        {
            Threads = threadsCollection
        };

        var process = new ProcessBuilder("ChatBot");

        // Define all steps in the process
        var emailTriageAgent = process.AddStepFromType<TriageAgent, BaseEmailWorkflowStepState>(initialState: state);
        var faqAgent = process.AddStepFromType<FaqAgent, BaseEmailWorkflowStepState>(initialState: state);
        var ragAgent = process.AddStepFromType<RagAgent, BaseEmailWorkflowStepState>(initialState: state);
        var OrchestratorAgent = process.AddStepFromType<OrchestratorAgent, OrchestratorState>(initialState: new(state));
        var askUserAgent = process.AddStepFromType<AskUser, BaseEmailWorkflowStepState>(initialState: state);
        var replyAgent = process.AddStepFromType<ReplyAgent, BaseEmailWorkflowStepState>(initialState: state);

        // Define input events and initial routing
        process
            .OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(emailTriageAgent));

        process
            .OnInputEvent(ProcessEvents.ReceiveUserMessage)
            .SendEventTo(new ProcessFunctionTargetBuilder(OrchestratorAgent, "receiveUserResponse"));

        emailTriageAgent.OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(faqAgent))
            .SendEventTo(new ProcessFunctionTargetBuilder(ragAgent));

        // FAQ and RAG agents forward results to RELAY
        faqAgent.OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(OrchestratorAgent, "execute", parameterName: "faqAnswers"));

        ragAgent.OnFunctionResult("execute")
            .SendEventTo(new ProcessFunctionTargetBuilder(OrchestratorAgent, "execute", parameterName: "ragAnswers"));

        // RELAY decides next step: ask user or send reply
        OrchestratorAgent.OnEvent(ProcessEvents.AskUserForDetails)
            .SendEventTo(new ProcessFunctionTargetBuilder(askUserAgent));

        OrchestratorAgent.OnEvent(ProcessEvents.SendEmailToCustomer)
            .SendEventTo(new ProcessFunctionTargetBuilder(replyAgent));

        // Final step ends the process
        replyAgent.OnFunctionResult("execute").StopProcess();

        return process.Build();
    }
}
