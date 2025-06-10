import { ApiError, AuthResponse, ChatRequest, ChatResponse, Favorite, LoginRequest, Product, RegisterRequest, SearchResponse, User } from '@/types/api'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5166/api'

class ApiClient {
  private baseURL: string

  constructor(baseURL: string) {
    this.baseURL = baseURL
  }

  private getAuthToken(): string | null {
    if (typeof window === 'undefined') return null
    return localStorage.getItem('access_token')
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseURL}${endpoint}`
    const token = this.getAuthToken()

    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
        ...options.headers,
      },
      ...options,
    }

    try {
      const response = await fetch(url, config)

      if (!response.ok) {
        let errorMessage = `HTTP ${response.status}`
        try {
          const errorData = await response.json()
          errorMessage = errorData.message || errorData.Message || errorMessage
        } catch {
          errorMessage = await response.text() || errorMessage
        }

        const error: ApiError = {
          message: errorMessage,
          statusCode: response.status,
        }
        throw error
      }

      // Handle empty responses
      const contentType = response.headers.get('content-type')
      if (!contentType?.includes('application/json')) {
        return {} as T
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && 'statusCode' in error) {
        throw error
      }
      throw {
        message: error instanceof Error ? error.message : 'Network error',
        statusCode: 0,
      } as ApiError
    }
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    return this.request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({
        Email: credentials.email,
        Password: credentials.password,
      }),
    })
  }

  async register(userData: RegisterRequest): Promise<void> {
    return this.request<void>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        Email: userData.email,
        Password: userData.password,
      }),
    })
  }

  async logout(): Promise<void> {
    return this.request<void>('/auth/logout', {
      method: 'POST',
    })
  }

  async getCurrentUser(): Promise<User> {
    return this.request<User>('/auth/whoami')
  }

  // Products endpoints
  async searchProducts(query: string, page: number = 1): Promise<Product[]> {
    return this.request<Product[]>(`/offers?q=${encodeURIComponent(query)}&page=${page}`)
  }

  async getProduct(id: string): Promise<Product> {
    return this.request<Product>(`/products/${id}`)
  }

  // Favorites endpoints
  async getFavorites(): Promise<Favorite[]> {
    console.log("getFavorites")
    return this.request<Favorite[]>('/favorites')
  }

  async getFavoritesAll(): Promise<Favorite[]> {
    console.log("getAll")
    return await this.request<Favorite[]>('/favorites/full')
  }
  
  async addToFavorites(productId: string): Promise<void> {
    return this.request<void>('/favorites', {
      method: 'POST',
      body: JSON.stringify({
        productId: productId
      }),
    })
  }

  async removeFromFavorites(productId: string): Promise<void> {
    return this.request<void>(`/favorites/${productId}`, {
      method: 'DELETE',
    })
  }

  // Chat endpoints
  /*async sendChatMessage(request: ChatRequest): Promise<ChatResponse> {
    return this.request<ChatResponse>('/assistant/ask', {
      method: 'POST',
      body: JSON.stringify({
        Prompt: request.message,
        SessionId: request.sessionId,
      }),
    })
  }*/
  async sendChatMessage(request: ChatRequest): Promise<ChatResponse> {
    return this.request<ChatResponse>('/assistant/ask', {
      method: 'POST',
      body: JSON.stringify({
        Prompt:    request.message,
        SessionId: request.sessionId ?? null,
        SystemPrompt: request.systemPrompt ?? null
      }),
    })
  }

}

export const apiClient = new ApiClient(API_BASE_URL)

// Helper functions for token management
export const setAuthToken = (token: string): void => {
  if (typeof window !== 'undefined') {
    localStorage.setItem('access_token', token)
  }
}

export const removeAuthToken = (): void => {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('access_token')
  }
}

export const getAuthToken = (): string | null => {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('access_token')
  }
  return null
}

