'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'

export default function MainPage() {
  const [userEmail, setUserEmail] = useState<string | null>(null);
  const router = useRouter()
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  
  useEffect(() => {
    setUserEmail(localStorage.getItem('userEmail'));
  }, []);

  const logout = async () => {
    setIsLoggingOut(true)
    try {
      await fetch('http://localhost:5166/api/auth/logout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      })
     
      localStorage.removeItem('token')
       
      router.push('/login')
    } catch (err) {
      console.error('Logout error:', err)
      localStorage.removeItem('token')
      router.push('/login')
    } finally {
      setIsLoggingOut(false)
    }
  }

  return (
    <div className="flex items-center justify-center min-h-screen bg-green-100 px-4">
      <div className="bg-white p-8 rounded-xl shadow-md text-center">
        <h1 className="text-3xl font-bold text-green-700 mb-2">
          Witaj, {userEmail || 'Użytkowniku'} 👋
        </h1>
        <p className="text-gray-700 text-lg mb-6">Miło Cię widzieć w panelu!</p>
        <button
          onClick={logout}
          disabled={isLoggingOut}
          className={`px-4 py-2 rounded transition ${
            isLoggingOut 
              ? 'bg-gray-400 cursor-not-allowed' 
              : 'bg-red-600 hover:bg-red-700 text-white'
          }`}
        >
          {isLoggingOut ? 'Wylogowywanie...' : 'Wyloguj się'}
        </button>
      </div>
    </div>
  )
}