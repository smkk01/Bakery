import { useEffect } from 'react'
import { Link } from 'react-router-dom'

interface CartSuccessToastProps {
  onDismiss: () => void
  productName: string
}

export function CartSuccessToast({ onDismiss, productName }: CartSuccessToastProps) {
  useEffect(() => {
    const timeoutId = window.setTimeout(onDismiss, 4_000)
    return () => window.clearTimeout(timeoutId)
  }, [onDismiss])

  return (
    <div className="cart-success-toast alert alert-success shadow" role="status">
      <div>
        <strong>{productName}</strong> was added to your basket.
        <Link className="alert-link ms-2" to="/cart">View basket</Link>
      </div>
      <button aria-label="Dismiss added to basket notification" className="btn-close" onClick={onDismiss} type="button" />
    </div>
  )
}
