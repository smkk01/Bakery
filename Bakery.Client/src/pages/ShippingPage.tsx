import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { CheckoutProgress } from '../components/CheckoutProgress'
import { useCheckout } from '../contexts/CheckoutContext'

export function ShippingPage() {
  const { shipping, setShipping } = useCheckout()
  const navigate = useNavigate()
  const [formError, setFormError] = useState('')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    if (!form.reportValidity()) {
      return
    }

    if (shipping.phone.replace(/\D/g, '').length < 7) {
      setFormError('Enter a valid phone number with at least 7 digits.')
      return
    }

    setFormError('')
    navigate('/checkout/review')
  }

  return (
    <section className="container py-4 py-lg-5">
      <CheckoutProgress currentStep={0} />
      <div className="checkout-page-heading mb-4">
        <p className="eyebrow mb-2">Where should we deliver?</p>
        <h1 className="display-6 fw-semibold mb-2">Shipping details</h1>
        <p className="text-secondary mb-0">Add the delivery contact and address for this order.</p>
      </div>

      <div className="row g-4 align-items-start">
        <div className="col-lg-8">
          <form className="card border-0 shadow-sm" noValidate onSubmit={handleSubmit}>
            <div className="card-body p-4 p-lg-5">
              <div className="row g-3">
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-name">Full name</label>
                  <input
                    autoComplete="name"
                    className="form-control"
                    id="shipping-name"
                    maxLength={200}
                    onChange={(event) => setShipping({ ...shipping, fullName: event.target.value })}
                    required
                    value={shipping.fullName}
                  />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-email">Email</label>
                  <input
                    autoComplete="email"
                    className="form-control"
                    id="shipping-email"
                    onChange={(event) => setShipping({ ...shipping, email: event.target.value })}
                    required
                    type="email"
                    value={shipping.email}
                  />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-phone">Phone</label>
                  <input
                    autoComplete="tel"
                    className="form-control"
                    id="shipping-phone"
                    maxLength={32}
                    onChange={(event) => setShipping({ ...shipping, phone: event.target.value })}
                    required
                    type="tel"
                    value={shipping.phone}
                  />
                </div>
                <div className="col-12">
                  <label className="form-label" htmlFor="shipping-address">Address</label>
                  <input
                    autoComplete="address-line1"
                    className="form-control"
                    id="shipping-address"
                    maxLength={200}
                    onChange={(event) => setShipping({ ...shipping, addressLine1: event.target.value })}
                    required
                    value={shipping.addressLine1}
                  />
                </div>
                <div className="col-12">
                  <label className="form-label" htmlFor="shipping-address2">Apartment, suite, etc. <span className="text-secondary">(optional)</span></label>
                  <input
                    autoComplete="address-line2"
                    className="form-control"
                    id="shipping-address2"
                    maxLength={100}
                    onChange={(event) => setShipping({ ...shipping, addressLine2: event.target.value })}
                    value={shipping.addressLine2}
                  />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-city">City</label>
                  <input autoComplete="address-level2" className="form-control" id="shipping-city" maxLength={100} onChange={(event) => setShipping({ ...shipping, city: event.target.value })} required value={shipping.city} />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-region">State / region</label>
                  <input autoComplete="address-level1" className="form-control" id="shipping-region" maxLength={100} onChange={(event) => setShipping({ ...shipping, region: event.target.value })} required value={shipping.region} />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-postal">Postal code</label>
                  <input autoComplete="postal-code" className="form-control" id="shipping-postal" maxLength={20} onChange={(event) => setShipping({ ...shipping, postalCode: event.target.value })} required value={shipping.postalCode} />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="shipping-country">Country</label>
                  <input autoComplete="country-name" className="form-control" id="shipping-country" maxLength={100} onChange={(event) => setShipping({ ...shipping, country: event.target.value })} required value={shipping.country} />
                </div>
              </div>
              {formError && <div className="alert alert-danger mt-4 mb-0" role="alert">{formError}</div>}
              <div className="d-flex justify-content-between align-items-center mt-4">
                <Link className="btn btn-link text-dark px-0" to="/cart">Back to basket</Link>
                <button className="btn btn-dark px-4" type="submit">Continue to review</button>
              </div>
            </div>
          </form>
        </div>
        <div className="col-lg-4">
          <div className="alert alert-light border small text-secondary" role="note">
            Shipping details are collected for this checkout flow. The current order API does not yet save or transmit address fields.
          </div>
        </div>
      </div>
    </section>
  )
}
