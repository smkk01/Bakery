import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useEffect, useMemo, useState } from 'react'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { CartSuccessToast } from '../components/CartSuccessToast'
import { useAuth } from '../contexts/AuthContext'
import { addToCart } from '../services/cart'
import { getProduct } from '../services/products'
import { getApiErrorMessage } from '../utils/apiError'

export function ProductDetailsPage() {
  const { id = '' } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [imageFailed, setImageFailed] = useState(false)
  const [selectedImage, setSelectedImage] = useState<string | null>(null)
  const [quantity, setQuantity] = useState(1)
  const [addedProductName, setAddedProductName] = useState<string | null>(null)
  const productQuery = useQuery({
    queryKey: ['products', id],
    queryFn: () => getProduct(id),
    enabled: Boolean(id),
  })
  const galleryImages = useMemo(() => {
    const images = productQuery.data?.images?.length
      ? productQuery.data.images
      : productQuery.data?.imageUrl
        ? [productQuery.data.imageUrl]
        : []
    return [...new Set(images.filter(Boolean))]
  }, [productQuery.data])

  useEffect(() => {
    setSelectedImage(galleryImages[0] ?? null)
    setImageFailed(false)
    setQuantity(1)
  }, [id, galleryImages])

  const addMutation = useMutation({
    mutationFn: async () => {
      if (!user || user.role !== 'Customer') {
        throw new Error('Sign in with a customer account to add items to your basket.')
      }
      if (!productQuery.data) {
        throw new Error('Product details are not available.')
      }
      return addToCart(user.userId, productQuery.data.id, quantity, user.accessToken)
    },
    onSuccess: (cart) => {
      queryClient.setQueryData(['cart', cart.customerId], cart)
      setAddedProductName(productQuery.data?.name ?? 'Item')
    },
  })

  if (productQuery.isPending) {
    return <div className="container"><LoadingSpinner label="Loading product…" /></div>
  }

  if (productQuery.isError || !productQuery.data) {
    return (
      <section className="container py-5">
        <div className="alert alert-warning" role="alert">This product could not be found.</div>
        <Link className="btn btn-outline-dark" to="/products">Back to products</Link>
      </section>
    )
  }

  const product = productQuery.data
  const displayedImage = selectedImage && galleryImages.includes(selectedImage)
    ? selectedImage
    : galleryImages[0]

  return (
    <section className="container py-5">
      <div className="row g-5 align-items-center">
        <div className="col-md-6">
          {displayedImage && !imageFailed ? (
            <div className="product-gallery">
              <div className="product-gallery-stage rounded-4 shadow-sm">
                <img
                  alt={product.name}
                  className="product-detail-image"
                  onError={() => setImageFailed(true)}
                  src={displayedImage}
                />
              </div>
              {galleryImages.length > 1 && (
                <div className="d-flex gap-2 mt-3" aria-label="Product image gallery">
                  {galleryImages.map((image, index) => (
                    <button
                      aria-label={`Show product image ${index + 1}`}
                      aria-pressed={image === displayedImage}
                      className={`product-gallery-thumbnail${image === displayedImage ? ' is-selected' : ''}`}
                      key={image}
                      onClick={() => {
                        setSelectedImage(image)
                        setImageFailed(false)
                      }}
                      type="button"
                    >
                      <img alt="" src={image} />
                    </button>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <div className="product-detail-placeholder rounded-4" aria-hidden="true">🥖</div>
          )}
        </div>
        <div className="col-md-6">
          <p className="eyebrow">From our bakery</p>
          <h1 className="display-5 fw-semibold">{product.name}</h1>
          <p className="lead text-secondary">{product.description}</p>
          <p aria-label={`Price $${product.price.toFixed(2)}`} className="display-6 fw-semibold">
            ${product.price.toFixed(2)}
          </p>
          {!product.isAvailable && <p className="text-danger">This item is currently unavailable.</p>}
          <div className="quantity-control d-flex align-items-center gap-3 my-4">
            <label className="form-label mb-0 fw-semibold" htmlFor="product-quantity">Quantity</label>
            <div className="input-group quantity-input-group">
              <button
                aria-label="Decrease quantity"
                className="btn btn-outline-secondary"
                disabled={quantity <= 1 || !product.isAvailable}
                onClick={() => setQuantity((current) => Math.max(1, current - 1))}
                type="button"
              >
                −
              </button>
              <input
                className="form-control text-center"
                id="product-quantity"
                max={10000}
                min={1}
                onChange={(event) => {
                  const next = Number(event.target.value)
                  if (Number.isInteger(next)) {
                    setQuantity(Math.min(10000, Math.max(1, next)))
                  }
                }}
                type="number"
                value={quantity}
              />
              <button
                aria-label="Increase quantity"
                className="btn btn-outline-secondary"
                disabled={quantity >= 10000 || !product.isAvailable}
                onClick={() => setQuantity((current) => Math.min(10000, current + 1))}
                type="button"
              >
                +
              </button>
            </div>
          </div>
          <button
            className="btn btn-dark btn-lg px-4"
            disabled={!product.isAvailable || addMutation.isPending || quantity < 1}
            onClick={() => {
              if (!user) {
                navigate('/login')
                return
              }
              addMutation.mutate()
            }}
            type="button"
          >
            {addMutation.isPending ? 'Adding…' : `Add ${quantity} to basket`}
          </button>
          {addMutation.isError && (
            <div className="alert alert-warning mt-3" role="alert">
              {getApiErrorMessage(addMutation.error, 'Could not add this item.')}
            </div>
          )}
        </div>
      </div>
      {addedProductName && (
        <CartSuccessToast onDismiss={() => setAddedProductName(null)} productName={addedProductName} />
      )}
    </section>
  )
}
