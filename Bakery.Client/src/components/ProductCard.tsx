import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { Product } from '../types/api'

interface ProductCardProps {
  product: Product
  onAdd: (product: Product) => void
}

export function ProductCard({ product, onAdd }: ProductCardProps) {
  const [imageFailed, setImageFailed] = useState(false)

  return (
    <article className="card product-card h-100 border-0 shadow-sm">
      {product.imageUrl && !imageFailed ? (
        <img
          src={product.imageUrl}
          className="card-img-top product-image"
          alt={product.name}
          loading="lazy"
          onError={() => setImageFailed(true)}
        />
      ) : (
        <div className="product-image-placeholder" aria-hidden="true">🥐</div>
      )}
      <div className="card-body d-flex flex-column p-4">
        <h2 className="h5 card-title">
          <Link className="text-decoration-none text-reset" to={`/products/${product.id}`}>
            {product.name}
          </Link>
        </h2>
        <p className="card-text text-secondary flex-grow-1">{product.description}</p>
        <div className="d-flex align-items-center justify-content-between mt-3">
          <span className="fw-semibold">${product.price.toFixed(2)}</span>
          <button
            className="btn btn-outline-dark btn-sm"
            disabled={!product.isAvailable}
            onClick={() => onAdd(product)}
            type="button"
          >
            {product.isAvailable ? 'Add to basket' : 'Sold out'}
          </button>
        </div>
      </div>
    </article>
  )
}
