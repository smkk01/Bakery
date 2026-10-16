import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <section className="container py-5 text-center">
      <p className="display-1" aria-hidden="true">🥨</p>
      <h1 className="h2">We lost the crumbs</h1>
      <p className="text-secondary">That page isn’t on our menu.</p>
      <Link className="btn btn-dark" to="/">Back to the bakery</Link>
    </section>
  )
}
