# Agents-Build-2025
An AI-powered workflow that uses Microsoft’s Agents SDK, Azure AI Foundry Agent Service, and the Semantic Kernel process orchestration framework to triage, analyze, and reply to customer emails with minimal human intervention.


## Overview
This project implements an end-to-end email response system using AI agents orchestrated in a Semantic Kernel process. The system intelligently triages incoming emails, retrieves answers via FAQ and Retrieval-Augmented Generation (RAG) agents, optionally escalates to human users, and formulates a final reply.

Agents are registered and executed through Azure AI Foundry, while runtime orchestration is handled via the Semantic Kernel's KernelProcess API. Human interactions are integrated via the M365 Agents SDK, enabling seamless user input when the system is uncertain.

## Workflow
Triage Agent: Categorizes the email and determines if it can be routed to an FAQ or RAG agent.

- FAQ Agent: Provides exact matches from a predefined knowledge base.
- RAG Agent: Performs grounded retrieval from Bing and SharePoint if the FAQ agent lacks a match.
- Orchestrator Agent: Determines if the result is sufficient or requires user clarification.
- AskUser Agent: Asks a human to help complete any missing answers.
- Reply Agent: Synthesizes and sends the final email response.


## Running Locally
1. Prerequisites
    - .NET 8 SDK
    - Azure CLI (logged into a subscription with Foundry enabled)
    - Python 3.10+ (for agent creation scripts)
    - Environment variables defined in a .env file:
        ```ini
        PROJECT_ENDPOINT=https://your-project-endpoint
        BUDDY_AGENT_ID=...
        ```
2. Install .NET dependencies
    ```bash
    dotnet restore
    Run the app
    ```
3. Run the app
    ```bash
    dotnet run --project src/SupportBuddy
    ```
4. Trigger a workflow
Send a POST request to http://localhost:3978/api/new-email with the following payload:
    ```json
    {
    "id": "12345",
    "subject": "Where is my invoice?",
    "body": "Hi team, I never received my invoice for April."
    }
    ```

## Creating Agents in Foundry
Python scripts in agents/tools/ help you programmatically create and manage the agents required for this workflow. Run these once to initialize the agent ecosystem.

```bash
python agents/tools/faq_agent.py
python agents/tools/rag_agent.py
...
```

## Key Technologies
- Azure AI Foundry Agent Service: Manages agents and thread state for reasoning.
- Semantic Kernel Process Framework: Handles orchestration and control flow.
- Microsoft Agents SDK: Interfaces with channels like Microsoft Teams or Outlook.
- Bing + SharePoint: Data sources for grounded retrieval (RAG agent).

## License
This project is licensed under the MIT License.