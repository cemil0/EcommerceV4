import axios from 'axios'
import { toast } from 'sonner'

const API_BASE_URL = '/api/v1'

export const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
})

// Request interceptor to add JWT token
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token')
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error) => {
        return Promise.reject(error)
    }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config

        // Handle 401 Unauthorized
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true

            try {
                const refreshToken = localStorage.getItem('refreshToken')
                if (refreshToken) {
                    const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
                        refreshToken,
                    })

                    const { token, refreshToken: newRefreshToken } = response.data
                    localStorage.setItem('token', token)
                    localStorage.setItem('refreshToken', newRefreshToken)

                    originalRequest.headers.Authorization = `Bearer ${token}`
                    return apiClient(originalRequest)
                }
            } catch (refreshError) {
                // Refresh failed, logout user
                localStorage.removeItem('token')
                localStorage.removeItem('refreshToken')
                localStorage.removeItem('user')
                window.location.href = '/Account/Login'
                return Promise.reject(refreshError)
            }
        }

        // Handle other errors
        const errorMessage = error.response?.data?.message || error.message || 'Bir hata oluştu'
        toast.error(errorMessage)

        return Promise.reject(error)
    }
)

export default apiClient
