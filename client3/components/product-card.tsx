'use client'

import Image from 'next/image'
import Link from 'next/link'
import { Heart, ExternalLink } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter } from '@/components/ui/card'
import { Product } from '@/types/api'
import { formatPrice, truncateText } from '@/lib/utils'
import { useAddToFavorites, useRemoveFromFavorites, useFavorites } from '@/lib/hooks'
import { useAuthStore } from '@/store/auth'
import { motion } from 'framer-motion'
import { useState } from 'react'

interface ProductCardProps {
  product: Product
  index?: number
}

export function ProductCard({ product, index = 0 }: ProductCardProps) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const { data: favorites } = useFavorites()
  const addToFavorites = useAddToFavorites()
  const removeFromFavorites = useRemoveFromFavorites()
  const [imageError, setImageError] = useState(false)

  const isFavorited = favorites?.some(fav => fav.productId === product.id) || false

  const handleFavoriteToggle = (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    
    if (!isAuthenticated) return

    if (isFavorited) {
      removeFromFavorites.mutate(product.id)
    } else {
      addToFavorites.mutate(product.id)
    }
  }

  const handleExternalLink = (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    window.open(product.url, '_blank', 'noopener,noreferrer')
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.1 }}
      whileHover={{ y: -5 }}
      className="group"
    >
      <Link href={`/products/${product.id}`}>
        <Card className="h-full overflow-hidden bg-white/80 backdrop-blur-md hover:shadow-2xl transition-all duration-300">
          <div className="relative aspect-square overflow-hidden">
            {!imageError ? (
              <Image
                src={product.imageUrl || `https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400&h=400&fit=crop&crop=center`}
                alt={product.title}
                fill
                className="object-cover transition-transform duration-300 group-hover:scale-105"
                onError={() => setImageError(true)}
              />
            ) : (
              <div className="w-full h-full bg-muted flex items-center justify-center">
                <span className="text-muted-foreground text-sm">No image</span>
              </div>
            )}
            
            {/* Favorite Button */}
            {isAuthenticated && (
              <Button
                variant="ghost"
                size="icon"
                className="absolute top-2 right-2 bg-white/80 backdrop-blur-sm hover:bg-white/90"
                onClick={handleFavoriteToggle}
                disabled={addToFavorites.isPending || removeFromFavorites.isPending}
              >
                <Heart 
                  className={`h-4 w-4 ${isFavorited ? 'fill-red-500 text-red-500' : 'text-muted-foreground'}`} 
                />
              </Button>
            )}

            {/* Marketplace Badge */}
            <div className="absolute top-2 left-2">
              <span className="bg-primary/90 text-primary-foreground text-xs px-2 py-1 rounded-full">
                {product.marketplace}
              </span>
            </div>
          </div>

          <CardContent className="p-4">
            <h3 className="font-semibold text-sm mb-2 line-clamp-2">
              {truncateText(product.title, 60)}
            </h3>
            <div className="flex items-center justify-between">
              <span className="text-2xl font-bold text-primary">
                {formatPrice(product.price, product.currency)}
              </span>
            </div>
          </CardContent>

          <CardFooter className="p-4 pt-0">
            <Button
              variant="outline"
              size="sm"
              className="w-full"
              onClick={handleExternalLink}
            >
              <ExternalLink className="h-4 w-4 mr-2" />
              View Deal
            </Button>
          </CardFooter>
        </Card>
      </Link>
    </motion.div>
  )
}