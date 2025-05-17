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

# Update the agent with the latest OpenAPI tool
agent = project_client.agents.create_agent(
    model="gpt-4.1",
    name="Customer Support Agent",
    response_format={
        "type": "json_schema",
        "json_schema": {
            "name": "questions_and_issues",
            "schema": {
                "type": "object",
                "properties": {
                    "questions": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        }
                    },
                    "issues": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        }
                    }
                },
            }
        }
    },
    instructions="""Look at the customer's email and break down all the questions and issues.

{
    "questions": [
        "What is the market size of the company?"
    ],
    "issues": [
        "The product is not working as expected."
    ]
}
"""
)