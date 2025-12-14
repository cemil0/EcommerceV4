import { Navigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { Loader2 } from 'lucide-react'

export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
    const { isAuthenticated, isLoading } = useAuth()

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
            </div>
        )
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />
    }

    return <>{children}</>
}
