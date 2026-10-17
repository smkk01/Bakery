import { useEffect } from 'react'
import { Link } from 'react-router-dom'

interface SignOutToastProps {
  onDismiss: () => void
}

export function SignOutToast({ onDismiss }: SignOutToastProps) {
  useEffect(() => {
    const timeoutId = window.setTimeout(onDismiss, 4_000)
    return () => window.clearTimeout(timeoutId)
  }, [onDismiss])

  return (
    <div className="sign-out-toast alert alert-success shadow" role="status">
      <div>
        <strong>Signed out successfully.</strong>
        <Link className="alert-link ms-2" to="/login">Sign in again</Link>
      </div>
      <button aria-label="Dismiss signed out notification" className="btn-close" onClick={onDismiss} type="button" />
    </div>
  )
}
