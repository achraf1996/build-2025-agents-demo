import os
import jsonref
from dotenv import load_dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.ai.agents.models import ConnectedAgentTool

load_dotenv()

# Create an Azure AI Client from an endpoint, copied from your Azure AI Foundry project.
# You need to login to Azure subscription via Azure CLI and set the environment variables
project_endpoint = os.environ["PROJECT_ENDPOINT"]  # Ensure the PROJECT_ENDPOINT environment variable is set

# Create an AIProjectClient instance
project_client = AIProjectClient(
    endpoint=project_endpoint,
    credential=DefaultAzureCredential(),  # Use Azure Default Credential for authentication
)

faq_agent = ConnectedAgentTool(
    id="asst_s7FZqTt6E80ovF6lQU2R5f5X", name="FaqAgent", description="Gets exact answers to common questions"
)

rag_agent = ConnectedAgentTool(
    id="asst_93uSVbxtNIiaugxcB3aSLZoz", name="RagAgent", description="Gets answers to common questions through research"
)

# Update the agent with the latest OpenAPI tool
agent = project_client.agents.create_agent(
    model="gpt-4.1",
    name="Customer Support Agent",
    tools=faq_agent.definitions + rag_agent.definitions,
    instructions="""Use the FaqAgent and RagAgent to get answers to the user's questions.

Follow the instructions below:
1. Send parallel requests to both agents
2. Prefer the FaqAgent's answer if it exists; return it as the final answer with no edits
3. If the FaqAgent doesn't have an answer, use the RagAgent to get the answer
4. If the RagAgent doesn't have an answer, return "I don't know" as the final answer"""
)