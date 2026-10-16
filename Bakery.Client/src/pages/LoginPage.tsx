import { useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { login } from '../services/auth'

export function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const mutation = useMutation({
    mutationFn: () => login({ email, password }),
    onSuccess: (user) => {
      signIn(user)
      navigate('/')
    },
  })

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    mutation.mutate()
  }

  return (
    <section className="container py-5">
      <div className="auth-card card border-0 shadow-sm mx-auto">
        <div className="card-body p-4 p-md-5">
          <p className="eyebrow">Welcome back</p>
          <h1 className="h2 mb-4">Sign in to the bakery</h1>
          <form onSubmit={handleSubmit}>
            <label className="form-label" htmlFor="email">Email</label>
            <input
              autoComplete="email"
              className="form-control mb-3"
              id="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />
            <label className="form-label" htmlFor="password">Password</label>
            <input
              autoComplete="current-password"
              className="form-control mb-4"
              id="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
            <button className="btn btn-dark w-100" disabled={mutation.isPending} type="submit">
              {mutation.isPending ? 'Signing in…' : 'Sign in'}
            </button>
          </form>
          {mutation.isError && (
            <div className="alert alert-danger mt-3 mb-0" role="alert">
              {mutation.error.message}
            </div>
          )}
          <p className="small text-secondary mt-4 mb-0">
            New here? <Link to="/register">Create a customer account</Link>
          </p>
        </div>
      </div>
    </section>
  )
}
