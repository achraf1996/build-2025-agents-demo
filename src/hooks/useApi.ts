import { useState, useCallback } from 'react'
import { apiService, ApiResponse } from '../services/api'

export function useApi<T = any>() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [data, setData] = useState<T | null>(null)

  const execute = useCallback(async (apiCall: () => Promise<ApiResponse<T>>) => {
    setLoading(true)
    setError(null)
    
    try {
      const response = await apiCall()
      
      if (response.success) {
        setData(response.data || null)
        return response
      } else {
        setError(response.error || 'An error occurred')
        return response
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error'
      setError(errorMessage)
      return { success: false, error: errorMessage }
    } finally {
      setLoading(false)
    }
  }, [])

  const reset = useCallback(() => {
    setLoading(false)
    setError(null)
    setData(null)
  }, [])

  return {
    loading,
    error,
    data,
    execute,
    reset,
  }
}