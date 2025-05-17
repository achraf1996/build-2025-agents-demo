// Copyright (c) Microsoft. All rights reserved.
namespace Events;

/// <summary>
/// Processes Events emitted by shared steps.<br/>
/// </summary>
public static class ProcessEvents
{
    public static readonly string StartProcess = nameof(StartProcess);
    public static readonly string AskUserForDetails = nameof(AskUserForDetails);
    public static readonly string SendEmailToCustomer = nameof(SendEmailToCustomer);
    public static readonly string ReceiveUserMessage = nameof(ReceiveUserMessage);
}