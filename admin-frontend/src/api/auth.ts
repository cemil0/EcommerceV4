import apiClient from './client'
import type { LoginRequest, LoginResponse } from '@/types/api'

export const authApi = {
    login: async (credentials: LoginRequest): Promise<LoginResponse> => {
        const response = await apiClient.post<LoginResponse>('/auth/login', credentials)
        return response.data
    },

    logout: async (): Promise<void> => {
        await apiClient.post('/auth/logout')
    },

    refreshToken: async (refreshToken: string): Promise<LoginResponse> => {
        const response = await apiClient.post<LoginResponse>('/auth/refresh', {
            refreshToken,
        })
        return response.data
    },
}
