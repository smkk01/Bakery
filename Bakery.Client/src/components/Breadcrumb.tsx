import { Link, useLocation } from 'react-router-dom'

const routeLabels: Record<string, string> = {
  products: 'Products',
  cart: 'Cart',
  checkout: 'Checkout',
  shipping: 'Shipping',
  review: 'Review order',
  confirmation: 'Confirmation',
  login: 'Login',
  register: 'Register',
  admin: 'Admin',
}

export function Breadcrumb({ productName }: Readonly<{ productName?: string }>) {
  const location = useLocation()
  const segments = location.pathname.split('/').filter(Boolean)

  if (segments.length === 0) {
    return null
  }

  const crumbs = segments.map((segment, index) => ({
    label: index === 1 && segments[0] === 'products'
      ? productName ?? 'Product details'
      : routeLabels[segment] ?? segment,
    to: `/${segments.slice(0, index + 1).join('/')}`,
  }))

  return (
    <nav aria-label="breadcrumb" className="container pt-3">
      <ol className="breadcrumb mb-0 small">
        <li className="breadcrumb-item"><Link to="/">Home</Link></li>
        {crumbs.map((crumb, index) => (
          <li
            aria-current={index === crumbs.length - 1 ? 'page' : undefined}
            className={`breadcrumb-item${index === crumbs.length - 1 ? ' active' : ''}`}
            key={crumb.to}
          >
            {index === crumbs.length - 1 ? crumb.label : <Link to={crumb.to}>{crumb.label}</Link>}
          </li>
        ))}
      </ol>
    </nav>
  )
}
