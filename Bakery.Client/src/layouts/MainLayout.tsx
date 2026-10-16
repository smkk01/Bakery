import { Outlet } from 'react-router-dom'
import { Breadcrumb } from '../components/Breadcrumb'
import { Footer } from '../components/Footer'
import { Navbar } from '../components/Navbar'

export function MainLayout() {
  return (
    <div className="app-shell min-vh-100 d-flex flex-column">
      <Navbar />
      <Breadcrumb />
      <main className="flex-grow-1">
        <Outlet />
      </main>
      <Footer />
    </div>
  )
}
