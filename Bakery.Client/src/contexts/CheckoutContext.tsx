import { createContext, useContext, type Dispatch, type SetStateAction } from 'react'
import type { AuthResponse, Cart } from '../types/api'

export interface ShippingAddress {
  fullName: string
  email: string
  phone: string
  addressLine1: string
  addressLine2: string
  city: string
  region: string
  postalCode: string
  country: string
}

export interface CheckoutContextValue {
  user: AuthResponse
  cart: Cart
  shipping: ShippingAddress
  setShipping: Dispatch<SetStateAction<ShippingAddress>>
}

export const emptyShippingAddress: ShippingAddress = {
  fullName: '',
  email: '',
  phone: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  region: '',
  postalCode: '',
  country: '',
}

const CheckoutContext = createContext<CheckoutContextValue | null>(null)

export const CheckoutProvider = CheckoutContext.Provider

export function useCheckout(): CheckoutContextValue {
  const context = useContext(CheckoutContext)
  if (!context) {
    throw new Error('Checkout pages must be rendered inside the checkout flow.')
  }
  return context
}
