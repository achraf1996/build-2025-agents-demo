import os
from dotenv import load_dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.ai.agents.models import BingGroundingTool

load_dotenv()

# Create an Azure AI Client from an endpoint, copied from your Azure AI Foundry project.
# You need to login to Azure subscription via Azure CLI and set the environment variables
project_endpoint = os.environ["PROJECT_ENDPOINT"]  # Ensure the PROJECT_ENDPOINT environment variable is set

# Create an AIProjectClient instance
project_client = AIProjectClient(
    endpoint=project_endpoint,
    credential=DefaultAzureCredential(),  # Use Azure Default Credential for authentication
)

bing = BingGroundingTool(connection_id="/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/binggourndingbuild")

with project_client:
    agent = project_client.agents.create_agent(
        model="gpt-4.1",  # Model deployment name
        name="rag-agent",  # Name of the agent
        tools=bing.definitions,
        response_format={
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
                                    "question": {"type": "string"},
                                    "answer": {"type": "string"},
                                },
                                "required": ["question", "answer"],
                            },
                        },
                        "unanswered_questions": {
                            "type": "array",
                            "items": {
                                "type": "object",
                                "properties": {
                                    "question": {"type": "string"},
                                },
                                "required": ["question"],
                            },
                        },
                    },
                    "required": ["answered_questions", "unanswered_questions"]
                }
            }
        },
        instructions="""Get answers for the user's questions by using the bing search tool. Format the answers in JSON.

{
    "answered_questions": [
        {
            "question": "What is the market size of the company?",
            "answer": "The market size of the company is $1 billion."
        },
        {
            "question": "What is the user sentiment?",
            "answer": "The user sentiment is positive."
        }
    ],
    "unanswered_questions": [
        {
            "question": "What is the market size of the company?"
        },
        {
            "question": "What is the user sentiment?"
        }
    ]
}"""
    )
    print(f"Created agent, ID: {agent.id}")