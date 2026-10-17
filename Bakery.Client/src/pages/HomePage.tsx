import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { ProductCard } from '../components/ProductCard'
import { CartSuccessToast } from '../components/CartSuccessToast'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { useAuth } from '../contexts/AuthContext'
import { useProducts } from '../hooks/useProducts'
import { addToCart } from '../services/cart'
import type { Product } from '../types/api'

export function HomePage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const productsQuery = useProducts()
  const [addedProductName, setAddedProductName] = useState<string | null>(null)
  const addMutation = useMutation({
    mutationFn: (product: Product) => {
      if (!user) {
        throw new Error('Sign in with a customer account to add items to your basket.')
      }
      if (user.role !== 'Customer') {
        throw new Error('Cart access is for customer accounts. Sign in with a customer account.')
      }
      return addToCart(user.userId, product.id, 1, user.accessToken)
    },
    onSuccess: (cart, product) => {
      queryClient.setQueryData(['cart', cart.customerId], cart)
      setAddedProductName(product.name)
    },
  })

  return (
    <>
      <section className="hero-section">
        <div className="container py-5">
          <div className="row align-items-center py-lg-5">
            <div className="col-lg-7 py-4">
              <p className="eyebrow mb-3">A little joy, fresh from the oven</p>
              <h1 className="display-3 fw-semibold hero-title tracking-tight">Made slowly.<br />Loved instantly.</h1>
              <p className="lead text-secondary mt-4 mb-4">
                Small-batch bakes, golden crusts, and something lovely for your table.
              </p>
              <Link className="btn btn-dark btn-lg px-4" to="/products">Explore the bakery</Link>
            </div>
            <div className="col-lg-5 text-center display-1 hero-art" aria-hidden="true">🥖</div>
          </div>
        </div>
      </section>

      <section className="container py-5" id="shop">
        <div className="d-flex flex-wrap align-items-end justify-content-between gap-3 mb-4">
          <div>
            <p className="eyebrow mb-2">The good stuff</p>
            <h2 className="display-6 fw-semibold mb-0">Freshly baked favorites</h2>
          </div>
          <span className="text-secondary">Made fresh, just for you</span>
        </div>

        {productsQuery.isPending && <LoadingSpinner label="Loading our bakes…" />}
        {productsQuery.isError && (
          <div className="alert alert-warning" role="alert">
            We couldn’t load the bakery catalog. Check that the API is running and try again.
          </div>
        )}
        {productsQuery.data?.length === 0 && (
          <div className="empty-state rounded-4 p-5 text-center">
            <span className="display-5" aria-hidden="true">🍞</span>
            <h3 className="h4 mt-3">The shelves are warming up</h3>
            <p className="text-secondary mb-0">There aren’t any products in the catalog yet.</p>
          </div>
        )}
        {productsQuery.data && productsQuery.data.length > 0 && (
          <div className="row g-4">
            {productsQuery.data.slice(0, 4).map((product) => (
              <div className="col-sm-6 col-lg-4 col-xl-3" key={product.id}>
                <ProductCard product={product} onAdd={(item) => addMutation.mutate(item)} />
              </div>
            ))}
          </div>
        )}
        {productsQuery.data && productsQuery.data.length > 4 && (
          <div className="text-center mt-4">
            <Link className="btn btn-outline-dark px-4" to="/products">View all products</Link>
          </div>
        )}
        {addMutation.isError && (
          <div className="alert alert-warning mt-4" role="alert">
            {addMutation.error.message}
            {!user && (
              <button className="btn btn-link p-0 ms-2" onClick={() => navigate('/login')} type="button">
                Sign in
              </button>
            )}
          </div>
        )}
      </section>
      {addedProductName && (
        <CartSuccessToast onDismiss={() => setAddedProductName(null)} productName={addedProductName} />
      )}
    </>
  )
}
