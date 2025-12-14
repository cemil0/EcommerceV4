import { useState } from 'react'
import { Outlet, Link, useLocation } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import {
    LayoutDashboard,
    Package,
    ShoppingCart,

    Menu,
    X,
    LogOut,
    ChevronDown,
} from 'lucide-react'
import { cn } from '@/lib/utils'

const navigation = [
    { name: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
    { name: 'Ürünler', href: '/admin/products', icon: Package },
    { name: 'Siparişler', href: '/admin/orders', icon: ShoppingCart },

]

export default function AdminLayout() {
    const [sidebarOpen, setSidebarOpen] = useState(false)
    const [userMenuOpen, setUserMenuOpen] = useState(false)
    const { user, logout } = useAuth()
    const location = useLocation()

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Mobile sidebar */}
            <div
                className={cn(
                    'fixed inset-0 z-40 lg:hidden',
                    sidebarOpen ? 'block' : 'hidden'
                )}
            >
                <div
                    className="fixed inset-0 bg-gray-600 bg-opacity-75"
                    onClick={() => setSidebarOpen(false)}
                />
                <div className="fixed inset-y-0 left-0 flex w-64 flex-col bg-white">
                    <div className="flex items-center justify-between h-16 px-4 border-b">
                        <span className="text-xl font-bold text-primary-600">Admin Panel</span>
                        <button
                            onClick={() => setSidebarOpen(false)}
                            className="text-gray-500 hover:text-gray-700"
                        >
                            <X className="w-6 h-6" />
                        </button>
                    </div>
                    <nav className="flex-1 px-2 py-4 space-y-1">
                        {navigation.map((item) => {
                            const isActive = location.pathname === item.href
                            return (
                                <Link
                                    key={item.name}
                                    to={item.href}
                                    onClick={() => setSidebarOpen(false)}
                                    className={cn(
                                        'flex items-center px-3 py-2 text-sm font-medium rounded-lg transition',
                                        isActive
                                            ? 'bg-primary-50 text-primary-700'
                                            : 'text-gray-700 hover:bg-gray-100'
                                    )}
                                >
                                    <item.icon className="w-5 h-5 mr-3" />
                                    {item.name}
                                </Link>
                            )
                        })}
                    </nav>
                </div>
            </div>

            {/* Desktop sidebar */}
            <div className="hidden lg:fixed lg:inset-y-0 lg:flex lg:w-64 lg:flex-col">
                <div className="flex flex-col flex-1 min-h-0 bg-white border-r">
                    <div className="flex items-center h-16 px-4 border-b">
                        <span className="text-xl font-bold text-primary-600">Admin Panel</span>
                    </div>
                    <nav className="flex-1 px-2 py-4 space-y-1 overflow-y-auto">
                        {navigation.map((item) => {
                            const isActive = location.pathname === item.href
                            return (
                                <Link
                                    key={item.name}
                                    to={item.href}
                                    className={cn(
                                        'flex items-center px-3 py-2 text-sm font-medium rounded-lg transition',
                                        isActive
                                            ? 'bg-primary-50 text-primary-700'
                                            : 'text-gray-700 hover:bg-gray-100'
                                    )}
                                >
                                    <item.icon className="w-5 h-5 mr-3" />
                                    {item.name}
                                </Link>
                            )
                        })}
                    </nav>
                </div>
            </div>

            {/* Main content */}
            <div className="lg:pl-64">
                {/* Top bar */}
                <div className="sticky top-0 z-10 flex h-16 bg-white border-b">
                    <button
                        type="button"
                        className="px-4 text-gray-500 lg:hidden"
                        onClick={() => setSidebarOpen(true)}
                    >
                        <Menu className="w-6 h-6" />
                    </button>

                    <div className="flex items-center justify-between flex-1 px-4">
                        <div className="flex-1" />

                        <div className="relative">
                            <button
                                onClick={() => setUserMenuOpen(!userMenuOpen)}
                                className="flex items-center space-x-3 px-3 py-2 rounded-lg hover:bg-gray-100 transition"
                            >
                                <div className="w-8 h-8 rounded-full bg-primary-600 flex items-center justify-center text-white font-medium">
                                    {user?.firstName?.[0]}{user?.lastName?.[0]}
                                </div>
                                <div className="hidden md:block text-left">
                                    <div className="text-sm font-medium text-gray-700">
                                        {user?.firstName} {user?.lastName}
                                    </div>
                                    <div className="text-xs text-gray-500">{user?.role}</div>
                                </div>
                                <ChevronDown className="w-4 h-4 text-gray-500" />
                            </button>

                            {userMenuOpen && (
                                <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border py-1">
                                    <button
                                        onClick={logout}
                                        className="flex items-center w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                                    >
                                        <LogOut className="w-4 h-4 mr-2" />
                                        Çıkış Yap
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Page content */}
                <main className="p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    )
}
