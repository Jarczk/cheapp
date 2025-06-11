'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Search, Heart, User, LogOut, ShoppingBag, MessageCircle, Shield } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuthStore } from '@/store/auth'
import { useLogout, useSearchHistory, useCurrentUser } from '@/lib/hooks'
import { useState, useRef } from 'react'
import { motion } from 'framer-motion'

export function Navbar() {
  const router = useRouter()
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const logout = useLogout()
  const { data: currentUser } = useCurrentUser()
  const isAdmin = currentUser?.roles?.includes('Admin')
  const [searchQuery, setSearchQuery] = useState('')
  const [historyOpen, setHistoryOpen] = useState(false)
  const [highlightIndex, setHighlightIndex] = useState(-1)
  const inputRef = useRef<HTMLInputElement>(null)
  const { data: history = [] } = useSearchHistory(historyOpen)

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (searchQuery.trim()) {
      router.push(`/products?q=${encodeURIComponent(searchQuery.trim())}`)
    }
  }
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!historyOpen || history.length === 0) return
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setHighlightIndex((prev) => (prev + 1) % history.length)
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setHighlightIndex((prev) => (prev - 1 + history.length) % history.length)
    } else if (e.key === 'Enter' && highlightIndex >= 0) {
      e.preventDefault()
      const item = history[highlightIndex]
      setSearchQuery(item.query)
      router.push(`/products?q=${encodeURIComponent(item.query)}`)
      setHistoryOpen(false)
      setHighlightIndex(-1)
    }
  }

  const handleBlur = (e: React.FocusEvent<HTMLFormElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) {
      setHistoryOpen(false)
      setHighlightIndex(-1)
    }
  }

  const handleLogout = () => {
    logout.mutate()
  }

  return (
    <motion.nav 
      initial={{ y: -100 }}
      animate={{ y: 0 }}
      className="sticky top-0 z-50 w-full border-b bg-white/80 backdrop-blur-md shadow-sm"
    >
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          {/* Logo */}
          <Link href="/" className="flex items-center space-x-2">
            <ShoppingBag className="h-8 w-8 text-primary" />
            <span className="text-2xl font-bold text-primary">Cheapp</span>
          </Link>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="hidden md:flex flex-1 max-w-md mx-8" onBlur={handleBlur}>
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                ref={inputRef}
                type="search"
                placeholder="Search for products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setHistoryOpen(true)}
                onKeyDown={handleKeyDown}
                className="pl-10 pr-4"
              />
              {historyOpen && history.length > 0 && (
                <ul className="absolute z-50 mt-1 w-full rounded-md border bg-white shadow">
                  {history.map((item, idx) => (
                    <li
                      key={item.id}
                      onMouseDown={() => {
                        setSearchQuery(item.query)
                        router.push(`/products?q=${encodeURIComponent(item.query)}`)
                        setHistoryOpen(false)
                        setHighlightIndex(-1)
                      }}
                      className={`px-3 py-1 cursor-pointer ${highlightIndex === idx ? 'bg-muted' : ''}`}
                    >
                      {item.query}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </form>

          {/* Navigation Links */}
          <div className="flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <Link href="/favorites">
                  <Button variant="ghost" size="sm" className="hidden sm:flex">
                    <Heart className="h-4 w-4 mr-2" />
                    Favorites
                  </Button>
                </Link>
                {isAdmin && (
                  <Link href="/admin">
                    <Button variant="ghost" size="sm" className="hidden sm:flex">
                      <Shield className="h-4 w-4 mr-2" />
                      Admin
                    </Button>
                  </Link>
                )}
                <Link href="/profile">
                  <Button variant="ghost" size="sm" className="hidden sm:flex">
                    <User className="h-4 w-4 mr-2" />
                    Profile
                  </Button>
                </Link>
                <Button 
                  variant="ghost" 
                  size="sm" 
                  onClick={handleLogout}
                  disabled={logout.isPending}
                  className="hidden sm:flex"
                >
                  <LogOut className="h-4 w-4 mr-2" />
                  {logout.isPending ? 'Logging out...' : 'Logout'}
                </Button>
              </>
            ) : (
              <>
                <Link href="/auth/login">
                  <Button variant="ghost" size="sm">
                    Login
                  </Button>
                </Link>
                <Link href="/auth/register">
                  <Button size="sm">
                    Sign Up
                  </Button>
                </Link>
              </>
            )}

            {/* Mobile menu icons */}
            <div className="flex sm:hidden space-x-2">
              {isAuthenticated && (
                <>
                  <Link href="/favorites">
                    <Button variant="ghost" size="icon">
                      <Heart className="h-4 w-4" />
                    </Button>
                  </Link>
                  {isAdmin && (
                    <Link href="/admin">
                      <Button variant="ghost" size="icon">
                        <Shield className="h-4 w-4" />
                      </Button>
                    </Link>
                  )}
                  <Link href="/profile">
                    <Button variant="ghost" size="icon">
                      <User className="h-4 w-4" />
                    </Button>
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>

        {/* Mobile Search */}
        <div className="md:hidden pb-4">
          <form onSubmit={handleSearch} onBlur={handleBlur}>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                ref={inputRef}
                type="search"
                placeholder="Search for products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setHistoryOpen(true)}
                onKeyDown={handleKeyDown}
                className="pl-10 pr-4"
              />
              {historyOpen && history.length > 0 && (
                <ul className="absolute z-50 mt-1 w-full rounded-md border bg-white shadow">
                  {history.map((item, idx) => (
                    <li
                      key={item.id}
                      onMouseDown={() => {
                        setSearchQuery(item.query)
                        router.push(`/products?q=${encodeURIComponent(item.query)}`)
                        setHistoryOpen(false)
                        setHighlightIndex(-1)
                      }}
                      className={`px-3 py-1 cursor-pointer ${highlightIndex === idx ? 'bg-muted' : ''}`}
                    >
                      {item.query}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </form>
        </div>
      </div>
    </motion.nav>
  )
}