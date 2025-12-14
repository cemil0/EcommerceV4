import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { productsApi, categoriesApi } from '@/api/admin'
import { ArrowLeft, Save, Upload, X } from 'lucide-react'
import { toast } from 'sonner'
import type { UpdateProductRequest } from '@/types/api'

export default function ProductEdit() {
    const { id } = useParams()
    const productId = Number(id)
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [selectedFiles, setSelectedFiles] = useState<File[]>([])
    const [previews, setPreviews] = useState<string[]>([])

    const [formData, setFormData] = useState<UpdateProductRequest & { basePrice: string | number }>({
        productName: '',
        sku: '',
        categoryId: 0,
        basePrice: 0,
        shortDescription: '',
        longDescription: '',
        brand: '',
        manufacturer: '',
        model: '',
        metaTitle: '',
        metaDescription: '',
        metaKeywords: '',
        isFeatured: false,
        isNewArrival: false,
        isActive: true,
    })

    // Fetch categories
    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: categoriesApi.getCategories,
    })

    // Fetch existing product data
    const { data: product, isLoading: isProductLoading } = useQuery({
        queryKey: ['product', productId],
        queryFn: () => productsApi.getProduct(productId),
        enabled: !!productId,
    })

    // Populate form when product data is loaded
    useEffect(() => {
        if (product) {
            setFormData({
                productName: product.productName,
                sku: product.sku,
                categoryId: product.categoryId,
                basePrice: product.basePrice,
                shortDescription: product.shortDescription || product.description || '',
                longDescription: product.description || '',
                brand: product.brand || '',
                manufacturer: product.manufacturer || '',
                model: product.model || '',
                metaTitle: '',
                metaDescription: '',
                metaKeywords: '',
                isFeatured: product.isFeatured || false,
                isNewArrival: product.isNewArrival || false,
                isActive: product.isActive,
            })
        }
    }, [product])

    const uploadImagesMutation = useMutation({
        mutationFn: async ({ id, files }: { id: number; files: File[] }) => {
            return productsApi.uploadImages(id, files)
        },
    })

    const updateMutation = useMutation({
        mutationFn: (data: UpdateProductRequest) => productsApi.updateProduct(productId, {
            ...data,
            basePrice: Number(data.basePrice), // Ensure number on submit
        }),
        onSuccess: async () => {
            if (selectedFiles.length > 0) {
                try {
                    toast.info('Ürün güncellendi, yeni resimler yükleniyor...')
                    await uploadImagesMutation.mutateAsync({
                        id: productId,
                        files: selectedFiles,
                    })
                } catch (error) {
                    toast.error('Resimler yüklenirken hata oluştu.')
                }
            }

            queryClient.invalidateQueries({ queryKey: ['products'] })
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            toast.success('Ürün başarıyla güncellendi')
            navigate(`/admin/products/${productId}`)
        },
        onError: (error) => {
            console.error(error)
            toast.error('Ürün güncellenirken bir hata oluştu')
        },
    })

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        if (formData.categoryId === 0) {
            toast.error('Lütfen bir kategori seçin')
            return
        }
        updateMutation.mutate(formData)
    }

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target
        setFormData((prev) => ({
            ...prev,
            [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked :
                name === 'categoryId' ? Number(value) : value, // Keep basePrice as string until submit
        }))
    }

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files)
            setSelectedFiles((prev) => [...prev, ...newFiles])

            // Generate previews
            const newPreviews = newFiles.map(file => URL.createObjectURL(file))
            setPreviews((prev) => [...prev, ...newPreviews])
        }
    }

    const removeFile = (index: number) => {
        setSelectedFiles((prev) => prev.filter((_, i) => i !== index))
        setPreviews((prev) => {
            URL.revokeObjectURL(prev[index])
            return prev.filter((_, i) => i !== index)
        })
    }

    const deleteImageMutation = useMutation({
        mutationFn: (imageId: number) => productsApi.deleteImage(productId, imageId),
        onSuccess: () => {
            toast.success('Resim silindi')
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
        },
        onError: () => toast.error('Resim silinemedi')
    })

    const handleDeleteImage = (imageId: number) => {
        if (confirm('Bu resmi silmek istediğinize emin misiniz?')) {
            deleteImageMutation.mutate(imageId)
        }
    }

    const isSubmitting = updateMutation.isPending || uploadImagesMutation.isPending

    if (isProductLoading) {
        return <div>Yükleniyor...</div>
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <button
                        onClick={() => navigate(`/admin/products/${productId}`)}
                        className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                    >
                        <ArrowLeft className="w-6 h-6 text-gray-600" />
                    </button>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Ürünü Düzenle</h1>
                        <p className="text-gray-600 mt-1">{product?.productName}</p>
                    </div>
                </div>
                <button
                    onClick={handleSubmit}
                    disabled={isSubmitting}
                    className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
                >
                    <Save className="w-5 h-5 mr-2" />
                    {isSubmitting ? 'Kaydediliyor...' : 'Değişiklikleri Kaydet'}
                </button>
            </div>

            <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Main Info */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Temel Bilgiler</h2>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Ürün Adı *</label>
                                <input
                                    type="text"
                                    name="productName"
                                    required
                                    value={formData.productName}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700">SKU (Stok Kodu) *</label>
                                <input
                                    type="text"
                                    name="sku"
                                    required
                                    value={formData.sku}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Kategori *</label>
                                <select
                                    name="categoryId"
                                    required
                                    value={formData.categoryId}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                >
                                    <option value={0}>Seçiniz</option>
                                    {categories?.map((cat) => (
                                        <option key={cat.categoryId} value={cat.categoryId}>
                                            {cat.categoryName}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Başlangıç Fiyatı (₺) *</label>
                                <input
                                    type="number"
                                    name="basePrice"
                                    required
                                    min="0"
                                    step="0.01"
                                    value={formData.basePrice}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Kısa Açıklama</label>
                            <textarea
                                name="shortDescription"
                                rows={3}
                                value={formData.shortDescription}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Detaylı Açıklama</label>
                            <textarea
                                name="longDescription"
                                rows={6}
                                value={formData.longDescription}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                    </div>

                    {/* Image Upload */}
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Yeni Görsel Ekle</h2>

                        {/* Existing Images Display */}
                        {product?.images && product.images.length > 0 && (
                            <div className="mb-4">
                                <h3 className="text-sm font-medium text-gray-700 mb-2">Mevcut Görseller</h3>
                                <div className="grid grid-cols-4 gap-2">
                                    {product.images.map((img) => (
                                        <div key={img.imageId} className="relative group aspect-square">
                                            <img src={`http://localhost:5000${img.thumbnailUrl || img.imageUrl}`} className="w-full h-full object-cover rounded-md" />
                                            <button
                                                type="button"
                                                onClick={() => handleDeleteImage(img.imageId)}
                                                className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                                                title="Resmi Sil"
                                            >
                                                <X className="w-4 h-4" />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                            {previews.map((preview, index) => (
                                <div key={index} className="relative group aspect-square bg-gray-100 rounded-lg overflow-hidden border border-gray-200">
                                    <img src={preview} alt={`Preview ${index}`} className="w-full h-full object-cover" />
                                    <button
                                        type="button"
                                        onClick={() => removeFile(index)}
                                        className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                                    >
                                        <X className="w-4 h-4" />
                                    </button>
                                </div>
                            ))}
                            <label className="flex flex-col items-center justify-center aspect-square border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-500 hover:bg-primary-50 transition-colors">
                                <div className="flex flex-col items-center justify-center pt-5 pb-6">
                                    <Upload className="w-8 h-8 text-gray-400 mb-2" />
                                    <p className="text-xs text-gray-500">Resim Ekle</p>
                                </div>
                                <input
                                    type="file"
                                    className="hidden"
                                    multiple
                                    accept="image/*"
                                    onChange={handleFileSelect}
                                />
                            </label>
                        </div>
                    </div>

                    {/* Variant Management Section */}
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Varyant Yönetimi</h2>
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Varyant</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fiyat (₺)</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Stok</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Durum</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {product?.variants?.map((variant) => (
                                        <VariantRow key={variant.variantId} productId={productId} variant={variant} />
                                    ))}
                                </tbody>
                            </table>
                            {(!product?.variants || product.variants.length === 0) && (
                                <p className="text-center py-4 text-gray-500">Varyant bulunamadı.</p>
                            )}
                        </div>
                    </div>
                </div>

                {/* Sidebar Info */}
                <div className="space-y-6">
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Ürün Detayları</h2>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Marka</label>
                            <input
                                type="text"
                                name="brand"
                                value={formData.brand}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Üretici</label>
                            <input
                                type="text"
                                name="manufacturer"
                                value={formData.manufacturer}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Model</label>
                            <input
                                type="text"
                                name="model"
                                value={formData.model}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Görünürlük</h2>

                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isActive"
                                id="isActive"
                                checked={formData.isActive}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                                Aktif (Satışta)
                            </label>
                        </div>
                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isFeatured"
                                id="isFeatured"
                                checked={formData.isFeatured}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isFeatured" className="ml-2 block text-sm text-gray-900">
                                Öne Çıkan Ürün
                            </label>
                        </div>
                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isNewArrival"
                                id="isNewArrival"
                                checked={formData.isNewArrival}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isNewArrival" className="ml-2 block text-sm text-gray-900">
                                Yeni Gelenler
                            </label>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    )
}

// Sub-component for individual variant row editing
function VariantRow({ productId, variant }: { productId: number, variant: any }) {
    const [price, setPrice] = useState<string | number>(variant.price)
    const [stock, setStock] = useState<string | number>(variant.stockQuantity)
    const [isActive, setIsActive] = useState(variant.isActive)
    const [isDirty, setIsDirty] = useState(false)
    const queryClient = useQueryClient()

    const updateVariantMutation = useMutation({
        mutationFn: async () => {
            // Parallel updates for price (variant update) and stock
            await productsApi.updateVariant(productId, variant.variantId, {
                variantName: variant.variantName,
                price: Number(price),
                isActive: isActive
            })
            await productsApi.updateStock(productId, variant.variantId, Number(stock))
        },
        onSuccess: () => {
            toast.success('Varyant güncellendi')
            setIsDirty(false)
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
        },
        onError: () => toast.error('Varyant güncellenemedi')
    })

    const handleSave = () => {
        updateVariantMutation.mutate()
    }

    return (
        <tr className="hover:bg-gray-50">
            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{variant.variantName}</td>
            <td className="px-6 py-4 whitespace-nowrap">
                <input
                    type="number"
                    value={price}
                    onChange={(e) => { setPrice(e.target.value); setIsDirty(true) }}
                    className="w-24 rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                />
            </td>
            <td className="px-6 py-4 whitespace-nowrap">
                <input
                    type="number"
                    value={stock}
                    onChange={(e) => { setStock(e.target.value); setIsDirty(true) }}
                    className="w-24 rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                />
            </td>
            <td className="px-6 py-4 whitespace-nowrap">
                <div className="flex items-center">
                    <input
                        type="checkbox"
                        checked={isActive}
                        onChange={(e) => { setIsActive(e.target.checked); setIsDirty(true) }}
                        className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-500">{isActive ? 'Aktif' : 'Pasif'}</span>
                </div>
            </td>
            <td className="px-6 py-4 whitespace-nowrap">
                {isDirty && (
                    <button
                        type="button"
                        onClick={handleSave}
                        className="text-primary-600 hover:text-primary-900 font-medium"
                    >
                        Kaydet
                    </button>
                )}
            </td>
        </tr>
    )
}
