'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import Image from 'next/image'
import { Heart, ExternalLink, Share2, ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { useProduct, useAddToFavorites, useRemoveFromFavorites, useFavorites } from '@/lib/hooks'
import { useAuthStore } from '@/store/auth'
import { formatPrice } from '@/lib/utils'
import { motion } from 'framer-motion'
import Link from 'next/link'

export default function ProductDetailPage() {
  const params = useParams()
  const productId = decodeURIComponent(params.id as string)
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const [imageError, setImageError] = useState(false)
  const { data: product, isLoading, error } = useProduct(productId)
  const { data: favorites } = useFavorites()
  const addToFavorites = useAddToFavorites()
  const removeFromFavorites = useRemoveFromFavorites()

  const isFavorited = favorites?.some(fav => fav.productId === productId) || false

  const handleFavoriteToggle = () => {
    if (!isAuthenticated) return

    if (isFavorited) {
      removeFromFavorites.mutate(productId)
    } else {
      addToFavorites.mutate(productId)
    }
  }

  const handleShare = async () => {
    if (navigator.share && product) {
      try {
        await navigator.share({
          title: product.title,
          text: `Check out this deal: ${product.title}`,
          url: window.location.href,
        })
      } catch (error) {
        // Fallback to clipboard
        navigator.clipboard.writeText(window.location.href)
      }
    } else {
      navigator.clipboard.writeText(window.location.href)
    }
  }

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <Skeleton className="aspect-square w-full" />
          <div className="space-y-4">
            <Skeleton className="h-8 w-3/4" />
            <Skeleton className="h-6 w-1/2" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-12 w-full" />
          </div>
        </div>
      </div>
    )
  }

  if (error || !product) {
    return (
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center py-20">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Product Not Found</h1>
          <p className="text-muted-foreground mb-8">
            The product you're looking for doesn't exist or has been removed.
          </p>
          <Link href="/products">
            <Button>
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Products
            </Button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6 }}
      >
        {/* Breadcrumb */}
        <div className="mb-6">
          <Link href="/products" className="text-primary hover:underline flex items-center">
            <ArrowLeft className="h-4 w-4 mr-1" />
            Back to Products
          </Link>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
          {/* Product Image */}
          <div className="space-y-4">
            <Card className="overflow-hidden">
              <div className="relative aspect-square">
                {!imageError ? (
                  <Image
                    src={product.imageUrl || `https://nftcalendar.io/storage/uploads/2022/02/21/image-not-found_0221202211372462137974b6c1a.png`}
                    alt={product.title}
                    fill
                    className="object-cover"
                    onError={() => setImageError(true)}
                  />
                ) : (
                  <div className="w-full h-full bg-muted flex items-center justify-center">
                    <span className="text-muted-foreground">No image available</span>
                  </div>
                )}
              </div>
            </Card>
          </div>

          {/* Product Details */}
          <div className="space-y-6">
            <div>
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h1 className="text-3xl font-bold text-gray-900 mb-2">
                    {product.title}
                  </h1>
                  <div className="flex items-center gap-2 mb-4">
                    <span className="bg-primary/10 text-primary text-sm px-3 py-1 rounded-full">
                      {product.marketplace}
                    </span>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" size="icon" onClick={handleShare}>
                    <Share2 className="h-4 w-4" />
                  </Button>
                  {isAuthenticated && (
                    <Button
                      variant="outline"
                      size="icon"
                      onClick={handleFavoriteToggle}
                      disabled={addToFavorites.isPending || removeFromFavorites.isPending}
                    >
                      <Heart 
                        className={`h-4 w-4 ${isFavorited ? 'fill-red-500 text-red-500' : ''}`} 
                      />
                    </Button>
                  )}
                </div>
              </div>

              <div className="text-4xl font-bold text-primary mb-6">
                {formatPrice(product.price, product.currency)}
              </div>

              {product.description && (
                <div className="mb-6">
                  <h3 className="font-semibold mb-2">Description</h3>
                  <p className="text-muted-foreground">{product.description}</p>
                </div>
              )}
            </div>

            {/* Actions */}
            <div className="space-y-4">
              <Button 
                size="lg" 
                className="w-full"
                onClick={() => window.open(product.url, '_blank', 'noopener,noreferrer')}
              >
                <ExternalLink className="h-5 w-5 mr-2" />
                View on {product.marketplace}
              </Button>

              {!isAuthenticated && (
                <p className="text-sm text-muted-foreground text-center">
                  <Link href="/auth/login" className="text-primary hover:underline">
                    Sign in
                  </Link>{' '}
                  to save this product to your favorites
                </p>
              )}
            </div>

            {/* Product Info */}
            <Card>
              <CardHeader>
                <CardTitle>Product Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Marketplace:</span>
                  <span className="font-medium">{product.marketplace}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Currency:</span>
                  <span className="font-medium">{product.currency}</span>
                </div>
                {product.gtin && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">GTIN:</span>
                    <span className="font-medium font-mono text-sm">{product.gtin}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Last Updated:</span>
                  <span className="font-medium">
                    {new Date(product.retrievedAt).toLocaleDateString()}
                  </span>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </motion.div>
    </div>
  )
}