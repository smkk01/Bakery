import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { SignOutToast } from './SignOutToast'
import { useAuth } from '../contexts/AuthContext'

export function Navbar() {
  const { user, signOut } = useAuth()
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isSignOutNotificationVisible, setIsSignOutNotificationVisible] = useState(false)

  return (
    <header className="border-bottom bg-white">
      <nav className="navbar navbar-expand-lg container py-3" aria-label="Main navigation">
        <Link className="navbar-brand brand-mark" to="/">crumb &amp; crumb</Link>
        <button
          className="navbar-toggler"
          type="button"
          aria-controls="main-navigation"
          aria-expanded={isMenuOpen}
          aria-label="Toggle navigation"
          onClick={() => setIsMenuOpen((open) => !open)}
        >
          <span className="navbar-toggler-icon" />
        </button>
        <div className={`collapse navbar-collapse${isMenuOpen ? ' show' : ''}`} id="main-navigation">
          <div className="navbar-nav ms-auto align-items-lg-center gap-lg-3">
            <NavLink className="nav-link" onClick={() => setIsMenuOpen(false)} to="/products">Products</NavLink>
            {user?.role === 'Admin' && (
              <NavLink className="nav-link" onClick={() => setIsMenuOpen(false)} to="/admin">Admin</NavLink>
            )}
            {!user && (
              <NavLink className="btn btn-dark btn-sm px-3" onClick={() => setIsMenuOpen(false)} to="/login">Sign in</NavLink>
            )}
          </div>
        </div>
        {user && (
          <button
            aria-label={`Sign out ${user.fullName}`}
            className="btn btn-outline-dark btn-sm ms-2"
            onClick={() => {
              signOut()
              setIsMenuOpen(false)
              setIsSignOutNotificationVisible(true)
            }}
            type="button"
          >
            Sign out
          </button>
        )}
        <NavLink
          aria-label="View shopping cart"
          className={({ isActive }) => `btn btn-sm ms-2${isActive ? ' btn-dark' : ' btn-outline-dark'}`}
          onClick={() => setIsMenuOpen(false)}
          to="/cart"
        >
          Cart
        </NavLink>
      </nav>
      {isSignOutNotificationVisible && (
        <SignOutToast onDismiss={() => setIsSignOutNotificationVisible(false)} />
      )}
    </header>
  )
}
