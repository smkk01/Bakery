import { api } from './api'
import type { Cart } from '../types/api'

export async function getCart(customerId: string, token: string): Promise<Cart> {
  const { data } = await api.get<Cart>('/cart', {
    params: { customerId },
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function addToCart(
  customerId: string,
  productId: string,
  quantity: number,
  token: string,
): Promise<Cart> {
  const { data } = await api.post<Cart>(
    '/cart/add',
    { customerId, productId, quantity },
    { headers: { Authorization: `Bearer ${token}` } },
  )
  return data
}

export async function updateCartItem(
  customerId: string,
  productId: string,
  quantity: number,
  token: string,
): Promise<Cart> {
  const { data } = await api.put<Cart>(
    '/cart/update',
    { customerId, productId, quantity },
    { headers: { Authorization: `Bearer ${token}` } },
  )
  return data
}

export async function removeCartItem(
  customerId: string,
  productId: string,
  token: string,
): Promise<Cart> {
  const { data } = await api.delete<Cart>('/cart/remove', {
    data: { customerId, productId },
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}
