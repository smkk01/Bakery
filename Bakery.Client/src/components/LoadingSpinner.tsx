interface LoadingSpinnerProps {
  label?: string
}

export function LoadingSpinner({ label = 'Loading' }: Readonly<LoadingSpinnerProps>) {
  return (
    <div className="d-flex justify-content-center align-items-center gap-3 py-5" role="status">
      <span aria-hidden="true" className="spinner-border text-secondary" />
      <span className="text-secondary">{label}</span>
    </div>
  )
}
