import os
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import BingGroundingTool

# Load environment variables from .env
load_dotenv()

# Constants
MODEL_NAME = "gpt-4o"
AGENT_NAME = "RAG Agent"
AGENT_ENV_KEY = "RAG_AGENT_ID"
TOOL = [{'type': 'bing_custom_search', 'bing_custom_search': {'search_configurations': [{'connection_id': '/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/custombing', 'instance_name': 'AgentService'}]}}]
INSTRUCTIONS = """You will be given question. Each one will have an ID.

First, search bing for answers for each one.

Then, provide the answers that you found using Bing web search. Do your best to provide an answer, but if you didn't find anything relevant, simply say "I don't know" for that specific question. 

YOU MUST NOT PROVIDE ANSWERS THAT YOU DIDN'T GET FROM BING. This includes not sharing personal information about yourself like your name or obvious facts."""


def get_project_endpoint() -> str:
    endpoint = os.getenv("PROJECT_ENDPOINT")
    if not endpoint:
        raise EnvironmentError("PROJECT_ENDPOINT is not set in the environment.")
    return endpoint


def create_project_client(endpoint: str) -> AIProjectClient:
    return AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())


def create_agent(client: AIProjectClient):
    agent = client.agents.create_agent(
        model=MODEL_NAME,
        name=AGENT_NAME,
        tools=TOOL,
        instructions=INSTRUCTIONS
    )
    with open(".env", "a") as f:
        f.write(f"\n{AGENT_ENV_KEY}={agent.id}")
    return agent


def update_agent(client: AIProjectClient, agent_id: str):
    return client.agents.update_agent(
        agent_id=agent_id,
        model=MODEL_NAME,
        name=AGENT_NAME,
        tools=TOOL,
        instructions=INSTRUCTIONS
    )


def main():
    endpoint = get_project_endpoint()
    client = create_project_client(endpoint)

    agent_id = os.getenv(AGENT_ENV_KEY)
    if not agent_id:
        agent = create_agent(client)
    else:
        agent = update_agent(client, agent_id)

    print(f"{AGENT_NAME} ready with ID: {agent.id}")


if __name__ == "__main__":
    main()
