import { useMemo, useState, type FormEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { AdminTable, type AdminTableColumn } from '../components/AdminTable'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { useAuth } from '../contexts/AuthContext'
import {
  createCategory,
  createProduct,
  deleteCategory,
  deleteProduct,
  getCategories,
  getProducts,
  updateCategory,
  updateProduct,
} from '../services/products'
import { getOrders, updateOrderStatus } from '../services/orders'
import type {
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  Order,
  Product,
} from '../types/api'
import { OrderStatus, orderStatusLabels } from '../types/api'
import { getApiErrorMessage } from '../utils/apiError'

type DashboardSection = 'products' | 'categories' | 'orders'

interface ProductFormState {
  categoryId: string
  name: string
  description: string
  price: string
  imageUrl: string
  isAvailable: boolean
}

const emptyProduct: ProductFormState = {
  categoryId: '',
  name: '',
  description: '',
  price: '',
  imageUrl: '',
  isAvailable: true,
}

const orderStatuses = Object.values(OrderStatus).filter(
  (value): value is OrderStatus => typeof value === 'number',
)

function formatDate(value: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function AdminPage() {
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const [section, setSection] = useState<DashboardSection>('products')
  const [productForm, setProductForm] = useState<ProductFormState>(emptyProduct)
  const [editingProductId, setEditingProductId] = useState<string | null>(null)
  const [categoryName, setCategoryName] = useState('')
  const [categoryDescription, setCategoryDescription] = useState('')
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null)

  const categoriesQuery = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  })
  const productsQuery = useQuery({
    queryKey: ['products'],
    queryFn: getProducts,
  })
  const ordersQuery = useQuery({
    queryKey: ['admin-orders', user?.userId],
    queryFn: () => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required to read orders.')
      }
      return getOrders(user.accessToken)
    },
    enabled: user?.role === 'Admin',
  })

  const productMutation = useMutation({
    mutationFn: ({ id, request }: { id: string | null; request: CreateProductRequest }) => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required.')
      }
      return id
        ? updateProduct(id, request, user.accessToken)
        : createProduct(request, user.accessToken)
    },
    onSuccess: async () => {
      setProductForm(emptyProduct)
      setEditingProductId(null)
      await queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
  const deleteProductMutation = useMutation({
    mutationFn: (id: string) => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required.')
      }
      return deleteProduct(id, user.accessToken)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  })
  const categoryMutation = useMutation({
    mutationFn: ({ id, request }: { id: string | null; request: CreateCategoryRequest }) => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required.')
      }
      return id
        ? updateCategory(id, request, user.accessToken)
        : createCategory(request, user.accessToken)
    },
    onSuccess: async () => {
      setCategoryName('')
      setCategoryDescription('')
      setEditingCategoryId(null)
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
    },
  })
  const deleteCategoryMutation = useMutation({
    mutationFn: (id: string) => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required.')
      }
      return deleteCategory(id, user.accessToken)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] }),
  })
  const orderStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => {
      if (!user || user.role !== 'Admin') {
        throw new Error('Administrator access is required.')
      }
      return updateOrderStatus(id, status, user.accessToken)
    },
    onSuccess: (order) => {
      queryClient.setQueryData<Order[]>(['admin-orders', user?.userId], (orders) =>
        orders?.map((existing) => existing.id === order.id ? order : existing),
      )
    },
  })

  const categoryNames = useMemo(
    () => new Map((categoriesQuery.data ?? []).map((category) => [category.id, category.name])),
    [categoriesQuery.data],
  )

  const productColumns: AdminTableColumn<Product>[] = [
    {
      key: 'name',
      label: 'Product',
      searchValue: (product) => `${product.name} ${product.description}`,
      sortValue: (product) => product.name,
      render: (product) => (
        <Link className="fw-semibold text-decoration-none" to={`/products/${product.id}`}>
          {product.name}
        </Link>
      ),
    },
    {
      key: 'category',
      label: 'Category',
      searchValue: (product) => categoryNames.get(product.categoryId) ?? '',
      sortValue: (product) => categoryNames.get(product.categoryId) ?? '',
      render: (product) => categoryNames.get(product.categoryId) ?? 'Uncategorized',
    },
    {
      key: 'price',
      label: 'Price',
      searchValue: (product) => product.price.toFixed(2),
      sortValue: (product) => product.price,
      render: (product) => `$${product.price.toFixed(2)}`,
    },
    {
      key: 'availability',
      label: 'Availability',
      searchValue: (product) => product.isAvailable ? 'available' : 'unavailable sold out',
      sortValue: (product) => product.isAvailable ? 1 : 0,
      render: (product) => (
        <span className={`badge ${product.isAvailable ? 'text-bg-success' : 'text-bg-secondary'}`}>
          {product.isAvailable ? 'Available' : 'Unavailable'}
        </span>
      ),
    },
    {
      key: 'actions',
      label: 'Actions',
      searchValue: () => '',
      render: (product) => (
        <div className="d-flex gap-2">
          <button className="btn btn-sm btn-outline-dark" onClick={() => startProductEdit(product)} type="button">
            Edit
          </button>
          <button
            className="btn btn-sm btn-outline-danger"
            disabled={deleteProductMutation.isPending}
            onClick={() => {
              if (window.confirm(`Delete "${product.name}"?`)) {
                deleteProductMutation.mutate(product.id)
              }
            }}
            type="button"
          >
            Delete
          </button>
        </div>
      ),
    },
  ]

  const categoryColumns: AdminTableColumn<Category>[] = [
    {
      key: 'name',
      label: 'Category',
      searchValue: (category) => `${category.name} ${category.description}`,
      sortValue: (category) => category.name,
      render: (category) => <span className="fw-semibold">{category.name}</span>,
    },
    {
      key: 'products',
      label: 'Products',
      searchValue: (category) => String(productsQuery.data?.filter((product) => product.categoryId === category.id).length ?? 0),
      sortValue: (category) => productsQuery.data?.filter((product) => product.categoryId === category.id).length ?? 0,
      render: (category) => productsQuery.data?.filter((product) => product.categoryId === category.id).length ?? 0,
    },
    {
      key: 'actions',
      label: 'Actions',
      searchValue: () => '',
      render: (category) => (
        <div className="d-flex gap-2">
          <button className="btn btn-sm btn-outline-dark" onClick={() => startCategoryEdit(category)} type="button">
            Edit
          </button>
          <button
            className="btn btn-sm btn-outline-danger"
            disabled={deleteCategoryMutation.isPending}
            onClick={() => {
              const usedByProducts = productsQuery.data?.some((product) => product.categoryId === category.id)
              const warning = usedByProducts
                ? `Products use "${category.name}". Deleting it may leave those products uncategorized. Continue?`
                : `Delete category "${category.name}"?`
              if (window.confirm(warning)) {
                deleteCategoryMutation.mutate(category.id)
              }
            }}
            type="button"
          >
            Delete
          </button>
        </div>
      ),
    },
  ]

  const orderColumns: AdminTableColumn<Order>[] = [
    {
      key: 'number',
      label: 'Order',
      searchValue: (order) => `${order.orderNumber} ${order.customerId}`,
      sortValue: (order) => order.orderNumber,
      render: (order) => (
        <div>
          <span className="d-block fw-semibold">{order.orderNumber}</span>
          <span className="small text-secondary">Customer {order.customerId}</span>
        </div>
      ),
    },
    {
      key: 'date',
      label: 'Date',
      searchValue: (order) => formatDate(order.orderDate),
      sortValue: (order) => new Date(order.orderDate).getTime(),
      render: (order) => formatDate(order.orderDate),
    },
    {
      key: 'items',
      label: 'Items',
      searchValue: (order) => order.items.map((item) => item.productName).join(' '),
      sortValue: (order) => order.items.length,
      render: (order) => (
        <span title={order.items.map((item) => `${item.productName} × ${item.quantity}`).join(', ')}>
          {order.items.length} item{order.items.length === 1 ? '' : 's'}
        </span>
      ),
    },
    {
      key: 'total',
      label: 'Total',
      searchValue: (order) => order.totalAmount.toFixed(2),
      sortValue: (order) => order.totalAmount,
      render: (order) => `$${order.totalAmount.toFixed(2)}`,
    },
    {
      key: 'status',
      label: 'Status',
      searchValue: (order) => orderStatusLabels[order.status] ?? String(order.status),
      sortValue: (order) => order.status,
      render: (order) => (
        <span className={`badge ${order.status === OrderStatus.Cancelled ? 'text-bg-secondary' : order.status === OrderStatus.Delivered ? 'text-bg-success' : 'text-bg-warning'}`}>
          {orderStatusLabels[order.status] ?? String(order.status)}
        </span>
      ),
    },
    {
      key: 'update',
      label: 'Update status',
      searchValue: () => '',
      render: (order) => (
        <select
          aria-label={`Update status for ${order.orderNumber}`}
          className="form-select form-select-sm admin-status-select"
          disabled={orderStatusMutation.isPending}
          onChange={(event) => orderStatusMutation.mutate({
            id: order.id,
            status: Number(event.target.value) as OrderStatus,
          })}
          value={order.status}
        >
          {orderStatuses.map((status) => (
            <option key={status} value={status}>{orderStatusLabels[status]}</option>
          ))}
        </select>
      ),
    },
  ]

  function startProductEdit(product: Product) {
    setEditingProductId(product.id)
    setProductForm({
      categoryId: product.categoryId,
      name: product.name,
      description: product.description,
      price: String(product.price),
      imageUrl: product.imageUrl ?? '',
      isAvailable: product.isAvailable,
    })
    setSection('products')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function startCategoryEdit(category: Category) {
    setEditingCategoryId(category.id)
    setCategoryName(category.name)
    setCategoryDescription(category.description)
    setSection('categories')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function submitProduct(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    productMutation.mutate({
      id: editingProductId,
      request: {
        categoryId: productForm.categoryId || categoriesQuery.data?.[0]?.id || '',
        name: productForm.name.trim(),
        description: productForm.description.trim(),
        price: Number(productForm.price),
        imageUrl: productForm.imageUrl.trim() || null,
        isAvailable: productForm.isAvailable,
      },
    })
  }

  function submitCategory(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    categoryMutation.mutate({
      id: editingCategoryId,
      request: { name: categoryName.trim(), description: categoryDescription.trim() },
    })
  }

  if (!user || user.role !== 'Admin') {
    return (
      <section className="container py-5">
        <div className="alert alert-warning" role="alert">
          <h1 className="h4">Administrator access required</h1>
          <p className="mb-0">
            Sign in with an Admin account to manage the bakery. <Link to="/login">Go to sign in</Link>
          </p>
        </div>
      </section>
    )
  }

  const loading = categoriesQuery.isPending || productsQuery.isPending ||
    (section === 'orders' && ordersQuery.isPending)
  const queryError = categoriesQuery.error ?? productsQuery.error ??
    (section === 'orders' ? ordersQuery.error : null)

  const sectionTabs: Array<{ id: DashboardSection; label: string; count: number | undefined }> = [
    { id: 'products', label: 'Products', count: productsQuery.data?.length },
    { id: 'categories', label: 'Categories', count: categoriesQuery.data?.length },
    { id: 'orders', label: 'Orders', count: ordersQuery.data?.length },
  ]

  const sharedMutationError = productMutation.error ?? deleteProductMutation.error ??
    categoryMutation.error ?? deleteCategoryMutation.error ?? orderStatusMutation.error
  const sharedMutationSuccess: ReactNode = productMutation.isSuccess
    ? 'Product saved.'
    : categoryMutation.isSuccess
      ? 'Category saved.'
      : deleteProductMutation.isSuccess
        ? 'Product deleted.'
        : deleteCategoryMutation.isSuccess
          ? 'Category deleted.'
          : orderStatusMutation.isSuccess
            ? 'Order status updated.'
            : null

  return (
    <section className="container py-5">
      <p className="eyebrow">Bakery operations</p>
      <div className="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-4">
        <div>
          <h1 className="display-6 fw-semibold mb-1">Admin dashboard</h1>
          <p className="text-secondary mb-0">Manage catalog and order fulfilment.</p>
        </div>
        <span className="badge rounded-pill text-bg-dark px-3 py-2">Signed in as Admin</span>
      </div>

      <div aria-label="Admin dashboard sections" className="nav nav-tabs admin-tabs mb-4" role="tablist">
        {sectionTabs.map((tab) => (
          <button
            aria-selected={section === tab.id}
            className={`nav-link${section === tab.id ? ' active' : ''}`}
            id={`admin-tab-${tab.id}`}
            key={tab.id}
            onClick={() => setSection(tab.id)}
            role="tab"
            type="button"
          >
            {tab.label}
            {tab.count !== undefined && <span className="badge text-bg-light ms-2">{tab.count}</span>}
          </button>
        ))}
      </div>

      {loading && <LoadingSpinner label={`Loading ${section}…`} />}
      {queryError && (
        <div className="alert alert-danger" role="alert">
          {getApiErrorMessage(queryError, `Could not load ${section}.`)}
        </div>
      )}
      {sharedMutationError && (
        <div className="alert alert-danger" role="alert">
          {getApiErrorMessage(sharedMutationError, 'The requested change failed.')}
        </div>
      )}
      {sharedMutationSuccess && (
        <div className="alert alert-success" role="status">{sharedMutationSuccess}</div>
      )}

      {section === 'products' && (
        <div className="admin-section" id="admin-panel-products" role="tabpanel" aria-labelledby="admin-tab-products">
          <div className="card border-0 shadow-sm mb-4">
            <div className="card-body p-4">
              <h2 className="h4">{editingProductId ? 'Edit product' : 'Add a product'}</h2>
              {categoriesQuery.data?.length === 0 ? (
                <p className="mb-0 text-secondary">Create a category before adding products.</p>
              ) : (
                <form onSubmit={submitProduct}>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <label className="form-label" htmlFor="admin-product-name">Name</label>
                      <input className="form-control" id="admin-product-name" maxLength={120} onChange={(event) => setProductForm({ ...productForm, name: event.target.value })} required value={productForm.name} />
                    </div>
                    <div className="col-md-3">
                      <label className="form-label" htmlFor="admin-product-price">Price</label>
                      <input className="form-control" id="admin-product-price" max="1000000" min="0.01" onChange={(event) => setProductForm({ ...productForm, price: event.target.value })} required step="0.01" type="number" value={productForm.price} />
                    </div>
                    <div className="col-md-3">
                      <label className="form-label" htmlFor="admin-product-category">Category</label>
                      <select className="form-select" id="admin-product-category" onChange={(event) => setProductForm({ ...productForm, categoryId: event.target.value })} required value={productForm.categoryId || categoriesQuery.data?.[0]?.id || ''}>
                        {categoriesQuery.data?.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
                      </select>
                    </div>
                    <div className="col-12">
                      <label className="form-label" htmlFor="admin-product-description">Description</label>
                      <textarea className="form-control" id="admin-product-description" maxLength={2000} onChange={(event) => setProductForm({ ...productForm, description: event.target.value })} required rows={2} value={productForm.description} />
                    </div>
                    <div className="col-md-8">
                      <label className="form-label" htmlFor="admin-product-image">Image URL</label>
                      <input className="form-control" id="admin-product-image" onChange={(event) => setProductForm({ ...productForm, imageUrl: event.target.value })} type="url" value={productForm.imageUrl} />
                    </div>
                    <div className="col-md-4 d-flex align-items-end">
                      <div className="form-check mb-2">
                        <input checked={productForm.isAvailable} className="form-check-input" id="admin-product-available" onChange={(event) => setProductForm({ ...productForm, isAvailable: event.target.checked })} type="checkbox" />
                        <label className="form-check-label" htmlFor="admin-product-available">Available for sale</label>
                      </div>
                    </div>
                  </div>
                  <div className="d-flex gap-2 mt-3">
                    <button className="btn btn-dark" disabled={productMutation.isPending || !categoriesQuery.data?.length} type="submit">
                      {productMutation.isPending ? 'Saving…' : editingProductId ? 'Save product' : 'Create product'}
                    </button>
                    {editingProductId && (
                      <button className="btn btn-outline-secondary" onClick={() => { setEditingProductId(null); setProductForm(emptyProduct) }} type="button">
                        Cancel
                      </button>
                    )}
                  </div>
                </form>
              )}
            </div>
          </div>
          <AdminTable
            columns={productColumns}
            emptyMessage="No products have been added yet."
            rowKey={(product) => product.id}
            rows={productsQuery.data ?? []}
            searchPlaceholder="Search products…"
          />
        </div>
      )}

      {section === 'categories' && (
        <div className="admin-section" id="admin-panel-categories" role="tabpanel" aria-labelledby="admin-tab-categories">
          <div className="card border-0 shadow-sm mb-4">
            <div className="card-body p-4">
              <h2 className="h4">{editingCategoryId ? 'Edit category' : 'Add a category'}</h2>
              <form onSubmit={submitCategory}>
                <div className="row g-3">
                  <div className="col-md-4">
                    <label className="form-label" htmlFor="admin-category-name">Name</label>
                    <input className="form-control" id="admin-category-name" maxLength={100} onChange={(event) => setCategoryName(event.target.value)} required value={categoryName} />
                  </div>
                  <div className="col-md-8">
                    <label className="form-label" htmlFor="admin-category-description">Description</label>
                    <input className="form-control" id="admin-category-description" maxLength={1000} onChange={(event) => setCategoryDescription(event.target.value)} required value={categoryDescription} />
                  </div>
                </div>
                <div className="d-flex gap-2 mt-3">
                  <button className="btn btn-dark" disabled={categoryMutation.isPending} type="submit">
                    {categoryMutation.isPending ? 'Saving…' : editingCategoryId ? 'Save category' : 'Create category'}
                  </button>
                  {editingCategoryId && (
                    <button className="btn btn-outline-secondary" onClick={() => { setEditingCategoryId(null); setCategoryName(''); setCategoryDescription('') }} type="button">
                      Cancel
                    </button>
                  )}
                </div>
              </form>
            </div>
          </div>
          <AdminTable
            columns={categoryColumns}
            emptyMessage="No categories have been added yet."
            rowKey={(category) => category.id}
            rows={categoriesQuery.data ?? []}
            searchPlaceholder="Search categories…"
          />
        </div>
      )}

      {section === 'orders' && (
        <div className="admin-section" id="admin-panel-orders" role="tabpanel" aria-labelledby="admin-tab-orders">
          {orderStatusMutation.error && (
            <div className="alert alert-danger" role="alert">
              {getApiErrorMessage(orderStatusMutation.error, 'Could not update order status.')}
            </div>
          )}
          <AdminTable
            columns={orderColumns}
            emptyMessage="No customer orders have been placed yet."
            rowKey={(order) => order.id}
            rows={ordersQuery.data ?? []}
            searchPlaceholder="Search orders by number, customer, or item…"
          />
        </div>
      )}
    </section>
  )
}
