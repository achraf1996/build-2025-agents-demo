import os
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import BingGroundingTool

# Load environment variables from .env
load_dotenv()

# Constants
MODEL_NAME = "gpt-4.1-mini"
AGENT_NAME = "FAQ Agent JSON Formatter"
AGENT_ENV_KEY = "FAQ_AGENT_TO_JSON_ID"
CONNECTION_ID = "/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/binggourndingbuild"

INSTRUCTIONS = """Reformat the answers to this format:
```json
{"answered_questions": [{"question_id": "233414","answer": "The market size of the company is $1 billion."},{"question_id": "221123","answer": "The user sentiment is positive."}],"unanswered_questions": [""164234", "851234"]}
```

"I don't know" means that the question should be in the `unanswered_questions array."""

RESPONSE_FORMAT = {
    "type": "json_schema",
    "json_schema": {
        "name": "answered_questions",
        "schema": {
            "type": "object",
            "properties": {
                "answered_questions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "question_id": {"type": "string"},
                            "answer": {"type": "string"},
                        },
                        "required": ["question", "answer"],
                    },
                },
                "unanswered_questions": {
                    "type": "array",
                    "items": {
                        "type": "string"
                    },
                },
            },
            "required": ["answered_questions", "unanswered_questions"],
        },
    },
}


def get_project_endpoint() -> str:
    endpoint = os.getenv("PROJECT_ENDPOINT")
    if not endpoint:
        raise EnvironmentError("PROJECT_ENDPOINT is not set in the environment.")
    return endpoint


def create_project_client(endpoint: str) -> AIProjectClient:
    return AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())


def load_bing_tool() -> BingGroundingTool:
    return BingGroundingTool(connection_id=CONNECTION_ID)


def create_agent(client: AIProjectClient, tool: BingGroundingTool):
    agent = client.agents.create_agent(
        model=MODEL_NAME,
        name=AGENT_NAME,
        response_format=RESPONSE_FORMAT,
        instructions=INSTRUCTIONS
    )
    with open(".env", "a") as f:
        f.write(f"\n{AGENT_ENV_KEY}={agent.id}")
    return agent


def update_agent(client: AIProjectClient, agent_id: str, tool: BingGroundingTool):
    return client.agents.update_agent(
        agent_id=agent_id,
        model=MODEL_NAME,
        name=AGENT_NAME,
        response_format=RESPONSE_FORMAT,
        instructions=INSTRUCTIONS
    )


def main():
    endpoint = get_project_endpoint()
    client = create_project_client(endpoint)

    tool = load_bing_tool()

    agent_id = os.getenv(AGENT_ENV_KEY)
    if not agent_id:
        agent = create_agent(client, tool)
    else:
        agent = update_agent(client, agent_id, tool)

    print(f"{AGENT_NAME} ready with ID: {agent.id}")


if __name__ == "__main__":
    main()
