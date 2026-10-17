import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import type { AuthResponse } from '../types/api'

const authStorageKey = 'bakery.auth'

interface AuthContextValue {
  user: AuthResponse | null
  signIn: (user: AuthResponse) => void
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [user, setUser] = useState<AuthResponse | null>(getStoredUser)
  const value = useMemo(
    () => ({
      user,
      signIn: (authenticatedUser: AuthResponse) => {
        sessionStorage.setItem(authStorageKey, JSON.stringify(authenticatedUser))
        setUser(authenticatedUser)
      },
      signOut: () => {
        sessionStorage.removeItem(authStorageKey)
        setUser(null)
      },
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

function getStoredUser(): AuthResponse | null {
  const storedUser = sessionStorage.getItem(authStorageKey)
  if (!storedUser) {
    return null
  }

  try {
    const user = JSON.parse(storedUser) as AuthResponse
    if (!user.accessToken || Number.isNaN(Date.parse(user.expiresAtUtc)) || Date.parse(user.expiresAtUtc) <= Date.now()) {
      sessionStorage.removeItem(authStorageKey)
      return null
    }

    return user
  } catch {
    sessionStorage.removeItem(authStorageKey)
    return null
  }
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.')
  }
  return context
}
