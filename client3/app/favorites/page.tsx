'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Heart, ShoppingBag } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { ProductGrid } from '@/components/product-grid'
import { useFavoritesAll } from '@/lib/hooks'
import { useAuthStore } from '@/store/auth'
import { motion } from 'framer-motion'
import Link from 'next/link'

export default function FavoritesPage() {
  const router = useRouter()
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const { data: favorites = [], isLoading, error } = useFavoritesAll()

  console.log(favorites)
  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/auth/login')
    }
  }, [isAuthenticated, router])

  if (!isAuthenticated) {
    return null // Will redirect
  }

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6 }}
      >
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center gap-3 mb-4">
            <Heart className="h-8 w-8 text-primary" />
            <h1 className="text-3xl font-bold text-gray-900">My Favorites</h1>
          </div>
          <p className="text-muted-foreground">
            Keep track of products you're interested in and get notified of price changes.
          </p>
        </div>

        {/* Error State */}
        {error && (
          <div className="text-center py-12">
            <div className="text-destructive text-lg mb-2">
              Failed to load favorites
            </div>
            <p className="text-sm text-muted-foreground">
              {error.message}
            </p>
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <ProductGrid products={[]} isLoading={true} />
        )}

        {/* Empty State */}
        {!isLoading && !error && favorites.length === 0 && (
          <div className="text-center py-20">
            <Heart className="h-16 w-16 mx-auto mb-4 text-muted-foreground opacity-50" />
            <h2 className="text-2xl font-semibold text-gray-900 mb-2">
              No favorites yet
            </h2>
            <p className="text-muted-foreground mb-8 max-w-md mx-auto">
              Start adding products to your favorites to keep track of deals and price changes.
            </p>
            <Link href="/products">
              <Button>
                <ShoppingBag className="h-4 w-4 mr-2" />
                Browse Products
              </Button>
            </Link>
          </div>
        )}

        {/* Favorites Grid */}
        {!isLoading && !error && favorites.length > 0 && (
          <div>
            <div className="mb-6">
              <p className="text-muted-foreground">
                {favorites.length} {favorites.length === 1 ? 'favorite' : 'favorites'}
              </p>
            </div>
            <ProductGrid products={favorites} />
          </div>
        )}
      </motion.div>
    </div>
  )
}