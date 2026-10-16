import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet } from 'react-router-dom'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { CheckoutProvider, emptyShippingAddress } from '../contexts/CheckoutContext'
import { useAuth } from '../contexts/AuthContext'
import { getCart } from '../services/cart'
import { getApiErrorMessage } from '../utils/apiError'

export function CheckoutFlow() {
  const { user } = useAuth()
  const [shipping, setShipping] = useState({
    ...emptyShippingAddress,
    fullName: user?.fullName ?? '',
    email: user?.email ?? '',
  })
  useEffect(() => {
    setShipping({
      ...emptyShippingAddress,
      fullName: user?.fullName ?? '',
      email: user?.email ?? '',
    })
  }, [user?.email, user?.fullName, user?.userId])

  const cartQuery = useQuery({
    queryKey: ['cart', user?.userId],
    queryFn: () => getCart(user!.userId, user!.accessToken),
    enabled: user?.role === 'Customer',
  })

  if (!user) {
    return <Navigate replace to="/login" />
  }

  if (user.role !== 'Customer') {
    return (
      <section className="container py-5">
        <div className="alert alert-warning" role="alert">
          Checkout is available to customer accounts.
        </div>
      </section>
    )
  }

  if (cartQuery.isPending) {
    return <div className="container"><LoadingSpinner label="Preparing checkout…" /></div>
  }

  if (cartQuery.isError) {
    return (
      <section className="container py-5">
        <div className="alert alert-danger" role="alert">
          {getApiErrorMessage(cartQuery.error, 'Could not load your basket.')}
        </div>
      </section>
    )
  }

  if (!cartQuery.data?.items.length) {
    return <Navigate replace to="/cart" />
  }

  return (
    <CheckoutProvider value={{ user, cart: cartQuery.data, shipping, setShipping }}>
      <Outlet />
    </CheckoutProvider>
  )
}
