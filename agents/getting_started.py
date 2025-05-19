import os
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient

# Load environment variables from .env
load_dotenv()

# Set constants
MODEL_NAME = "gpt-4o"
AGENT_NAME = "Customer Support Agent"
AGENT_ENV_KEY = "TEST_AGENT_ID"
INSTRUCTIONS = "Search for the answers to the questions about Azure AI Foundry Agent Service using Bing."

# Tool definition
TOOL = [{
    "type": "bing_custom_search",
    "bing_custom_search": {
        "search_configurations": [{
            "connection_id": "/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/msbuildcustombing",
            "instance_name": "AgentServicLearnPages"
        }]
    }
}]

# Create the agent
endpoint = os.environ["PROJECT_ENDPOINT"]
client = AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())

agent = client.agents.create_agent(
    model=MODEL_NAME,
    name=AGENT_NAME,
    tools=TOOL,
    instructions=INSTRUCTIONS
)

# Save the agent ID for later use
with open(".env", "a") as f:
    f.write(f"\n{AGENT_ENV_KEY}={agent.id}")

print(f"{AGENT_NAME} created with ID: {agent.id}")
