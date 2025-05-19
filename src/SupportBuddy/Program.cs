// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using DotNetEnv;
using System.IO;

// Load environment variables from .env file at application startup
Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddDebug(); // Logs to Visual Studio/VS Code debug output window

// Add core ASP.NET services
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add Microsoft Agents SDK services
builder.AddAgentApplicationOptions(); // Registers AgentApplicationOptions for DI
builder.AddAgentCore();               // Registers core agent services
builder.AddAgent<BuddyAgent>();       // Registers the BuddyAgent implementation

// Add ephemeral in-memory state storage for development use
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<IConversationStateStore, InMemoryConversationStateStore>();
builder.Services.AddSingleton<IProcessStateStore, ProcessStateStore>();

// Register workflow and messaging services
builder.Services.AddSingleton<SendUserMessageService>();
builder.Services.AddSingleton<RespondToEmailWorkflowService>();

// Register Lazy<T> wrapper for deferred resolution of workflow service
builder.Services.AddSingleton(provider =>
    new Lazy<RespondToEmailWorkflowService>(() =>
        provider.GetRequiredService<RespondToEmailWorkflowService>()));

// Configure PersistentAgentsClient with Azure identity and environment endpoint
builder.Services.AddSingleton<PersistentAgentsClient>(_ =>
{
    var endpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
    return new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
});

var app = builder.Build();

// Configure HTTP request routing
app.UseRouting();

// Route to process messages from channels (e.g., Microsoft Bot Framework)
app.MapPost("/api/messages", async (
    HttpRequest request,
    HttpResponse response,
    IAgentHttpAdapter adapter,
    IAgent agent,
    CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
}).AllowAnonymous();

// Route to start a new workflow when an email is received
app.MapPost("/api/new-email", async (
    Email request,
    RespondToEmailWorkflowService workflowService,
    CancellationToken cancellationToken) =>
{
    await workflowService.StartWorkflow(request, cancellationToken);
}).AllowAnonymous();

// Configure local development URL (hardcoded for testing)
app.Urls.Add("http://localhost:3978");

// Basic health check/test route
app.MapGet("/", () => "Microsoft Agents SDK Sample");

// Start the application
app.Run();
