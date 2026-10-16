import { api } from './api'
import type { CreateOrderRequest, Order, OrderStatus } from '../types/api'

export async function createOrder(request: CreateOrderRequest, token: string): Promise<Order> {
  const { data } = await api.post<Order>('/orders', request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function getOrders(token: string): Promise<Order[]> {
  const { data } = await api.get<Order[]>('/orders', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function updateOrderStatus(
  id: string,
  status: OrderStatus,
  token: string,
): Promise<Order> {
  const { data } = await api.put<Order>(`/orders/${id}/status`, { status }, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}
