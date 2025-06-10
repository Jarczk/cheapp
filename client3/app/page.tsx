'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Search, TrendingUp, Shield, Zap } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { motion } from 'framer-motion'

export default function HomePage() {
  const router = useRouter()
  const [searchQuery, setSearchQuery] = useState('')
  
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (searchQuery.trim()) {
      router.push(`/products?q=${encodeURIComponent(searchQuery.trim())}`)
    }
  }

  const features = [
    {
      icon: Search,
      title: 'Smart Search',
      description: 'Find products across multiple marketplaces with our intelligent search engine.',
    },
    {
      icon: TrendingUp,
      title: 'Price Comparison',
      description: 'Compare prices from different sellers to ensure you get the best deal.',
    },
    {
      icon: Shield,
      title: 'Trusted Sources',
      description: 'We only aggregate from verified and trusted marketplace partners.',
    },
    {
      icon: Zap,
      title: 'Real-time Updates',
      description: 'Get the latest prices and availability information in real-time.',
    },
  ]

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8">
      {/* Hero Section */}
      <section className="py-20 text-center">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
        >
          <h1 className="text-4xl sm:text-6xl font-bold text-gray-900 mb-6">
            Find the{' '}
            <span className="text-primary">Best Deals</span>
            <br />
            Across All Marketplaces
          </h1>
          <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
            Compare prices, discover discounts, and save money on your favorite products 
            with our comprehensive price comparison platform.
          </p>
        </motion.div>

        {/* Search Bar */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.2 }}
          className="max-w-2xl mx-auto mb-12"
        >
          <form onSubmit={handleSearch} className="flex gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-muted-foreground" />
              <Input
                type="search"
                placeholder="Search for products, brands, or categories..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-12 pr-4 h-14 text-lg bg-white/80 backdrop-blur-md border-2 border-primary/20 focus:border-primary"
              />
            </div>
            <Button type="submit" size="lg" className="h-14 px-8">
              Search
            </Button>
          </form>
        </motion.div>

        {/* Popular Searches */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.6, delay: 0.4 }}
          className="flex flex-wrap justify-center gap-2 mb-16"
        >
          <span className="text-sm text-muted-foreground mr-2">Popular searches:</span>
          {['iPhone', 'Laptop', 'Fiat'].map((term) => (
            <Button
              key={term}
              variant="outline"
              size="sm"
              onClick={() => {
                setSearchQuery(term)
                router.push(`/products?q=${encodeURIComponent(term)}`)
              }}
              className="text-xs"
            >
              {term}
            </Button>
          ))}
        </motion.div>
      </section>

      {/* Features Section */}
      <section className="py-20">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.6 }}
          className="text-center mb-16"
        >
          <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
            Why Choose Cheapp?
          </h2>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            We make it easy to find the best deals and save money on your purchases.
          </p>
        </motion.div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          {features.map((feature, index) => (
            <motion.div
              key={feature.title}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.8 + index * 0.1 }}
            >
              <Card className="h-full text-center bg-white/80 backdrop-blur-md hover:shadow-xl transition-all duration-300">
                <CardHeader>
                  <div className="mx-auto mb-4 p-3 bg-primary/10 rounded-full w-fit">
                    <feature.icon className="h-8 w-8 text-primary" />
                  </div>
                  <CardTitle className="text-xl">{feature.title}</CardTitle>
                </CardHeader>
                <CardContent>
                  <CardDescription className="text-base">
                    {feature.description}
                  </CardDescription>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 text-center">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 1.2 }}
          className="bg-primary/5 rounded-3xl p-12"
        >
          <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
            Ready to Start Saving?
          </h2>
          <p className="text-lg text-gray-600 mb-8 max-w-2xl mx-auto">
            Join thousands of smart shoppers who use Cheapp to find the best deals every day.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" onClick={() => router.push('/auth/register')}>
              Get Started Free
            </Button>
            <Button variant="outline" size="lg" onClick={() => router.push('/products')}>
              Browse Products
            </Button>
          </div>
        </motion.div>
      </section>
    </div>
  )
}