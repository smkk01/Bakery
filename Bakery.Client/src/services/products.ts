import { api } from './api'
import type {
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  Product,
} from '../types/api'

export async function getProducts(): Promise<Product[]> {
  const { data } = await api.get<Product[]>('/products')
  return data
}

export async function getProduct(id: string): Promise<Product> {
  const { data } = await api.get<Product>(`/products/${id}`)
  return data
}

export async function getCategories(): Promise<Category[]> {
  const { data } = await api.get<Category[]>('/categories')
  return data
}

export async function createProduct(
  request: CreateProductRequest,
  token: string,
): Promise<Product> {
  const { data } = await api.post<Product>('/products', request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function updateProduct(
  id: string,
  request: CreateProductRequest,
  token: string,
): Promise<Product> {
  const { data } = await api.put<Product>(`/products/${id}`, request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function deleteProduct(id: string, token: string): Promise<void> {
  await api.delete(`/products/${id}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
}

export async function createCategory(
  request: CreateCategoryRequest,
  token: string,
): Promise<Category> {
  const { data } = await api.post<Category>('/categories', request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function updateCategory(
  id: string,
  request: CreateCategoryRequest,
  token: string,
): Promise<Category> {
  const { data } = await api.put<Category>(`/categories/${id}`, request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return data
}

export async function deleteCategory(id: string, token: string): Promise<void> {
  await api.delete(`/categories/${id}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
}
