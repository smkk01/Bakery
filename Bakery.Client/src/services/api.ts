import axios from 'axios'

export const api = axios.create({
  baseURL: `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5247'}/api`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
})
