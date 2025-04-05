'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const router = useRouter()

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!email || !password) {
      setError('Wypełnij wszystkie pola.')
      return
    }

    if (email === 'jojesadmin@admin.pl' && password === 'admin123') {
      router.push('/main')
    } else {
      setError('Nieprawidłowy email lub hasło.')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100 px-4">
      <div className="w-full max-w-md bg-white p-6 rounded-xl shadow-md">
        <h1 className="text-2xl font-bold mb-4 text-center">Zaloguj się</h1>
        <form onSubmit={handleLogin} className="space-y-4">
          <div>
            <label htmlFor="email" className="block text-sm font-medium mb-1">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full border rounded px-3 py-2 text-sm"
              placeholder="twój@email.com"
            />
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium mb-1">Hasło</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full border rounded px-3 py-2 text-sm"
              placeholder="••••••••"
            />
          </div>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 transition"
          >
            Zaloguj się
          </button>
        </form>
        <div className="text-center text-sm text-gray-600 mt-4">
          <a href="/register" className="text-blue-600 hover:underline">Zarejestruj się</a> ·{' '}
          <a href="/reset-password" className="text-blue-600 hover:underline">Zapomniałeś hasła?</a>
        </div>
      </div>
    </div>
  )
}
