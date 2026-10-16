import { Link } from 'react-router-dom'

export function Footer() {
  return (
    <footer className="border-top py-4 mt-5 bg-white">
      <div className="container d-flex flex-column flex-sm-row justify-content-between align-items-center gap-2 small text-secondary">
        <span>Baked with care, shared with love.</span>
        <Link className="text-decoration-none" to="/products">Browse the bakery</Link>
      </div>
    </footer>
  )
}
