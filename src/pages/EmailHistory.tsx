import React, { useState } from 'react'
import { 
  Search, 
  Filter, 
  Clock, 
  CheckCircle, 
  AlertCircle, 
  Mail,
  Eye,
  MoreHorizontal
} from 'lucide-react'

interface EmailRecord {
  id: string
  subject: string
  from: string
  to: string
  status: 'completed' | 'processing' | 'pending' | 'failed'
  timestamp: string
  processingTime: string
  questionsCount: number
  agentsUsed: string[]
}

const EmailHistory: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [selectedEmail, setSelectedEmail] = useState<EmailRecord | null>(null)

  // Mock data - in a real app, this would come from an API
  const emailHistory: EmailRecord[] = [
    {
      id: '1',
      subject: 'Invoice inquiry for April',
      from: 'customer@example.com',
      to: 'support@company.com',
      status: 'completed',
      timestamp: '2024-01-15 14:30:00',
      processingTime: '2m 15s',
      questionsCount: 2,
      agentsUsed: ['Triage', 'FAQ', 'Reply']
    },
    {
      id: '2',
      subject: 'Product support request',
      from: 'user@company.com',
      to: 'support@company.com',
      status: 'processing',
      timestamp: '2024-01-15 12:45:00',
      processingTime: '5m 32s',
      questionsCount: 3,
      agentsUsed: ['Triage', 'FAQ', 'RAG']
    },
    {
      id: '3',
      subject: 'Billing question about subscription',
      from: 'billing@client.com',
      to: 'support@company.com',
      status: 'pending',
      timestamp: '2024-01-15 11:20:00',
      processingTime: '1m 45s',
      questionsCount: 1,
      agentsUsed: ['Triage']
    },
    {
      id: '4',
      subject: 'Technical issue with API integration',
      from: 'dev@startup.com',
      to: 'support@company.com',
      status: 'completed',
      timestamp: '2024-01-15 09:15:00',
      processingTime: '8m 20s',
      questionsCount: 4,
      agentsUsed: ['Triage', 'FAQ', 'RAG', 'Reply']
    },
    {
      id: '5',
      subject: 'Account access problem',
      from: 'admin@enterprise.com',
      to: 'support@company.com',
      status: 'failed',
      timestamp: '2024-01-15 08:30:00',
      processingTime: '3m 10s',
      questionsCount: 2,
      agentsUsed: ['Triage', 'FAQ']
    }
  ]

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'completed':
        return <CheckCircle className="h-4 w-4 text-success-600" />
      case 'processing':
        return <Clock className="h-4 w-4 text-warning-600 animate-spin" />
      case 'pending':
        return <Clock className="h-4 w-4 text-gray-600" />
      case 'failed':
        return <AlertCircle className="h-4 w-4 text-red-600" />
      default:
        return <Clock className="h-4 w-4 text-gray-600" />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'bg-success-100 text-success-800'
      case 'processing':
        return 'bg-warning-100 text-warning-800'
      case 'pending':
        return 'bg-gray-100 text-gray-800'
      case 'failed':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const filteredEmails = emailHistory.filter(email => {
    const matchesSearch = email.subject.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         email.from.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesStatus = statusFilter === 'all' || email.status === statusFilter
    return matchesSearch && matchesStatus
  })

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Email History</h1>
        <p className="mt-2 text-gray-600">
          View and manage processed email workflows
        </p>
      </div>

      {/* Filters */}
      <div className="card">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search emails..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="input pl-10"
              />
            </div>
          </div>
          <div className="sm:w-48">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="input"
            >
              <option value="all">All Status</option>
              <option value="completed">Completed</option>
              <option value="processing">Processing</option>
              <option value="pending">Pending</option>
              <option value="failed">Failed</option>
            </select>
          </div>
        </div>
      </div>

      {/* Email List */}
      <div className="card">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Email
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Processing Time
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Questions
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Agents Used
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Timestamp
                </th>
                <th className="relative px-6 py-3">
                  <span className="sr-only">Actions</span>
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredEmails.map((email) => (
                <tr key={email.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <Mail className="h-4 w-4 text-gray-400 mr-3" />
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {email.subject}
                        </div>
                        <div className="text-sm text-gray-500">
                          From: {email.from}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      {getStatusIcon(email.status)}
                      <span className={`ml-2 px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(email.status)}`}>
                        {email.status}
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {email.processingTime}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {email.questionsCount}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex flex-wrap gap-1">
                      {email.agentsUsed.map((agent) => (
                        <span
                          key={agent}
                          className="px-2 py-1 text-xs font-medium bg-primary-100 text-primary-800 rounded"
                        >
                          {agent}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(email.timestamp).toLocaleString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => setSelectedEmail(email)}
                      className="text-primary-600 hover:text-primary-900"
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredEmails.length === 0 && (
          <div className="text-center py-12">
            <Mail className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No emails found</h3>
            <p className="mt-1 text-sm text-gray-500">
              {searchTerm || statusFilter !== 'all' 
                ? 'Try adjusting your search or filter criteria.'
                : 'No emails have been processed yet.'
              }
            </p>
          </div>
        )}
      </div>

      {/* Email Detail Modal */}
      {selectedEmail && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => setSelectedEmail(null)} />
            
            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-medium text-gray-900">Email Details</h3>
                  <button
                    onClick={() => setSelectedEmail(null)}
                    className="text-gray-400 hover:text-gray-600"
                  >
                    ×
                  </button>
                </div>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Subject</label>
                    <p className="mt-1 text-sm text-gray-900">{selectedEmail.subject}</p>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">From</label>
                      <p className="mt-1 text-sm text-gray-900">{selectedEmail.from}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">To</label>
                      <p className="mt-1 text-sm text-gray-900">{selectedEmail.to}</p>
                    </div>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Status</label>
                      <div className="mt-1 flex items-center">
                        {getStatusIcon(selectedEmail.status)}
                        <span className={`ml-2 px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(selectedEmail.status)}`}>
                          {selectedEmail.status}
                        </span>
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Processing Time</label>
                      <p className="mt-1 text-sm text-gray-900">{selectedEmail.processingTime}</p>
                    </div>
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Agents Used</label>
                    <div className="mt-1 flex flex-wrap gap-1">
                      {selectedEmail.agentsUsed.map((agent) => (
                        <span
                          key={agent}
                          className="px-2 py-1 text-xs font-medium bg-primary-100 text-primary-800 rounded"
                        >
                          {agent}
                        </span>
                      ))}
                    </div>
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Timestamp</label>
                    <p className="mt-1 text-sm text-gray-900">
                      {new Date(selectedEmail.timestamp).toLocaleString()}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default EmailHistory