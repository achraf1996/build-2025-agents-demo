import React from 'react'
import { Link } from 'react-router-dom'
import { 
  Mail, 
  Plus, 
  Clock, 
  CheckCircle, 
  AlertCircle,
  TrendingUp,
  Users,
  Zap
} from 'lucide-react'

const Dashboard: React.FC = () => {
  const stats = [
    {
      name: 'Total Emails Processed',
      value: '1,234',
      change: '+12%',
      changeType: 'positive',
      icon: Mail,
    },
    {
      name: 'Active Workflows',
      value: '23',
      change: '+4',
      changeType: 'positive',
      icon: Clock,
    },
    {
      name: 'Completed Today',
      value: '89',
      change: '+18%',
      changeType: 'positive',
      icon: CheckCircle,
    },
    {
      name: 'Pending Review',
      value: '12',
      change: '-2',
      changeType: 'negative',
      icon: AlertCircle,
    },
  ]

  const recentEmails = [
    {
      id: '1',
      subject: 'Invoice inquiry for April',
      from: 'customer@example.com',
      status: 'completed',
      timestamp: '2 hours ago',
    },
    {
      id: '2',
      subject: 'Product support request',
      from: 'user@company.com',
      status: 'processing',
      timestamp: '4 hours ago',
    },
    {
      id: '3',
      subject: 'Billing question',
      from: 'billing@client.com',
      status: 'pending',
      timestamp: '6 hours ago',
    },
  ]

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'bg-success-100 text-success-800'
      case 'processing':
        return 'bg-warning-100 text-warning-800'
      case 'pending':
        return 'bg-gray-100 text-gray-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-gray-600">
            Monitor your AI-powered email support workflow
          </p>
        </div>
        <Link to="/new-email" className="btn-primary">
          <Plus className="h-4 w-4 mr-2" />
          Process New Email
        </Link>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <div key={stat.name} className="card">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">{stat.name}</p>
                <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
              </div>
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-50">
                <stat.icon className="h-6 w-6 text-primary-600" />
              </div>
            </div>
            <div className="mt-4 flex items-center">
              <span className={`text-sm font-medium ${
                stat.changeType === 'positive' ? 'text-success-600' : 'text-red-600'
              }`}>
                {stat.change}
              </span>
              <span className="ml-2 text-sm text-gray-500">from last week</span>
            </div>
          </div>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Recent Emails */}
        <div className="lg:col-span-2">
          <div className="card">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900">Recent Emails</h2>
              <Link to="/history" className="text-sm text-primary-600 hover:text-primary-700">
                View all
              </Link>
            </div>
            <div className="space-y-4">
              {recentEmails.map((email) => (
                <div key={email.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors duration-200">
                  <div className="flex-1">
                    <h3 className="font-medium text-gray-900">{email.subject}</h3>
                    <p className="text-sm text-gray-600">From: {email.from}</p>
                  </div>
                  <div className="flex items-center space-x-3">
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(email.status)}`}>
                      {email.status}
                    </span>
                    <span className="text-sm text-gray-500">{email.timestamp}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Quick Actions & System Status */}
        <div className="space-y-6">
          {/* Quick Actions */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
            <div className="space-y-3">
              <Link to="/new-email" className="flex items-center p-3 bg-primary-50 rounded-lg hover:bg-primary-100 transition-colors duration-200">
                <Plus className="h-5 w-5 text-primary-600 mr-3" />
                <span className="font-medium text-primary-700">Process New Email</span>
              </Link>
              <Link to="/history" className="flex items-center p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors duration-200">
                <Clock className="h-5 w-5 text-gray-600 mr-3" />
                <span className="font-medium text-gray-700">View Workflow History</span>
              </Link>
              <Link to="/settings" className="flex items-center p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors duration-200">
                <Users className="h-5 w-5 text-gray-600 mr-3" />
                <span className="font-medium text-gray-700">Manage Agents</span>
              </Link>
            </div>
          </div>

          {/* System Status */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">System Status</h2>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Triage Agent</span>
                <div className="flex items-center">
                  <div className="h-2 w-2 bg-success-500 rounded-full mr-2"></div>
                  <span className="text-sm text-success-600">Online</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">FAQ Agent</span>
                <div className="flex items-center">
                  <div className="h-2 w-2 bg-success-500 rounded-full mr-2"></div>
                  <span className="text-sm text-success-600">Online</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">RAG Agent</span>
                <div className="flex items-center">
                  <div className="h-2 w-2 bg-success-500 rounded-full mr-2"></div>
                  <span className="text-sm text-success-600">Online</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Reply Agent</span>
                <div className="flex items-center">
                  <div className="h-2 w-2 bg-success-500 rounded-full mr-2"></div>
                  <span className="text-sm text-success-600">Online</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Dashboard