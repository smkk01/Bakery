import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { ProductCard } from '../components/ProductCard'
import { CartSuccessToast } from '../components/CartSuccessToast'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { useAuth } from '../contexts/AuthContext'
import { addToCart } from '../services/cart'
import { getCategories } from '../services/products'
import { useProducts } from '../hooks/useProducts'
import type { Product } from '../types/api'
import { getApiErrorMessage } from '../utils/apiError'

export function ProductsPage() {
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [sortBy, setSortBy] = useState('featured')
  const [addedProductName, setAddedProductName] = useState<string | null>(null)
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const productsQuery = useProducts()
  const categoriesQuery = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  })
  const addMutation = useMutation({
    mutationFn: (product: Product) => {
      if (!user || user.role !== 'Customer') {
        throw new Error('Sign in with a customer account to add items to your basket.')
      }
      return addToCart(user.userId, product.id, 1, user.accessToken)
    },
    onSuccess: (cart, product) => {
      queryClient.setQueryData(['cart', cart.customerId], cart)
      setAddedProductName(product.name)
    },
  })

  const filteredProducts = useMemo(() => {
    const normalizedSearch = search.trim().toLocaleLowerCase()
    const filtered = (productsQuery.data ?? []).filter((product) => {
      const matchesSearch = !normalizedSearch ||
        `${product.name} ${product.description}`.toLocaleLowerCase().includes(normalizedSearch)
      return matchesSearch && (!categoryId || product.categoryId === categoryId)
    })

    switch (sortBy) {
      case 'name-asc':
        return filtered.sort((first, second) => first.name.localeCompare(second.name))
      case 'price-asc':
        return filtered.sort((first, second) => first.price - second.price)
      case 'price-desc':
        return filtered.sort((first, second) => second.price - first.price)
      default:
        return filtered
    }
  }, [categoryId, productsQuery.data, search, sortBy])

  return (
    <section className="container py-5">
      <div className="d-flex flex-column flex-lg-row justify-content-between align-items-lg-end gap-3 mb-4">
        <div>
          <p className="eyebrow mb-2">Made fresh every day</p>
          <h1 className="display-6 fw-semibold mb-0">Our products</h1>
          {productsQuery.data && (
            <p className="small text-secondary mt-2 mb-0">
              Showing {filteredProducts.length} of {productsQuery.data.length} products
            </p>
          )}
        </div>
        <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
          <div>
            <label className="visually-hidden" htmlFor="product-search">Search products</label>
            <input
              className="form-control"
              id="product-search"
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search bakes"
              type="search"
              value={search}
            />
          </div>
          <div>
            <label className="visually-hidden" htmlFor="product-category">Filter by category</label>
            <select
              className="form-select"
              id="product-category"
              onChange={(event) => setCategoryId(event.target.value)}
              value={categoryId}
            >
              <option value="">All categories</option>
              {categoriesQuery.data?.map((category) => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="visually-hidden" htmlFor="product-sort">Sort products</label>
            <select
              className="form-select"
              id="product-sort"
              onChange={(event) => setSortBy(event.target.value)}
              value={sortBy}
            >
              <option value="featured">Featured</option>
              <option value="name-asc">Name: A to Z</option>
              <option value="price-asc">Price: low to high</option>
              <option value="price-desc">Price: high to low</option>
            </select>
          </div>
        </div>
      </div>

      {productsQuery.isPending && <LoadingSpinner label="Loading the product catalog…" />}
      {(productsQuery.isError || categoriesQuery.isError) && (
        <div className="alert alert-warning" role="alert">
          The catalog couldn’t be loaded. Check your API connection and try again.
        </div>
      )}
      {productsQuery.data && filteredProducts.length === 0 && (
        <div className="empty-state rounded-4 p-5 text-center">
          <span className="display-5" aria-hidden="true">🥐</span>
          <h2 className="h4 mt-3">No matching bakes</h2>
          <p className="text-secondary mb-0">Try another search or category.</p>
        </div>
      )}
      {filteredProducts.length > 0 && (
        <div className="row g-4">
          {filteredProducts.map((product) => (
            <div className="col-sm-6 col-lg-4 col-xl-3" key={product.id}>
              <ProductCard
                product={product}
                onAdd={(item) => {
                  if (!user) {
                    navigate('/login')
                    return
                  }
                  addMutation.mutate(item)
                }}
              />
            </div>
          ))}
        </div>
      )}
      {addMutation.isError && (
      <div className="alert alert-warning mt-4" role="alert">
        {getApiErrorMessage(addMutation.error, 'Could not add this item.')}
      </div>
      )}
      {addedProductName && (
        <CartSuccessToast onDismiss={() => setAddedProductName(null)} productName={addedProductName} />
      )}
    </section>
  )
}
