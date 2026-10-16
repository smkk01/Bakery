import { Link, useLocation, Navigate } from 'react-router-dom'
import { CheckoutProgress } from '../components/CheckoutProgress'
import type { ShippingAddress } from '../contexts/CheckoutContext'
import type { Order } from '../types/api'

interface ConfirmationState {
  order?: Order
  shipping?: ShippingAddress
}

export function OrderConfirmationPage() {
  const location = useLocation()
  const { order, shipping } = (location.state as ConfirmationState | null) ?? {}

  if (!order || !shipping) {
    return <Navigate replace to="/checkout/shipping" />
  }

  return (
    <section className="container py-4 py-lg-5">
      <CheckoutProgress currentStep={2} />
      <div className="checkout-confirmation rounded-4 p-4 p-md-5 text-center mx-auto">
        <span aria-hidden="true" className="display-3">🎉</span>
        <p className="eyebrow mt-3 mb-2">Order confirmed</p>
        <h1 className="display-6 fw-semibold">Thank you, {shipping.fullName}!</h1>
        <p className="text-secondary">
          Your order has been placed. A confirmation will be sent to {shipping.email}.
        </p>
        <div className="confirmation-order-number my-4">
          <span className="d-block small text-secondary">Order number</span>
          <strong>{order.orderNumber}</strong>
        </div>
        <div className="mx-auto confirmation-summary text-start">
          <div className="d-flex justify-content-between gap-3">
            <span>Order status</span>
            <span>{order.status}</span>
          </div>
          <div className="d-flex justify-content-between gap-3 mt-2 fw-semibold">
            <span>Total</span>
            <span>${order.totalAmount.toFixed(2)}</span>
          </div>
          <div className="border-top mt-3 pt-3 small text-secondary">
            Delivering to {shipping.addressLine1}, {shipping.city}, {shipping.country}
          </div>
        </div>
        <Link className="btn btn-dark mt-4 px-4" to="/products">Continue shopping</Link>
      </div>
    </section>
  )
}
