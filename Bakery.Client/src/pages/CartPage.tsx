import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { useAuth } from '../contexts/AuthContext'
import { getCart, removeCartItem, updateCartItem } from '../services/cart'
import { getApiErrorMessage } from '../utils/apiError'

export function CartPage() {
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const cartKey = ['cart', user?.userId]
  const cartQuery = useQuery({
    queryKey: cartKey,
    queryFn: () => getCart(user!.userId, user!.accessToken),
    enabled: user?.role === 'Customer',
  })
  const updateMutation = useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) => {
      if (!user || user.role !== 'Customer') {
        throw new Error('Sign in with a customer account to update your basket.')
      }
      return updateCartItem(user.userId, productId, quantity, user.accessToken)
    },
    onSuccess: (cart) => queryClient.setQueryData(cartKey, cart),
  })
  const removeMutation = useMutation({
    mutationFn: (productId: string) => {
      if (!user || user.role !== 'Customer') {
        throw new Error('Sign in with a customer account to update your basket.')
      }
      return removeCartItem(user.userId, productId, user.accessToken)
    },
    onSuccess: (cart) => queryClient.setQueryData(cartKey, cart),
  })
  const mutationError = updateMutation.error ?? removeMutation.error

  if (!user) {
    return <div className="container py-5"><h1 className="h2">Your basket</h1><p><Link to="/login">Sign in</Link> with your customer account to view your basket.</p></div>
  }

  if (user.role !== 'Customer') {
    return <div className="container py-5"><h1 className="h2">Your basket</h1><p>Basket access is available to customer accounts.</p></div>
  }

  return (
    <section className="container py-5">
      <div className="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-4">
        <div>
          <p className="eyebrow mb-2">The good things</p>
          <h1 className="display-6 fw-semibold mb-0">Your basket</h1>
        </div>
        {cartQuery.data && cartQuery.data.items.length > 0 && (
          <Link className="btn btn-outline-dark" to="/products">Continue shopping</Link>
        )}
      </div>
      {cartQuery.isPending && <LoadingSpinner label="Loading your basket…" />}
      {cartQuery.isError && (
        <div className="alert alert-danger" role="alert">
          {getApiErrorMessage(cartQuery.error, 'Could not load your basket.')}
        </div>
      )}
      {cartQuery.data?.items.length === 0 && (
        <div className="empty-state rounded-4 p-5 text-center">
          <span className="display-5" aria-hidden="true">🧺</span>
          <h2 className="h4 mt-3">Your basket is empty</h2>
          <p className="text-secondary">There’s always room for something fresh.</p>
          <Link className="btn btn-dark px-4" to="/products">Browse products</Link>
        </div>
      )}
      {mutationError && (
        <div className="alert alert-danger" role="alert">
          {getApiErrorMessage(mutationError, 'Your basket could not be updated.')}
        </div>
      )}
      {cartQuery.data && cartQuery.data.items.length > 0 && (
        <div className="row g-4 align-items-start">
          <div className="col-lg-8">
            <div className="card border-0 shadow-sm">
              <ul className="list-group list-group-flush">
                {cartQuery.data.items.map((item) => (
                  <li className="list-group-item cart-line-item p-3 p-md-4" key={item.productId}>
                    <div className="d-flex flex-column flex-sm-row justify-content-between gap-3">
                      <div>
                        <h2 className="h5 mb-1">{item.productName}</h2>
                        <span className="small text-secondary">${item.unitPrice.toFixed(2)} each</span>
                      </div>
                      <div className="d-flex flex-wrap align-items-center justify-content-between justify-content-sm-end gap-3">
                        <div aria-label={`Quantity for ${item.productName}`} className="input-group cart-quantity-control">
                          <button
                            aria-label={`Decrease ${item.productName} quantity`}
                            className="btn btn-outline-secondary"
                            disabled={updateMutation.isPending || removeMutation.isPending || item.quantity <= 1}
                            onClick={() => updateMutation.mutate({
                              productId: item.productId,
                              quantity: item.quantity - 1,
                            })}
                            type="button"
                          >
                            −
                          </button>
                          <input
                            aria-label={`${item.productName} quantity`}
                            className="form-control text-center"
                            max={10000}
                            min={1}
                            type="number"
                            readOnly
                            value={item.quantity}
                          />
                          <button
                            aria-label={`Increase ${item.productName} quantity`}
                            className="btn btn-outline-secondary"
                            disabled={updateMutation.isPending || removeMutation.isPending || item.quantity >= 10000}
                            onClick={() => updateMutation.mutate({
                              productId: item.productId,
                              quantity: item.quantity + 1,
                            })}
                            type="button"
                          >
                            +
                          </button>
                        </div>
                        <span className="fw-semibold cart-line-subtotal">${item.subtotal.toFixed(2)}</span>
                        <button
                          className="btn btn-link btn-sm text-danger p-0"
                          disabled={updateMutation.isPending || removeMutation.isPending}
                          onClick={() => removeMutation.mutate(item.productId)}
                          type="button"
                        >
                          Remove
                        </button>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          </div>
          <div className="col-lg-4">
            <div className="card border-0 shadow-sm">
              <div className="card-body p-4">
                <h2 className="h5 mb-4">Order summary</h2>
                <div className="d-flex justify-content-between mb-2">
                  <span className="text-secondary">Subtotal</span>
                  <span>${cartQuery.data.subtotal.toFixed(2)}</span>
                </div>
                <div className="d-flex justify-content-between border-top pt-3 mt-3 fw-semibold">
                  <span>Total</span>
                  <span>${cartQuery.data.total.toFixed(2)}</span>
                </div>
                <p className="small text-secondary mt-2">
                  Taxes and delivery, if applicable, will be confirmed before payment.
                </p>
                <Link
                  aria-disabled={updateMutation.isPending || removeMutation.isPending}
                  className={`btn btn-dark btn-lg w-100 mt-2${updateMutation.isPending || removeMutation.isPending ? ' disabled' : ''}`}
                  onClick={(event) => {
                    if (updateMutation.isPending || removeMutation.isPending) {
                      event.preventDefault()
                    }
                  }}
                  to="/checkout/shipping"
                >
                  Checkout
                </Link>
              </div>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}
