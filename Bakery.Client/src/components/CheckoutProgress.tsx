import { NavLink } from 'react-router-dom'

const steps = [
  { path: '/checkout/shipping', label: 'Shipping', number: '01' },
  { path: '/checkout/review', label: 'Review order', number: '02' },
  { path: '/checkout/confirmation', label: 'Confirmation', number: '03' },
]

export function CheckoutProgress({ currentStep }: Readonly<{ currentStep: number }>) {
  return (
    <nav aria-label="Checkout progress" className="checkout-progress mb-5">
      <ol className="list-unstyled d-flex justify-content-between gap-2 mb-0">
        {steps.map((step, index) => (
          <li className={`checkout-progress-step${index <= currentStep ? ' is-current' : ''}`} key={step.path}>
            {index < currentStep ? (
              <NavLink className="checkout-progress-link" to={step.path}>
                <span className="checkout-progress-number">{step.number}</span>
                <span>{step.label}</span>
              </NavLink>
            ) : (
              <span aria-current={index === currentStep ? 'step' : undefined} className="checkout-progress-label">
                <span className="checkout-progress-number">{step.number}</span>
                <span>{step.label}</span>
              </span>
            )}
          </li>
        ))}
      </ol>
    </nav>
  )
}
