/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    appDir: true,
  },
  images: {
  remotePatterns: [
    {
      protocol: 'http',
      hostname: '**',
    },
    {
      protocol: 'https',
      hostname: '**',
    },
  ],
  },
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5166/api',
  },
}

module.exports = nextConfig