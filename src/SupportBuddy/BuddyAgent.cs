// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.Agents.Persistent;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using System.Threading;
using System.Threading.Tasks;

public class BuddyAgent : AgentApplication
{
    private readonly SendUserMessageService _messageService;

    public BuddyAgent(
        AgentApplicationOptions options,
        SendUserMessageService messageService) : base(options)
    {
        _messageService = messageService;
        OnActivity(ActivityTypes.Message, OnMessageAsync, rank: RouteRank.Last);
    }

    private async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var userMessage = turnContext.Activity.Text;
        var conversationId = turnContext.Activity.Conversation.Id;

        await _messageService.HandleUserMessageAsync(conversationId, userMessage, turnContext, cancellationToken);
    }
}
