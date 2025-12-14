import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './features/auth/AuthContext'
import Login from './features/auth/Login'
import AdminLayout from './layouts/AdminLayout'
import Dashboard from './features/dashboard/Dashboard'
import ProductList from './features/products/ProductList'
import ProductAdd from './features/products/ProductAdd'
import ProductEdit from './features/products/ProductEdit'
import ProductDetail from './features/products/ProductDetail'
import ProductImport from './features/products/ProductImport'
import OrderList from './features/orders/OrderList'
import OrderDetail from './features/orders/OrderDetail'
import ProtectedRoute from './components/ProtectedRoute'

function App() {
    return (
        <BrowserRouter>
            <AuthProvider>
                <Routes>
                    <Route path="/login" element={<Login />} />

                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute>
                                <AdminLayout />
                            </ProtectedRoute>
                        }
                    >
                        <Route index element={<Navigate to="/admin/dashboard" replace />} />
                        <Route path="dashboard" element={<Dashboard />} />
                        <Route path="products" element={<ProductList />} />
                        <Route path="products/new" element={<ProductAdd />} />
                        <Route path="products/import" element={<ProductImport />} />
                        <Route path="products/edit/:id" element={<ProductEdit />} />
                        <Route path="products/:id" element={<ProductDetail />} />
                        <Route path="orders" element={<OrderList />} />
                        <Route path="orders/:id" element={<OrderDetail />} />
                    </Route>

                    <Route path="/" element={<Navigate to="/admin/dashboard" replace />} />
                    <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
                </Routes>
            </AuthProvider>
        </BrowserRouter>
    )
}

export default App
