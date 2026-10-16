import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import type { AuthResponse } from '../types/api'

interface AuthContextValue {
  user: AuthResponse | null
  signIn: (user: AuthResponse) => void
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [user, setUser] = useState<AuthResponse | null>(null)
  const value = useMemo(
    () => ({
      user,
      signIn: (authenticatedUser: AuthResponse) => setUser(authenticatedUser),
      signOut: () => setUser(null),
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.')
  }
  return context
}
