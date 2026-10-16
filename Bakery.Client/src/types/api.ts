export interface Product {
  id: string
  categoryId: string
  name: string
  description: string
  price: number
  imageUrl: string | null
  images?: string[]
  isAvailable: boolean
}

export interface Category {
  id: string
  name: string
  description: string
}

export interface AuthResponse {
  userId: string
  fullName: string
  email: string
  role: 'Admin' | 'Customer'
  accessToken: string
  expiresAtUtc: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest extends LoginRequest {
  fullName: string
  phone: string
}

export interface CartItem {
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface Cart {
  id: string
  customerId: string
  items: CartItem[]
  subtotal: number
  total: number
}

export interface CreateOrderRequest {
  customerId: string
  items: Array<{
    productId: string
    quantity: number
  }>
}

export interface Order {
  id: string
  orderNumber: string
  customerId: string
  orderDate: string
  status: OrderStatus
  totalAmount: number
  items: Array<{
    productId: string
    productName: string
    quantity: number
    unitPrice: number
  }>
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Preparing = 2,
  ReadyForPickup = 3,
  OutForDelivery = 4,
  Delivered = 5,
  Cancelled = 6,
}

export const orderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Preparing]: 'Preparing',
  [OrderStatus.ReadyForPickup]: 'Ready for pickup',
  [OrderStatus.OutForDelivery]: 'Out for delivery',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
}

export interface CreateProductRequest {
  categoryId: string
  name: string
  description: string
  price: number
  imageUrl: string | null
  isAvailable: boolean
}

export interface CreateCategoryRequest {
  name: string
  description: string
}
