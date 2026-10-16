import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { CheckoutProgress } from '../components/CheckoutProgress'
import { useCheckout } from '../contexts/CheckoutContext'
import { createOrder } from '../services/orders'
import { getApiErrorMessage } from '../utils/apiError'

export function ReviewOrderPage() {
  const { user, cart, shipping } = useCheckout()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const hasShippingAddress = [
    shipping.fullName,
    shipping.email,
    shipping.phone,
    shipping.addressLine1,
    shipping.city,
    shipping.region,
    shipping.postalCode,
    shipping.country,
  ].every((value) => value.trim().length > 0)
  const orderMutation = useMutation({
    mutationFn: () => createOrder({
      customerId: user.userId,
      items: cart.items.map(({ productId, quantity }) => ({ productId, quantity })),
    }, user.accessToken),
    onSuccess: async (order) => {
      await queryClient.invalidateQueries({ queryKey: ['cart', user.userId] })
      navigate('/checkout/confirmation', { replace: true, state: { order, shipping } })
    },
  })

  if (!hasShippingAddress) {
    return <Navigate replace to="/checkout/shipping" />
  }

  return (
    <section className="container py-4 py-lg-5">
      <CheckoutProgress currentStep={1} />
      <div className="checkout-page-heading mb-4">
        <p className="eyebrow mb-2">One last look</p>
        <h1 className="display-6 fw-semibold mb-2">Review your order</h1>
        <p className="text-secondary mb-0">Check the delivery details and items before placing your order.</p>
      </div>

      <div className="row g-4 align-items-start">
        <div className="col-lg-7">
          <div className="card border-0 shadow-sm mb-4">
            <div className="card-body p-4">
              <div className="d-flex justify-content-between align-items-start gap-3">
                <div>
                  <p className="eyebrow mb-2">Delivering to</p>
                  <h2 className="h5">{shipping.fullName}</h2>
                  <address className="text-secondary mb-0">
                    {shipping.addressLine1}<br />
                    {shipping.addressLine2 && <>{shipping.addressLine2}<br /></>}
                    {shipping.city}, {shipping.region} {shipping.postalCode}<br />
                    {shipping.country}<br />
                    {shipping.phone}<br />
                    {shipping.email}
                  </address>
                </div>
                <Link className="btn btn-sm btn-outline-dark" to="/checkout/shipping">Edit</Link>
              </div>
            </div>
          </div>

          <div className="card border-0 shadow-sm">
            <div className="card-body p-4">
              <p className="eyebrow mb-3">Your items</p>
              <ul className="list-group list-group-flush">
                {cart.items.map((item) => (
                  <li className="list-group-item d-flex justify-content-between gap-3 px-0" key={item.productId}>
                    <span>{item.productName} <span className="text-secondary">× {item.quantity}</span></span>
                    <span>${item.subtotal.toFixed(2)}</span>
                  </li>
                ))}
              </ul>
              <div className="d-flex justify-content-between pt-3 fw-semibold">
                <span>Subtotal</span>
                <span>${cart.subtotal.toFixed(2)}</span>
              </div>
              <div className="d-flex justify-content-between pt-2 fw-semibold">
                <span>Total</span>
                <span>${cart.total.toFixed(2)}</span>
              </div>
              <p className="small text-secondary mt-2 mb-0">
                The API recalculates the final total from current catalog prices when the order is placed.
              </p>
            </div>
          </div>
        </div>

        <div className="col-lg-5">
          <div className="card border-0 shadow-sm">
            <div className="card-body p-4">
              <h2 className="h5">Place order</h2>
              <p className="small text-secondary">
                No payment is collected in this checkout. Order creation is sent to the bakery API.
              </p>
              {orderMutation.isError && (
                <div className="alert alert-danger" role="alert">
                  {getApiErrorMessage(orderMutation.error, 'Could not place the order.')}
                </div>
              )}
              <div className="d-grid gap-2">
                <button
                  className="btn btn-dark btn-lg"
                  disabled={orderMutation.isPending}
                  onClick={() => orderMutation.mutate()}
                  type="button"
                >
                  {orderMutation.isPending ? 'Placing order…' : 'Place order'}
                </button>
                <button
                  className="btn btn-link text-dark"
                  disabled={orderMutation.isPending}
                  onClick={() => navigate('/checkout/shipping')}
                  type="button"
                >
                  Back to shipping
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
