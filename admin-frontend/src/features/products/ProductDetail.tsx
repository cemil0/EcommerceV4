import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { productsApi } from '@/api/admin'
import { ArrowLeft, Edit, Package, Grid, DollarSign, Tag, Calendar, CheckCircle, XCircle } from 'lucide-react'
import { formatCurrency } from '@/lib/utils'

export default function ProductDetail() {
    const { id } = useParams()
    const navigate = useNavigate()
    const productId = Number(id)
    const [selectedImage, setSelectedImage] = useState<string | null>(null)

    const { data: product, isLoading, isError } = useQuery({
        queryKey: ['product', productId],
        queryFn: () => productsApi.getProduct(productId),
        enabled: !!productId,
    })

    // Set first image as selected when loaded
    if (product?.images && product.images.length > 0 && !selectedImage) {
        setSelectedImage(product.images[0].imageUrl)
    }

    if (isLoading) {
        return (
            <div className="flex justify-center items-center h-96">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
        )
    }

    if (isError || !product) {
        return (
            <div className="text-center py-12">
                <h2 className="text-2xl font-bold text-gray-900">Ürün bulunamadı</h2>
                <button
                    onClick={() => navigate('/admin/products')}
                    className="mt-4 text-primary-600 hover:text-primary-800"
                >
                    Ürün listesine dön
                </button>
            </div>
        )
    }

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Link
                        to="/admin/products"
                        className="p-2 rounded-full hover:bg-gray-100 transition-colors"
                    >
                        <ArrowLeft className="w-6 h-6 text-gray-600" />
                    </Link>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">{product.productName}</h1>
                        <p className="text-gray-500 text-sm">SKU: {product.sku}</p>
                    </div>
                </div>
                <div className="flex space-x-3">
                    <span className={`px-3 py-1 rounded-full text-sm font-medium flex items-center ${product.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                        {product.isActive ? (
                            <><CheckCircle className="w-4 h-4 mr-1" /> Aktif</>
                        ) : (
                            <><XCircle className="w-4 h-4 mr-1" /> Pasif</>
                        )}
                    </span>
                    {/* Edit button placeholder - for future use */}
                    <Link
                        to={`/admin/products/edit/${product.productId}`}
                        className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                    >
                        <Edit className="w-4 h-4 mr-2" />
                        Düzenle
                    </Link>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Main Info */}
                <div className="lg:col-span-2 space-y-6">
                    {/* Image Gallery */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">Ürün Görselleri</h2>
                        {product.images && product.images.length > 0 ? (
                            <div className="space-y-4">
                                <div className="aspect-video bg-gray-100 rounded-lg overflow-hidden flex items-center justify-center border border-gray-200">
                                    <img
                                        src={`http://localhost:5000${selectedImage || product.images[0].imageUrl}`}
                                        alt={product.productName}
                                        className="max-h-96 w-auto object-contain"
                                    />
                                </div>
                                <div className="grid grid-cols-6 gap-2">
                                    {product.images.map((img) => (
                                        <button
                                            key={img.imageId}
                                            onClick={() => setSelectedImage(img.imageUrl)}
                                            className={`aspect-square rounded-md overflow-hidden border-2 transition-colors ${selectedImage === img.imageUrl ? 'border-primary-500' : 'border-transparent hover:border-gray-300'
                                                }`}
                                        >
                                            <img src={`http://localhost:5000${img.thumbnailUrl || img.imageUrl}`} alt="Thumbnail" className="w-full h-full object-cover" />
                                        </button>
                                    ))}
                                </div>
                            </div>
                        ) : (
                            <div className="p-8 text-center text-gray-500 bg-gray-50 rounded-lg border-2 border-dashed border-gray-200">
                                <p>Görsel bulunmuyor</p>
                            </div>
                        )}
                    </div>

                    {/* Basic Details */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">Genel Bilgiler</h2>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Ürün Adı</label>
                                <div className="mt-1 text-gray-900">{product.productName}</div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Kategori</label>
                                <div className="mt-1 flex items-center text-gray-900">
                                    <Grid className="w-4 h-4 mr-2 text-gray-400" />
                                    {product.categoryName}
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Baz Fiyat</label>
                                <div className="mt-1 flex items-center text-gray-900 font-semibold text-lg">
                                    <DollarSign className="w-4 h-4 mr-1 text-gray-400" />
                                    {formatCurrency(product.basePrice)}
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Oluşturulma Tarihi</label>
                                <div className="mt-1 flex items-center text-gray-900">
                                    <Calendar className="w-4 h-4 mr-2 text-gray-400" />
                                    {new Date(product.createdAt).toLocaleDateString('tr-TR')}
                                </div>
                            </div>
                        </div>

                        {/* Feature Badges */}
                        <div className="mt-4 flex gap-2">
                            {product.isFeatured && (
                                <span className="px-3 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                                    ⭐ Öne Çıkan
                                </span>
                            )}
                            {product.isNewArrival && (
                                <span className="px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                    🆕 Yeni Ürün
                                </span>
                            )}
                        </div>

                        <div className="mt-6">
                            <label className="block text-sm font-medium text-gray-500">Açıklama</label>
                            <div className="mt-2 text-gray-700 bg-gray-50 p-4 rounded-md">
                                {product.description || 'Açıklama bulunmuyor.'}
                            </div>
                        </div>
                    </div>

                    {/* Product Details - Brand, Manufacturer, Model */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">Ürün Detayları</h2>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Marka</label>
                                <div className="mt-1 text-gray-900 font-medium">
                                    {product.brand || <span className="text-gray-400 italic">Belirtilmemiş</span>}
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Üretici</label>
                                <div className="mt-1 text-gray-900 font-medium">
                                    {product.manufacturer || <span className="text-gray-400 italic">Belirtilmemiş</span>}
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-500">Model</label>
                                <div className="mt-1 text-gray-900 font-medium">
                                    {product.model || <span className="text-gray-400 italic">Belirtilmemiş</span>}
                                </div>
                            </div>
                        </div>
                        {product.shortDescription && (
                            <div className="mt-6">
                                <label className="block text-sm font-medium text-gray-500">Kısa Açıklama</label>
                                <div className="mt-2 text-gray-700 bg-gray-50 p-4 rounded-md">
                                    {product.shortDescription}
                                </div>
                            </div>
                        )}
                    </div>

                    {/* Variants */}
                    <div className="bg-white rounded-lg shadow overflow-hidden">
                        <div className="px-6 py-4 border-b border-gray-200">
                            <h2 className="text-lg font-semibold text-gray-900">Varyantlar ve Stok</h2>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Varyant Adı</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">SKU</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Fiyat</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Stok</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Durum</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {product.variants.map((variant) => (
                                        <tr key={variant.variantId} className="hover:bg-gray-50">
                                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                                {variant.variantName}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {variant.sku}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                                {formatCurrency(variant.price)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${variant.stockQuantity > 10 ? 'bg-blue-100 text-blue-800' :
                                                    variant.stockQuantity > 0 ? 'bg-yellow-100 text-yellow-800' :
                                                        'bg-red-100 text-red-800'
                                                    }`}>
                                                    {variant.stockQuantity} adet
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {variant.isActive ? (
                                                    <span className="text-green-600 flex items-center"><CheckCircle className="w-3 h-3 mr-1" /> Aktif</span>
                                                ) : (
                                                    <span className="text-red-600 flex items-center"><XCircle className="w-3 h-3 mr-1" /> Pasif</span>
                                                )}
                                            </td>
                                        </tr>
                                    ))}
                                    {product.variants.length === 0 && (
                                        <tr>
                                            <td colSpan={5} className="px-6 py-4 text-center text-gray-500">Varyant bulunamadı</td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Sidebar */}
                <div className="space-y-6">
                    {/* Summary Card */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 mb-4">Özet</h2>
                        <div className="space-y-4">
                            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                                <div className="flex items-center text-gray-600">
                                    <Package className="w-5 h-5 mr-3" />
                                    <span>Toplam Stok</span>
                                </div>
                                <span className="font-semibold text-gray-900">
                                    {product.variants.reduce((acc, v) => acc + v.stockQuantity, 0)}
                                </span>
                            </div>
                            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                                <div className="flex items-center text-gray-600">
                                    <Tag className="w-5 h-5 mr-3" />
                                    <span>Varyant Sayısı</span>
                                </div>
                                <span className="font-semibold text-gray-900">
                                    {product.variants.length}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}
