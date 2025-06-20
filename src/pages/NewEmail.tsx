import React, { useState, useEffect } from 'react'
import { Send, Mail, User, FileText, Loader2, CheckCircle, AlertCircle, Wifi, WifiOff } from 'lucide-react'
import { apiService, EmailSubmissionData } from '../services/api'
import { useApi } from '../hooks/useApi'

const NewEmail: React.FC = () => {
  const [formData, setFormData] = useState<Omit<EmailSubmissionData, 'id'>>({
    from: '',
    to: 'support@company.com',
    subject: '',
    body: ''
  })
  
  const [connectionStatus, setConnectionStatus] = useState<'checking' | 'connected' | 'disconnected'>('checking')
  const { loading: isSubmitting, error: submitError, execute: submitEmail } = useApi()

  // Test backend connection on component mount
  useEffect(() => {
    const testConnection = async () => {
      try {
        const response = await apiService.testConnection()
        setConnectionStatus(response.success ? 'connected' : 'disconnected')
      } catch (error) {
        console.error('Connection test failed:', error)
        setConnectionStatus('disconnected')
      }
    }

    testConnection()
  }, [])

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    const emailData: EmailSubmissionData = {
      id: Date.now().toString(),
      ...formData
    }

    const response = await submitEmail(() => apiService.submitEmail(emailData))
    
    if (response.success) {
      // Reset form after successful submission
      setTimeout(() => {
        setFormData({
          from: '',
          to: 'support@company.com',
          subject: '',
          body: ''
        })
      }, 3000)
    }
  }

  const isFormValid = formData.from && formData.subject && formData.body

  return (
    <div className="max-w-4xl mx-auto space-y-8">
      {/* Header with Connection Status */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Process New Email</h1>
          <p className="mt-2 text-gray-600">
            Submit a customer email to start the AI-powered support workflow
          </p>
        </div>
        
        {/* Connection Status Indicator */}
        <div className="flex items-center space-x-2">
          {connectionStatus === 'checking' && (
            <>
              <Loader2 className="h-4 w-4 text-gray-500 animate-spin" />
              <span className="text-sm text-gray-500">Checking connection...</span>
            </>
          )}
          {connectionStatus === 'connected' && (
            <>
              <Wifi className="h-4 w-4 text-success-600" />
              <span className="text-sm text-success-600">Backend connected</span>
            </>
          )}
          {connectionStatus === 'disconnected' && (
            <>
              <WifiOff className="h-4 w-4 text-red-600" />
              <span className="text-sm text-red-600">Backend disconnected</span>
            </>
          )}
        </div>
      </div>

      {/* Connection Warning */}
      {connectionStatus === 'disconnected' && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <div className="flex items-center">
            <AlertCircle className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="font-medium text-red-800">Backend Connection Failed</h3>
              <p className="text-sm text-red-600 mt-1">
                Unable to connect to the SupportBuddy backend. Please ensure the .NET application is running on port 3978.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Email Form */}
        <div className="lg:col-span-2">
          <form onSubmit={handleSubmit} className="card space-y-6">
            <div className="flex items-center space-x-3 pb-4 border-b border-gray-200">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100">
                <Mail className="h-5 w-5 text-primary-600" />
              </div>
              <h2 className="text-xl font-semibold text-gray-900">Email Details</h2>
            </div>

            {/* From Field */}
            <div>
              <label htmlFor="from" className="label">
                <User className="h-4 w-4 inline mr-2" />
                From (Customer Email)
              </label>
              <input
                type="email"
                id="from"
                name="from"
                value={formData.from}
                onChange={handleInputChange}
                placeholder="customer@example.com"
                className="input"
                required
              />
            </div>

            {/* To Field */}
            <div>
              <label htmlFor="to" className="label">
                <Mail className="h-4 w-4 inline mr-2" />
                To (Support Email)
              </label>
              <input
                type="email"
                id="to"
                name="to"
                value={formData.to}
                onChange={handleInputChange}
                className="input"
                required
              />
            </div>

            {/* Subject Field */}
            <div>
              <label htmlFor="subject" className="label">
                <FileText className="h-4 w-4 inline mr-2" />
                Subject
              </label>
              <input
                type="text"
                id="subject"
                name="subject"
                value={formData.subject}
                onChange={handleInputChange}
                placeholder="Enter email subject..."
                className="input"
                required
              />
            </div>

            {/* Body Field */}
            <div>
              <label htmlFor="body" className="label">
                <FileText className="h-4 w-4 inline mr-2" />
                Email Body
              </label>
              <textarea
                id="body"
                name="body"
                value={formData.body}
                onChange={handleInputChange}
                placeholder="Enter the customer's email message..."
                rows={8}
                className="textarea"
                required
              />
            </div>

            {/* Status Messages */}
            {!isSubmitting && !submitError && connectionStatus === 'connected' && (
              <div className="flex items-center p-4 bg-success-50 border border-success-200 rounded-lg">
                <CheckCircle className="h-5 w-5 text-success-600 mr-3" />
                <div>
                  <p className="font-medium text-success-800">Ready to submit</p>
                  <p className="text-sm text-success-600">Backend is connected and ready to process emails.</p>
                </div>
              </div>
            )}

            {submitError && (
              <div className="flex items-center p-4 bg-red-50 border border-red-200 rounded-lg">
                <AlertCircle className="h-5 w-5 text-red-600 mr-3" />
                <div>
                  <p className="font-medium text-red-800">Submission failed</p>
                  <p className="text-sm text-red-600">{submitError}</p>
                </div>
              </div>
            )}

            {!isSubmitting && !submitError && connectionStatus === 'connected' && (
              <div className="flex items-center p-4 bg-success-50 border border-success-200 rounded-lg">
                <CheckCircle className="h-5 w-5 text-success-600 mr-3" />
                <div>
                  <p className="font-medium text-success-800">Email submitted successfully!</p>
                  <p className="text-sm text-success-600">The AI workflow has been started and will process this email.</p>
                </div>
              </div>
            )}

            {/* Submit Button */}
            <div className="flex justify-end pt-4 border-t border-gray-200">
              <button
                type="submit"
                disabled={!isFormValid || isSubmitting || connectionStatus !== 'connected'}
                className="btn-primary min-w-[140px]"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Processing...
                  </>
                ) : (
                  <>
                    <Send className="h-4 w-4 mr-2" />
                    Start Workflow
                  </>
                )}
              </button>
            </div>
          </form>
        </div>

        {/* Workflow Info */}
        <div className="space-y-6">
          <div className="card">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Workflow Process</h3>
            <div className="space-y-4">
              <div className="flex items-start space-x-3">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-100 text-xs font-medium text-primary-600">
                  1
                </div>
                <div>
                  <p className="font-medium text-gray-900">Triage</p>
                  <p className="text-sm text-gray-600">AI analyzes and categorizes the email</p>
                </div>
              </div>
              <div className="flex items-start space-x-3">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-100 text-xs font-medium text-primary-600">
                  2
                </div>
                <div>
                  <p className="font-medium text-gray-900">FAQ & RAG</p>
                  <p className="text-sm text-gray-600">Searches knowledge base and web for answers</p>
                </div>
              </div>
              <div className="flex items-start space-x-3">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-100 text-xs font-medium text-primary-600">
                  3
                </div>
                <div>
                  <p className="font-medium text-gray-900">Review</p>
                  <p className="text-sm text-gray-600">Human review if needed</p>
                </div>
              </div>
              <div className="flex items-start space-x-3">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-100 text-xs font-medium text-primary-600">
                  4
                </div>
                <div>
                  <p className="font-medium text-gray-900">Reply</p>
                  <p className="text-sm text-gray-600">Automated response sent to customer</p>
                </div>
              </div>
            </div>
          </div>

          <div className="card">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Sample Emails</h3>
            <div className="space-y-3">
              <button
                type="button"
                onClick={() => setFormData({
                  from: 'customer@example.com',
                  to: 'support@company.com',
                  subject: 'Where is my invoice?',
                  body: 'Hi team, I never received my invoice for April. Can you please send it to me? My account number is 12345.'
                })}
                className="w-full text-left p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors duration-200"
              >
                <p className="font-medium text-gray-900">Invoice Inquiry</p>
                <p className="text-sm text-gray-600">Customer asking about missing invoice</p>
              </button>
              <button
                type="button"
                onClick={() => setFormData({
                  from: 'user@company.com',
                  to: 'support@company.com',
                  subject: 'Product not working',
                  body: 'Hello, I purchased your software last week but it keeps crashing when I try to export files. I\'ve tried restarting but the issue persists. Please help!'
                })}
                className="w-full text-left p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors duration-200"
              >
                <p className="font-medium text-gray-900">Technical Support</p>
                <p className="text-sm text-gray-600">Software issue requiring assistance</p>
              </button>
            </div>
          </div>

          {/* Debug Info */}
          {process.env.NODE_ENV === 'development' && (
            <div className="card bg-gray-50">
              <h3 className="text-sm font-semibold text-gray-700 mb-2">Debug Info</h3>
              <div className="text-xs text-gray-600 space-y-1">
                <div>Backend URL: {window.location.origin}/api</div>
                <div>Connection: {connectionStatus}</div>
                <div>Form Valid: {isFormValid ? 'Yes' : 'No'}</div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default NewEmail