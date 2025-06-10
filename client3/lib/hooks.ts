import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from './api'
import { ChatRequest, Favorite, LoginRequest, Product, RegisterRequest } from '@/types/api'
import { useAuthStore } from '@/store/auth'

// Auth hooks
export const useLogin = () => {
  const setAuth = useAuthStore((state) => state.setAuth)
  
  return useMutation({
    mutationFn: (credentials: LoginRequest) => apiClient.login(credentials),
    onSuccess: (data) => {
      setAuth(data.access_token)
    },
  })
}

export const useRegister = () => {
  return useMutation({
    mutationFn: (userData: RegisterRequest) => apiClient.register(userData),
  })
}

export const useLogout = () => {
  const logout = useAuthStore((state) => state.logout)
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: () => apiClient.logout(),
    onSuccess: () => {
      logout()
      queryClient.clear()
    },
    onError: () => {
      // Even if the API call fails, we should still log out locally
      logout()
      queryClient.clear()
    },
  })
}

export const useCurrentUser = () => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: () => apiClient.getCurrentUser(),
    enabled: isAuthenticated,
    retry: false,
  })
}

// Products hooks
export const useSearchProducts = (query: string, enabled: boolean = true) => {
  return useQuery({
    queryKey: ['products', 'search', query],
    queryFn: () => apiClient.searchProducts(query),
    enabled: enabled && query.length > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useProduct = (id: string) => {
  return useQuery({
    queryKey: ['products', id],
    queryFn: () => apiClient.getProduct(id),
    enabled: !!id,
  })
}

export const useSearchHistory = (enabled: boolean = true, limit: number = 10) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  return useQuery({
    queryKey: ['searchHistory', limit],
    queryFn: () => apiClient.getSearchHistory(limit),
    enabled: enabled && isAuthenticated,
    staleTime: 0,
  })
}

// Favorites hooks
export const useFavorites = () => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  
  return useQuery({
    queryKey: ['favorites'],
    queryFn: () => apiClient.getFavorites(),
    enabled: isAuthenticated,
  })
}

export const useFavoritesAll = () => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  return useQuery({
    queryKey: ['favorites/full'],
    queryFn: () => apiClient.getFavoritesAll(),
    enabled: isAuthenticated,
    staleTime: 0,
  })
}

export const useAddToFavorites = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (productId: string) => apiClient.addToFavorites(productId),
    onMutate: async (productId: string) => {
      await queryClient.cancelQueries({ queryKey: ['favorites'] })

      const previousFavorites = queryClient.getQueryData<Favorite[]>(['favorites'])

      const optimisticFavorite: Favorite = {
        id: `temp-${productId}`,
        productId,
        userId: '',
        createdAt: new Date().toISOString(),
      }

      queryClient.setQueryData<Favorite[]>(['favorites'], (old = []) => [
        ...old,
        optimisticFavorite,
      ])

      return { previousFavorites }
    },
    onError: (_err, _productId, context) => {
      if (context?.previousFavorites) {
        queryClient.setQueryData(['favorites'], context.previousFavorites)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] })
      queryClient.invalidateQueries({ queryKey: ['favorites/full'] })
    },
  })
}

export const useRemoveFromFavorites = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (productId: string) => apiClient.removeFromFavorites(productId),
    onMutate: async (productId: string) => {
      await queryClient.cancelQueries({ queryKey: ['favorites'] })
      await queryClient.cancelQueries({ queryKey: ['favorites/full'] })

      const previousFavorites = queryClient.getQueryData<Favorite[]>(['favorites'])
      const previousFull = queryClient.getQueryData<Favorite[]>(['favorites/full'])

      queryClient.setQueryData<Favorite[]>(['favorites'], (old = []) =>
        old.filter((fav) => fav.productId !== productId)
      )
      queryClient.setQueryData<Favorite[]>(['favorites/full'], (old = []) =>
        old.filter((fav) => fav.productId !== productId)
      )

      return { previousFavorites, previousFull }
    },
    onError: (_err, _productId, context) => {
      if (context?.previousFavorites) {
        queryClient.setQueryData(['favorites'], context.previousFavorites)
      }
      if (context?.previousFull) {
        queryClient.setQueryData(['favorites/full'], context.previousFull)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] })
      queryClient.invalidateQueries({ queryKey: ['favorites/full'] })
    },
  })
}

// Chat hooks
/*export const useSendChatMessage = () => {
  return useMutation({
    mutationFn: (request: ChatRequest) => apiClient.sendChatMessage(request),
  })
}*/
/*export const useSendChatMessage = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (msg: ChatRequest) => apiClient.sendChatMessage(msg),
  })
}*/

export const useSendChatMessage = () =>
  useMutation({
    mutationFn: (payload: ChatRequest) => apiClient.sendChatMessage(payload),
  })

export const useChatHistory = (sessionId?: string) =>
  useQuery({
    queryKey: ['chat', sessionId],
    queryFn: () => apiClient.getChatHistory(sessionId!),
    enabled: !!sessionId,           // run only if we already have an id
    staleTime: 0,
  })
