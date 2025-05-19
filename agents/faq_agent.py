import os
import jsonref
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import (
    OpenApiTool,
    OpenApiConnectionAuthDetails,
    OpenApiConnectionSecurityScheme,
)

# Load environment variables from .env
load_dotenv()

# Constants
MODEL_NAME = "gpt-4.1-mini"
AGENT_NAME = "FAQ Agent (with exact answers)"
AGENT_ENV_KEY = "FAQ_AGENT_ID"
OPENAPI_SPEC_PATH = os.path.join(os.path.dirname(__file__), "tools/cqa_tool.json")
CONNECTION_ID = "/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/language-demo-connection"

INSTRUCTIONS = """Get answers for the user's questions by using the faq and return the exact answer without rewriting the answer. Format the answers in JSON.

You MUST use the FAQ tool. Do not rely on your own knowledge. If the answer is not found by the FAQ tool, say "I don't know" for that specific question.
```"""


def get_project_endpoint() -> str:
    endpoint = os.getenv("PROJECT_ENDPOINT")
    if not endpoint:
        raise EnvironmentError("PROJECT_ENDPOINT is not set in the environment.")
    return endpoint


def create_project_client(endpoint: str) -> AIProjectClient:
    return AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())


def load_openapi_tool() -> OpenApiTool:
    with open(OPENAPI_SPEC_PATH, "r") as f:
        spec = jsonref.loads(f.read())

    auth = OpenApiConnectionAuthDetails(
        security_scheme=OpenApiConnectionSecurityScheme(connection_id=CONNECTION_ID)
    )

    return OpenApiTool(name="faq", spec=spec, description="Gets answers to questions", auth=auth)


def create_agent(client: AIProjectClient, openapi_tool: OpenApiTool):
    agent = client.agents.create_agent(
        model=MODEL_NAME,
        name=AGENT_NAME,
        tools=openapi_tool.definitions,
        instructions=INSTRUCTIONS
    )
    with open(".env", "a") as f:
        f.write(f"\n{AGENT_ENV_KEY}={agent.id}")
    return agent


def update_agent(client: AIProjectClient, agent_id: str, openapi_tool: OpenApiTool):
    return client.agents.update_agent(
        agent_id=agent_id,
        model=MODEL_NAME,
        name=AGENT_NAME,
        tools=openapi_tool.definitions,
        instructions=INSTRUCTIONS
    )


def main():
    endpoint = get_project_endpoint()
    client = create_project_client(endpoint)
    tool = load_openapi_tool()

    agent_id = os.getenv(AGENT_ENV_KEY)
    if not agent_id:
        agent = create_agent(client, tool)
    else:
        agent = update_agent(client, agent_id, tool)

    print(f"{AGENT_NAME} ready with ID: {agent.id}")


if __name__ == "__main__":
    main()
