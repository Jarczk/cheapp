export interface User {
  id: string
  email: string
  userName?: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
}

export interface AuthResponse {
  access_token: string
  refresh_token?: string
}

export interface Product {
  id: string
  title: string
  price: number
  currency: string
  marketplace: string
  url: string
  retrievedAt: string
  gtin?: string
  imageUrl?: string
  description?: string
}

export interface SearchResponse {
  products: Product[]
  total: number
  page: number
  pageSize: number
}

export interface Favorite {
  id: string
  productId: string
  userId: string
  createdAt: string
  product?: Product
}

export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  timestamp: string
}

export interface ChatRequest {
  message: string
  sessionId?: string
  systemPrompt?: string
}

export interface ChatResponse {
  answer: string
  sessionId: string
}

export interface ApiError {
  message: string
  statusCode: number
  details?: string[]
}