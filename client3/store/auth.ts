import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { getAuthToken, removeAuthToken, setAuthToken } from '@/lib/api'

interface AuthState {
  token: string | null
  isAuthenticated: boolean
  setAuth: (token: string) => void
  logout: () => void
  initialize: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      isAuthenticated: false,
      
      setAuth: (token: string) => {
        setAuthToken(token)
        set({ token, isAuthenticated: true })
      },
      
      logout: () => {
        removeAuthToken()
        set({ token: null, isAuthenticated: false })
      },
      
      initialize: () => {
        const token = getAuthToken()
        if (token) {
          set({ token, isAuthenticated: true })
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ token: state.token, isAuthenticated: state.isAuthenticated }),
    }
  )
)