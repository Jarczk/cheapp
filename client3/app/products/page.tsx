'use client'

import { useState, useEffect } from 'react'
import { useSearchParams } from 'next/navigation'
import { Search, Filter } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { ProductGrid } from '@/components/product-grid'
import { useSearchProducts } from '@/lib/hooks'
import { debounce } from '@/lib/utils'
import { motion } from 'framer-motion'

export default function ProductsPage() {
  const searchParams = useSearchParams()
  const initialQuery = searchParams.get('q') || ''
  const [searchQuery, setSearchQuery] = useState(initialQuery)
  const [debouncedQuery, setDebouncedQuery] = useState(initialQuery)

  const { data: products = [], isLoading, error } = useSearchProducts(debouncedQuery, debouncedQuery.length > 0)

  // Debounce search query
  useEffect(() => {
    const debouncedSearch = debounce((query: string) => {
      setDebouncedQuery(query)
    }, 500)

    debouncedSearch(searchQuery)
  }, [searchQuery])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setDebouncedQuery(searchQuery)
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
          <h1 className="text-3xl font-bold text-gray-900 mb-4">
            {debouncedQuery ? `Search Results for "${debouncedQuery}"` : 'Search Products'}
          </h1>
          
          {/* Search Bar */}
          <form onSubmit={handleSearch} className="flex gap-4 mb-6">
            <div className="relative flex-1 max-w-2xl">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                type="search"
                placeholder="Search for products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 pr-4"
              />
            </div>
            <Button type="submit">Search</Button>
          </form>

          {/* Results Info */}
          {debouncedQuery && !isLoading && (
            <div className="flex items-center justify-between mb-6">
              <p className="text-muted-foreground">
                {products.length} {products.length === 1 ? 'result' : 'results'} found
              </p>
              <Button variant="outline" size="sm">
                <Filter className="h-4 w-4 mr-2" />
                Filters
              </Button>
            </div>
          )}
        </div>

        {/* Error State */}
        {error && (
          <div className="text-center py-12">
            <div className="text-destructive text-lg mb-2">
              Failed to load products
            </div>
            <p className="text-sm text-muted-foreground">
              {error.message}
            </p>
          </div>
        )}

        {/* Products Grid */}
        {!error && (
          <ProductGrid products={products} isLoading={isLoading} />
        )}

        {/* Empty State for no query */}
        {!debouncedQuery && !isLoading && (
          <div className="text-center py-20">
            <Search className="h-16 w-16 mx-auto mb-4 text-muted-foreground opacity-50" />
            <h2 className="text-2xl font-semibold text-gray-900 mb-2">
              Start Your Search
            </h2>
            <p className="text-muted-foreground mb-8 max-w-md mx-auto">
              Enter a product name, brand, or category to find the best deals across multiple marketplaces.
            </p>
            <div className="flex flex-wrap justify-center gap-2">
              <span className="text-sm text-muted-foreground mr-2">Try searching for:</span>
              {['iPhone', 'Laptop', 'Headphones', 'Gaming Chair'].map((term) => (
                <Button
                  key={term}
                  variant="outline"
                  size="sm"
                  onClick={() => setSearchQuery(term)}
                  className="text-xs"
                >
                  {term}
                </Button>
              ))}
            </div>
          </div>
        )}
      </motion.div>
    </div>
  )
}