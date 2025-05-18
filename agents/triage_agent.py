import os
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient

# Load environment variables from .env file
load_dotenv()

# Constants
MODEL_NAME = "gpt-4.1"
AGENT_NAME = "Triage Agent"
AGENT_ENV_KEY = "TRIAGE_AGENT_ID"
INSTRUCTIONS = """Look at the customer's email and break down all the questions and issues.

{
    "questions": [
        "What is the market size of the company?"
    ],
    "issues": [
        "The product is not working as expected."
    ]
}
"""
RESPONSE_FORMAT = {
    "type": "json_schema",
    "json_schema": {
        "name": "questions_and_issues",
        "schema": {
            "type": "object",
            "properties": {
                "questions": {"type": "array", "items": {"type": "string"}},
                "issues": {"type": "array", "items": {"type": "string"}}
            },
        },
    }
}


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
        response_format=RESPONSE_FORMAT,
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
        response_format=RESPONSE_FORMAT,
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
