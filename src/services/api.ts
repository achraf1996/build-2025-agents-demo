// API service for SupportBuddy backend communication
const API_BASE_URL = '/api'

export interface EmailSubmissionData {
  id: string
  from: string
  to: string
  subject: string
  body: string
}

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  error?: string
}

class ApiService {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          'Content-Type': 'application/json',
          ...options.headers,
        },
        ...options,
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }

      // Check if response has content
      const contentType = response.headers.get('content-type')
      let data = null
      
      if (contentType && contentType.includes('application/json')) {
        data = await response.json()
      } else {
        // For endpoints that don't return JSON (like the current /api/new-email)
        data = await response.text()
      }

      return {
        success: true,
        data,
      }
    } catch (error) {
      console.error('API request failed:', error)
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error occurred',
      }
    }
  }

  async submitEmail(emailData: EmailSubmissionData): Promise<ApiResponse> {
    return this.request('/new-email', {
      method: 'POST',
      body: JSON.stringify(emailData),
    })
  }

  async getEmailHistory(): Promise<ApiResponse<any[]>> {
    return this.request('/emails')
  }

  async getSystemStatus(): Promise<ApiResponse> {
    return this.request('/status')
  }

  // Test connection to backend
  async testConnection(): Promise<ApiResponse> {
    return this.request('/')
  }
}

export const apiService = new ApiService()