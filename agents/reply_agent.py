import os
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import BingGroundingTool

# Load environment variables from .env
load_dotenv()

# Constants
MODEL_NAME = "gpt-4o"
AGENT_NAME = "Reply Agent"
AGENT_ENV_KEY = "REPLY_AGENT_ID"
TOOL = {'type': 'openapi', 'openapi': {'name': 'Outlook', 'description': None, 'spec': {'openapi': '3.0.3', 'info': {'version': '1.0.0.0', 'title': 'SendEmail', 'description': 'Replies back to the original email'}, 'servers': [{'url': 'https://prod-04.westus2.logic.azure.com/workflows/a29a575becc640109293ca74e8ee48bb/triggers/Reply/paths'}], 'security': [{'sig': []}], 'paths': {'/invoke': {'post': {'description': 'Replies back to the original email', 'operationId': 'Reply-invoke', 'parameters': [{'name': 'api-version', 'in': 'query', 'description': '`2016-10-01` is the most common generally available version', 'required': True, 'schema': {'type': 'string', 'default': '2016-10-01'}, 'example': '2016-10-01'}, {'name': 'sv', 'in': 'query', 'description': 'The version number', 'required': True, 'schema': {'type': 'string', 'default': '1.0'}, 'example': '1.0'}, {'name': 'sp', 'in': 'query', 'description': 'The permissions', 'required': True, 'schema': {'type': 'string', 'default': '%2Ftriggers%2FWhen_response_is_received%2Frun'}, 'example': '%2Ftriggers%2FWhen_response_is_received%2Frun'}], 'responses': {'default': {'description': 'The Logic App Response.', 'content': {'application/json': {'schema': {'type': 'object'}}}}}, 'deprecated': False, 'requestBody': {'content': {'application/json': {'schema': {'type': 'object', 'properties': {'emailId': {'type': 'string'}, 'response': {'type': 'string'}}}}}, 'required': True}}}}, 'components': {'securitySchemes': {'sig': {'type': 'apiKey', 'description': 'The SHA 256 hash of the entire request URI with an internal key.', 'name': 'sig', 'in': 'query'}}}}, 'auth': {'type': 'connection', 'security_scheme': {'connection_id': '/subscriptions/8038977e-bdd7-447a-a194-d640a385ebcf/resourceGroups/rg-admin-0541/providers/Microsoft.CognitiveServices/accounts/mabolan-build-2025-demo-resource/projects/mabolan-build-2025-demo/connections/LogicApps_Tool_Connection_SendEmail_0466'}}, 'default_params': ['api-version', 'sp', 'sv']}, 'tags': ['logicapps']}

INSTRUCTIONS = """Reply back to the original email with the provided answers; format your reply using HTML"""


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
        tools=[TOOL],
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
        tools=[TOOL],
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
