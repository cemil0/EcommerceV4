import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { authApi } from '@/api/auth'
import type { User, LoginRequest } from '@/types/api'

interface AuthContextType {
    user: User | null
    isAuthenticated: boolean
    isLoading: boolean
    login: (credentials: LoginRequest) => Promise<void>
    logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(null)
    const [isLoading, setIsLoading] = useState(true)
    const navigate = useNavigate()

    useEffect(() => {
        // Check if user is logged in on mount
        const storedUser = localStorage.getItem('user')
        const token = localStorage.getItem('token')

        if (storedUser && token && storedUser !== 'undefined') {
            try {
                setUser(JSON.parse(storedUser))
            } catch (error) {
                // Invalid JSON, clear storage
                localStorage.removeItem('user')
                localStorage.removeItem('token')
                localStorage.removeItem('refreshToken')
            }
        }
        setIsLoading(false)
    }, [])

    const login = async (credentials: LoginRequest) => {
        try {
            const response = await authApi.login(credentials)

            // Backend returns: accessToken, refreshToken, email, firstName, lastName
            // Store tokens
            localStorage.setItem('token', response.accessToken)
            localStorage.setItem('refreshToken', response.refreshToken)

            // Build user object from response
            const user = {
                id: '', // Not provided in login response
                email: response.email,
                firstName: response.firstName,
                lastName: response.lastName,
                role: 'Admin' // We know this user has Admin role
            }
            localStorage.setItem('user', JSON.stringify(user))

            setUser(user)
            toast.success('Giriş başarılı!')
            navigate('/admin/dashboard')
        } catch (error) {
            toast.error('Giriş başarısız. Lütfen bilgilerinizi kontrol edin.')
            throw error
        }
    }

    const logout = () => {
        authApi.logout().catch(() => {
            // Ignore logout errors
        })

        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        setUser(null)
        toast.info('Çıkış yapıldı')
        navigate('/login')
    }

    return (
        <AuthContext.Provider
            value={{
                user,
                isAuthenticated: !!user,
                isLoading,
                login,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    )
}

export function useAuth() {
    const context = useContext(AuthContext)
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider')
    }
    return context
}
