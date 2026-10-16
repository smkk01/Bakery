import { useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { register } from '../services/auth'

export function RegisterPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [password, setPassword] = useState('')
  const mutation = useMutation({
    mutationFn: () => register({ fullName, email, phone, password }),
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
          <p className="eyebrow">Join us</p>
          <h1 className="h2 mb-4">Create your account</h1>
          <form onSubmit={handleSubmit}>
            <label className="form-label" htmlFor="full-name">Full name</label>
            <input autoComplete="name" className="form-control mb-3" id="full-name" onChange={(event) => setFullName(event.target.value)} required value={fullName} />
            <label className="form-label" htmlFor="register-email">Email</label>
            <input autoComplete="email" className="form-control mb-3" id="register-email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />
            <label className="form-label" htmlFor="phone">Phone</label>
            <input autoComplete="tel" className="form-control mb-3" id="phone" onChange={(event) => setPhone(event.target.value)} required type="tel" value={phone} />
            <label className="form-label" htmlFor="register-password">Password</label>
            <input
              autoComplete="new-password"
              className="form-control mb-1"
              id="register-password"
              minLength={12}
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
            <p className="form-text mb-4">Use 12–128 characters, including at least one letter and one number.</p>
            <button className="btn btn-dark w-100" disabled={mutation.isPending} type="submit">
              {mutation.isPending ? 'Creating account…' : 'Create account'}
            </button>
          </form>
          {mutation.isError && (
            <div className="alert alert-danger mt-3 mb-0" role="alert">
              {mutation.error.message}
            </div>
          )}
          <p className="small text-secondary mt-4 mb-0">
            Already registered? <Link to="/login">Sign in</Link>
          </p>
        </div>
      </div>
    </section>
  )
}
