import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { api } from 'src/services/api'

interface AuthUser {
  id: string
  email: string
  businessName: string
}

interface LoginCredentials {
  email: string
  password: string
}

interface RegisterData {
  email: string
  password: string
  businessName: string
}

interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

const REFRESH_TOKEN_KEY = 'sme_refresh_token'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<AuthUser | null>(null)
  // Store refresh token in localStorage so it survives page refresh
  const refreshTokenValue = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))

  const isAuthenticated = computed(() => accessToken.value !== null)

  function setTokens(response: AuthResponse) {
    accessToken.value = response.accessToken
    refreshTokenValue.value = response.refreshToken
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)

    // Decode the JWT payload to extract user info (no library needed)
    try {
      const payload = JSON.parse(atob(response.accessToken.split('.')[1] ?? ''))
      user.value = {
        id: payload.sub,
        email: payload.email,
        businessName: payload.businessName
      }
    } catch {
      user.value = null
    }
  }

  function clearState() {
    accessToken.value = null
    refreshTokenValue.value = null
    user.value = null
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  }

  async function login(credentials: LoginCredentials) {
    const response = await api.post<AuthResponse>('/auth/login', credentials)
    setTokens(response.data)
  }

  async function register(data: RegisterData) {
    const response = await api.post<AuthResponse>('/auth/register', data)
    setTokens(response.data)
  }

  async function logout() {
    try {
      await api.post('/auth/logout')
    } finally {
      clearState()
    }
  }

  async function refreshToken(): Promise<boolean> {
    const token = refreshTokenValue.value
    if (!token) return false

    try {
      const response = await api.post<AuthResponse>('/auth/refresh', {
        refreshToken: token
      })
      setTokens(response.data)
      return true
    } catch {
      clearState()
      return false
    }
  }

  return {
    accessToken,
    user,
    isAuthenticated,
    refreshTokenValue,
    login,
    register,
    logout,
    refreshToken
  }
})
