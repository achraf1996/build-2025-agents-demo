import React, { useState } from 'react'
import { 
  Settings as SettingsIcon, 
  Bot, 
  Database, 
  Globe, 
  Shield,
  Save,
  RefreshCw,
  AlertCircle,
  CheckCircle
} from 'lucide-react'

const Settings: React.FC = () => {
  const [activeTab, setActiveTab] = useState('agents')
  const [saveStatus, setSaveStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle')

  const tabs = [
    { id: 'agents', name: 'AI Agents', icon: Bot },
    { id: 'connections', name: 'Connections', icon: Database },
    { id: 'workflow', name: 'Workflow', icon: RefreshCw },
    { id: 'security', name: 'Security', icon: Shield },
  ]

  const agents = [
    {
      id: 'triage',
      name: 'Triage Agent',
      description: 'Analyzes and categorizes incoming emails',
      status: 'online',
      model: 'gpt-4.1-mini',
      lastUpdated: '2024-01-15 10:30:00'
    },
    {
      id: 'faq',
      name: 'FAQ Agent',
      description: 'Provides answers from knowledge base',
      status: 'online',
      model: 'gpt-4.1-mini',
      lastUpdated: '2024-01-15 10:30:00'
    },
    {
      id: 'rag',
      name: 'RAG Agent',
      description: 'Performs web search and retrieval',
      status: 'online',
      model: 'gpt-4.1',
      lastUpdated: '2024-01-15 10:30:00'
    },
    {
      id: 'reply',
      name: 'Reply Agent',
      description: 'Composes final email responses',
      status: 'online',
      model: 'gpt-4o',
      lastUpdated: '2024-01-15 10:30:00'
    }
  ]

  const handleSave = async () => {
    setSaveStatus('saving')
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    setSaveStatus('saved')
    setTimeout(() => setSaveStatus('idle'), 2000)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'online':
        return 'bg-success-100 text-success-800'
      case 'offline':
        return 'bg-red-100 text-red-800'
      case 'maintenance':
        return 'bg-warning-100 text-warning-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Settings</h1>
        <p className="mt-2 text-gray-600">
          Configure your AI agents and system settings
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Sidebar Navigation */}
        <div className="lg:col-span-1">
          <nav className="space-y-1">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`w-full flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors duration-200 ${
                  activeTab === tab.id
                    ? 'bg-primary-50 text-primary-700 border-r-2 border-primary-600'
                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                }`}
              >
                <tab.icon className="h-5 w-5 mr-3" />
                {tab.name}
              </button>
            ))}
          </nav>
        </div>

        {/* Main Content */}
        <div className="lg:col-span-3">
          <div className="card">
            {/* AI Agents Tab */}
            {activeTab === 'agents' && (
              <div className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-xl font-semibold text-gray-900">AI Agents</h2>
                    <p className="text-sm text-gray-600">Manage your AI agents and their configurations</p>
                  </div>
                  <button onClick={handleSave} className="btn-primary">
                    {saveStatus === 'saving' ? (
                      <>
                        <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                        Saving...
                      </>
                    ) : saveStatus === 'saved' ? (
                      <>
                        <CheckCircle className="h-4 w-4 mr-2" />
                        Saved
                      </>
                    ) : (
                      <>
                        <Save className="h-4 w-4 mr-2" />
                        Save Changes
                      </>
                    )}
                  </button>
                </div>

                <div className="space-y-4">
                  {agents.map((agent) => (
                    <div key={agent.id} className="border border-gray-200 rounded-lg p-4 hover:border-gray-300 transition-colors duration-200">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-4">
                          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100">
                            <Bot className="h-5 w-5 text-primary-600" />
                          </div>
                          <div>
                            <h3 className="font-medium text-gray-900">{agent.name}</h3>
                            <p className="text-sm text-gray-600">{agent.description}</p>
                          </div>
                        </div>
                        <div className="flex items-center space-x-3">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(agent.status)}`}>
                            {agent.status}
                          </span>
                          <button className="text-primary-600 hover:text-primary-700 text-sm font-medium">
                            Configure
                          </button>
                        </div>
                      </div>
                      <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-gray-500">Model:</span>
                          <span className="ml-2 text-gray-900">{agent.model}</span>
                        </div>
                        <div>
                          <span className="text-gray-500">Last Updated:</span>
                          <span className="ml-2 text-gray-900">{new Date(agent.lastUpdated).toLocaleString()}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Connections Tab */}
            {activeTab === 'connections' && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">Connections</h2>
                  <p className="text-sm text-gray-600">Manage external service connections</p>
                </div>

                <div className="space-y-4">
                  <div className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-3">
                        <Database className="h-5 w-5 text-primary-600" />
                        <div>
                          <h3 className="font-medium text-gray-900">Azure AI Foundry</h3>
                          <p className="text-sm text-gray-600">Primary AI service connection</p>
                        </div>
                      </div>
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-success-100 text-success-800">
                        Connected
                      </span>
                    </div>
                  </div>

                  <div className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-3">
                        <Globe className="h-5 w-5 text-primary-600" />
                        <div>
                          <h3 className="font-medium text-gray-900">Bing Search</h3>
                          <p className="text-sm text-gray-600">Web search for RAG agent</p>
                        </div>
                      </div>
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-success-100 text-success-800">
                        Connected
                      </span>
                    </div>
                  </div>

                  <div className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-3">
                        <Database className="h-5 w-5 text-primary-600" />
                        <div>
                          <h3 className="font-medium text-gray-900">Knowledge Base</h3>
                          <p className="text-sm text-gray-600">FAQ and documentation storage</p>
                        </div>
                      </div>
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-success-100 text-success-800">
                        Connected
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Workflow Tab */}
            {activeTab === 'workflow' && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">Workflow Configuration</h2>
                  <p className="text-sm text-gray-600">Configure workflow behavior and thresholds</p>
                </div>

                <div className="space-y-6">
                  <div>
                    <label className="label">Confidence Threshold</label>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      defaultValue="70"
                      className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                    />
                    <div className="flex justify-between text-sm text-gray-500 mt-1">
                      <span>0%</span>
                      <span>70%</span>
                      <span>100%</span>
                    </div>
                    <p className="text-sm text-gray-600 mt-2">
                      Minimum confidence level required before sending automated responses
                    </p>
                  </div>

                  <div>
                    <label className="label">
                      <input type="checkbox" className="mr-2" defaultChecked />
                      Enable human review for complex queries
                    </label>
                  </div>

                  <div>
                    <label className="label">
                      <input type="checkbox" className="mr-2" defaultChecked />
                      Auto-escalate after 3 failed attempts
                    </label>
                  </div>

                  <div>
                    <label className="label">Response Timeout (minutes)</label>
                    <input
                      type="number"
                      defaultValue="10"
                      min="1"
                      max="60"
                      className="input w-32"
                    />
                  </div>
                </div>
              </div>
            )}

            {/* Security Tab */}
            {activeTab === 'security' && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">Security Settings</h2>
                  <p className="text-sm text-gray-600">Configure security and access controls</p>
                </div>

                <div className="space-y-6">
                  <div className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center space-x-3 mb-4">
                      <Shield className="h-5 w-5 text-success-600" />
                      <h3 className="font-medium text-gray-900">Authentication</h3>
                    </div>
                    <div className="space-y-3">
                      <div>
                        <label className="label">
                          <input type="checkbox" className="mr-2" defaultChecked />
                          Require Azure AD authentication
                        </label>
                      </div>
                      <div>
                        <label className="label">
                          <input type="checkbox" className="mr-2" />
                          Enable multi-factor authentication
                        </label>
                      </div>
                    </div>
                  </div>

                  <div className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center space-x-3 mb-4">
                      <AlertCircle className="h-5 w-5 text-warning-600" />
                      <h3 className="font-medium text-gray-900">Data Protection</h3>
                    </div>
                    <div className="space-y-3">
                      <div>
                        <label className="label">
                          <input type="checkbox" className="mr-2" defaultChecked />
                          Encrypt sensitive data at rest
                        </label>
                      </div>
                      <div>
                        <label className="label">
                          <input type="checkbox" className="mr-2" defaultChecked />
                          Enable audit logging
                        </label>
                      </div>
                      <div>
                        <label className="label">Data Retention (days)</label>
                        <input
                          type="number"
                          defaultValue="90"
                          min="1"
                          max="365"
                          className="input w-32"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

export default Settings