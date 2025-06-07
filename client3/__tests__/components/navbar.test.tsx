import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Navbar } from '@/components/navbar'

// Mock Next.js router
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
  }),
}))

// Mock auth store
jest.mock('@/store/auth', () => ({
  useAuthStore: () => ({
    isAuthenticated: false,
  }),
}))

// Mock hooks
jest.mock('@/lib/hooks', () => ({
  useLogout: () => ({
    mutate: jest.fn(),
    isPending: false,
  }),
}))

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

const renderWithProviders = (component: React.ReactElement) => {
  const testQueryClient = createTestQueryClient()
  return render(
    <QueryClientProvider client={testQueryClient}>
      {component}
    </QueryClientProvider>
  )
}

describe('Navbar', () => {
  it('renders the logo and brand name', () => {
    renderWithProviders(<Navbar />)
    
    expect(screen.getByText('Cheapp')).toBeInTheDocument()
  })

  it('renders search input', () => {
    renderWithProviders(<Navbar />)
    
    expect(screen.getByPlaceholderText('Search for products...')).toBeInTheDocument()
  })

  it('renders login and signup buttons when not authenticated', () => {
    renderWithProviders(<Navbar />)
    
    expect(screen.getByText('Login')).toBeInTheDocument()
    expect(screen.getByText('Sign Up')).toBeInTheDocument()
  })
})