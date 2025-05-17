// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.OpenAI;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Threading;
using Azure.AI.Agents.Persistent;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Logging.AddConsole();

// builder.Services.AddTransient<ChatClient>(sp =>
// {
//     return new AzureOpenAIClient(
//             new Uri(builder.Configuration["AIServices:AzureOpenAI:Endpoint"]),
//             new ApiKeyCredential(builder.Configuration["AIServices:AzureOpenAI:ApiKey"]))
//     .GetChatClient(builder.Configuration["AIServices:AzureOpenAI:DeploymentName"]);
// });

// Add AgentApplicationOptions.  This will use DI'd services and IConfiguration for construction.
builder.AddAgentApplicationOptions();
builder.AddAgentCore();

// Add the Agent
builder.AddAgent<BuddyAgent>();

// Register IStorage.  For development, MemoryStorage is suitable.
// For production Agents, persisted storage should be used so
// that state survives Agent restarts, and operate correctly
// in a cluster of Agent instances.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

builder.Services.AddSingleton<SendUserMessageService>();
builder.Services.AddSingleton<RespondToEmailWorkflowService>();

// Add in-memory storage
builder.Services.AddSingleton<IConversationStateStore, InMemoryConversationStateStore>();
builder.Services.AddSingleton<IProcessStateStore, ProcessStateStore>();

builder.Services.AddSingleton<PersistentAgentsClient>(sp =>
{
    //Create a PersistentAgentsClient and PersistentAgent.
    PersistentAgentsClient client = new("https://mabolan-build-2025-demo-resource.services.ai.azure.com/api/projects/mabolan-build-2025-demo", new DefaultAzureCredential());

    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

app.MapPost("/api/messages", async (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
})
    .AllowAnonymous();

app.MapPost("/api/new-email", async (
    Email request,
    RespondToEmailWorkflowService workflowService,
    CancellationToken cancellationToken) =>
{
    await workflowService.StartWorkflow(request, cancellationToken);
})
    .AllowAnonymous();

// Hardcoded for brevity and ease of testing. 
// In production, this should be set in configuration.
app.Urls.Add($"http://localhost:3978");
app.MapGet("/", () => "Microsoft Agents SDK Sample");

app.Run();