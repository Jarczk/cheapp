'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'

export default function MainPage() {
  const [user, setUser] = useState<{ name: string } | null>(null)
  const router = useRouter()

  // useEffect(() => {
  //   const storedUser = localStorage.getItem('user')
  //   if (storedUser) {
  //     setUser(JSON.parse(storedUser))
  //   } else {
  //     router.push('/login') // Zmiana przekierowania na /login
  //   }
  // }, [router])

  // const handleLogout = () => {
  //   localStorage.removeItem('user')
  //   router.push('/login')
  // }

  // if (!user) return null // lub spinner

  return (
    <div className="flex items-center justify-center min-h-screen bg-green-100 px-4">
      <div className="bg-white p-8 rounded-xl shadow-md text-center">
        <h1 className="text-3xl font-bold text-green-700 mb-2">
          Witaj, 👋
        </h1>
        <p className="text-gray-700 text-lg mb-6">Miło Cię widzieć w panelu!</p>
        <button
          className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 transition"
        >
          Wyloguj się
        </button>
      </div>
    </div>
  )
}
