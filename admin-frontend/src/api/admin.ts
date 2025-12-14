import apiClient from './client'
import type { AdminDashboardDto, PagedRequest, PagedResponse, AdminProductDto, AdminProductDetailDto, AdminOrderDto, AdminOrderDetailDto, OrderFilterRequest, CreateProductRequest, UpdateProductRequest, CategoryDto, SalesChartDto, TopSellingProductDto } from '@/types/api'

export const dashboardApi = {
    getDashboard: async (startDate?: string): Promise<AdminDashboardDto> => {
        const params = startDate ? { startDate } : {}
        const response = await apiClient.get<AdminDashboardDto>('/Admin/Dashboard', { params })
        return response.data
    },

    getDashboardChart: async (days: number = 30): Promise<SalesChartDto> => {
        const response = await apiClient.get('/Admin/Orders/chart', { params: { days } })
        return response.data
    },

    getTopSellingProducts: async (count: number = 5, startDate?: string): Promise<TopSellingProductDto[]> => {
        const params: any = { count }
        if (startDate) params.startDate = startDate
        const response = await apiClient.get('/Admin/Orders/top-selling', { params })
        return response.data
    }
}

export const productsApi = {
    getProducts: async (params: PagedRequest): Promise<PagedResponse<AdminProductDto>> => {
        const response = await apiClient.get<PagedResponse<AdminProductDto>>('/Admin/Products', {
            params,
        })
        return response.data
    },


    getProduct: async (id: number): Promise<AdminProductDetailDto> => {
        const response = await apiClient.get<AdminProductDetailDto>(`/Admin/Products/${id}`)
        return response.data
    },

    updateProduct: async (id: number, data: UpdateProductRequest): Promise<AdminProductDto> => {
        const response = await apiClient.put<AdminProductDto>(`/Admin/Products/${id}`, data)
        return response.data
    },

    updateVariant: async (productId: number, variantId: number, data: { variantName: string; price: number; isActive: boolean }): Promise<void> => {
        await apiClient.put(`/Admin/Products/${productId}/variants/${variantId}`, data)
    },

    updateStock: async (productId: number, variantId: number, quantity: number): Promise<void> => {
        await apiClient.put(`/Admin/Products/${productId}/variants/${variantId}/stock`, { quantity })
    },

    createProduct: async (data: CreateProductRequest): Promise<AdminProductDto> => {
        const response = await apiClient.post<AdminProductDto>('/Admin/Products', data)
        return response.data
    },

    activateProduct: async (id: number): Promise<void> => {
        await apiClient.put(`/Admin/Products/${id}/activate`)
    },

    deactivateProduct: async (id: number): Promise<void> => {
        await apiClient.put(`/Admin/Products/${id}/deactivate`)
    },

    deleteProduct: async (id: number): Promise<void> => {
        await apiClient.delete(`/Admin/Products/${id}`)
    },

    uploadImages: async (id: number, files: File[]): Promise<void> => {
        const formData = new FormData()
        files.forEach((file) => {
            formData.append('files', file)
        })
        await apiClient.post(`/Admin/Products/${id}/images/bulk`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        })
    },

    deleteImage: async (productId: number, imageId: number): Promise<void> => {
        await apiClient.delete(`/Admin/Products/${productId}/images/${imageId}`)
    },

    bulkUpload: async (file: File): Promise<BulkImportResultDto> => {
        const formData = new FormData()
        formData.append('file', file)
        const response = await apiClient.post<BulkImportResultDto>('/Admin/Products/bulk-upload', formData, {
            headers: {
                'Content-Type': undefined,
            },
        })
        return response.data
    },

    getSampleExcel: async (): Promise<Blob> => {
        // Since there isn't a dedicated endpoint for sample, we can suggest creating one or just download a static file if it existed.
        // Checking backend controller, there is no explicit sample download.
        // I will implement a frontend-generated sample download in the component instead of an API call if backend doesn't support it,
        // OR I can assume one exists. For now, let's skip API method for sample and generate it on frontend or just assume 
        // the user will create one. But wait, I can add a method if I create the endpoint.
        // Actually, the plan said "Template Download". If backend has no endpoint, I'll generate it in frontend using 'xlsx' lib or just a static file link?
        // Let's stick to just bulkUpload here.
        throw new Error("Not implemented")
    }
}

export const ordersApi = {
    getOrders: async (params: OrderFilterRequest): Promise<PagedResponse<AdminOrderDto>> => {
        const response = await apiClient.get<PagedResponse<AdminOrderDto>>('/Admin/Orders', {
            params,
        })
        return response.data
    },

    getOrderDetail: async (id: number): Promise<AdminOrderDetailDto> => {
        const response = await apiClient.get<AdminOrderDetailDto>(`/Admin/Orders/${id}`)
        return response.data
    },

    updateOrderStatus: async (id: number, newStatus: string): Promise<void> => {
        await apiClient.put(`/Admin/Orders/${id}/status`, { newStatus })
    },
}

export const categoriesApi = {
    getCategories: async (): Promise<CategoryDto[]> => {
        const response = await apiClient.get<CategoryDto[]>('/Categories')
        return response.data
    }
}
