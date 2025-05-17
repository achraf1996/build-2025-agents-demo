import os
import jsonref
from dotenv import load_dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.ai.agents.models import OpenApiTool, OpenApiConnectionAuthDetails, OpenApiConnectionSecurityScheme

load_dotenv()

# Create an Azure AI Client from an endpoint, copied from your Azure AI Foundry project.
# You need to login to Azure subscription via Azure CLI and set the environment variables
project_endpoint = os.environ["PROJECT_ENDPOINT"]  # Ensure the PROJECT_ENDPOINT environment variable is set

# Create an AIProjectClient instance
project_client = AIProjectClient(
    endpoint=project_endpoint,
    credential=DefaultAzureCredential(),  # Use Azure Default Credential for authentication
)

# Load the OpenAPI specification for the weather service from a local JSON file using jsonref to handle references
with open(os.path.join(os.path.dirname(__file__), "tools/cqa_tool.json"), "r") as f:
        openapi_cqa_tool = jsonref.loads(f.read())

# Create Auth object for the OpenApiTool (note: using anonymous auth here; connection or managed identity requires additional setup)
auth = OpenApiConnectionAuthDetails(
        security_scheme=OpenApiConnectionSecurityScheme(
            connection_id="/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/language-demo-connection"
        ),
)

# Initialize the main OpenAPI tool definition for weather
openapi_tool = OpenApiTool(
    name="faq", spec=openapi_cqa_tool, description="Gets answers to questions", auth=auth
)

with project_client:
    agent = project_client.agents.create_agent(
        model="gpt-4.1",  # Model deployment name
        name="faq-agent",  # Name of the agent
        tools=openapi_tool.definitions,
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
        
        instructions="""Get answers for the user's questions by using the faq and return the exact answer without rewriting the answer. Format the answers in JSON.

You MUST use the cqa_tool. Do not rely on your own knowledge.

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