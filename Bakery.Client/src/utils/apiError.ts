import axios from 'axios'

interface ProblemDetails {
  title?: string
  detail?: string
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    return error.response?.data?.detail
      ?? error.response?.data?.title
      ?? error.message
  }

  return error instanceof Error ? error.message : fallback
}
