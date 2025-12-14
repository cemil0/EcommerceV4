export interface User {
    id: string
    email: string
    firstName: string
    lastName: string
    role: string
}

export interface LoginRequest {
    email: string
    password: string
}

export interface LoginResponse {
    accessToken: string
    refreshToken: string
    accessTokenExpiration: string
    refreshTokenExpiration: string
    email: string
    firstName: string
    lastName: string
}

export interface DashboardSummary {
    todayOrders: number
    todayRevenue: number
    totalCustomers: number
    totalProducts: number
    lowStockProducts: number
    pendingOrders: number
}

export interface RecentOrderSummary {
    orderId: number
    orderNumber: string
    customerName: string
    totalAmount: number
    status: string
    createdAt: string
}

export interface AdminDashboardDto {
    summary: DashboardSummary
    recentOrders: RecentOrderSummary[]
}

export interface PagedRequest {
    page: number
    pageSize: number
    searchTerm?: string
    sortBy?: string
    sortDescending?: boolean
    lowStock?: boolean
}

export interface PagedResponse<T> {
    data: T[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
    hasPrevious: boolean
    hasNext: boolean
}

export interface AdminProductDto {
    productId: number
    productName: string
    sku: string
    categoryId: number
    categoryName: string
    variantCount: number
    totalStock: number
    basePrice: number
    isActive: boolean
    createdAt: string
}

export interface AdminOrderDto {
    orderId: number
    orderNumber: string
    customerName: string
    companyName?: string
    totalAmount: number
    orderStatus: string
    paymentStatus: string
    createdAt: string
    itemCount: number
}

export interface OrderFilterRequest extends PagedRequest {
    status?: string
    startDate?: string
    endDate?: string
    customerType?: string
}

export interface CreateProductRequest {
    productName: string
    sku: string
    categoryId: number
    basePrice: number
    shortDescription?: string
    longDescription?: string
    brand?: string
    manufacturer?: string
    model?: string
    metaTitle?: string
    metaDescription?: string
    metaKeywords?: string
    isFeatured: boolean
    isNewArrival: boolean
    isActive: boolean
}

export interface CategoryDto {
    categoryId: number
    categoryName: string
    categorySlug: string
    parentCategoryId?: number
    isActive: boolean
    subCategories: CategoryDto[]
}

export interface AdminVariantDto {
    variantId: number
    variantName: string
    sku: string
    price: number
    stockQuantity: number
    isActive: boolean
}

export interface AdminProductDetailDto {
    productId: number
    productName: string
    description: string
    sku: string
    categoryId: number
    categoryName: string
    basePrice: number
    isActive: boolean

    // Product detail fields
    brand?: string
    manufacturer?: string
    model?: string
    shortDescription?: string
    isFeatured: boolean
    isNewArrival: boolean

    variants: AdminVariantDto[]
    images: ProductImageDto[]
    createdAt: string
}

export interface ProductImageDto {
    imageId: number
    productId: number
    imageUrl: string
    thumbnailUrl: string
    isPrimary: boolean
    displayOrder: number
    altText?: string
    createdAt: string
}

export interface UpdateProductRequest {
    productName: string
    sku: string
    categoryId: number
    basePrice: number
    shortDescription?: string
    longDescription?: string
    brand?: string
    manufacturer?: string
    model?: string
    metaTitle?: string
    metaDescription?: string
    metaKeywords?: string
    isFeatured: boolean
    isNewArrival: boolean
    isActive: boolean
}

export interface CustomerSummary {
    customerId: number
    fullName: string
    email: string
    phone: string
}

export interface CompanySummary {
    companyId: number
    companyName: string
    taxNumber: string
}

export interface AddressSummary {
    addressLine: string
    city: string
    district: string
    postalCode: string
}

export interface OrderItemSummary {
    orderItemId: number
    productName: string
    variantName: string
    quantity: number
    unitPrice: number
    totalPrice: number
}

export interface AdminOrderDetailDto {
    orderId: number
    orderNumber: string
    customer: CustomerSummary
    company?: CompanySummary
    items: OrderItemSummary[]
    subTotal: number
    taxAmount: number
    shippingCost: number
    totalAmount: number
    orderStatus: string
    paymentStatus: string
    shippingAddress: AddressSummary
    createdAt: string
}

export interface SalesChartPointDto {
    date: string
    totalRevenue: number
    orderCount: number
}

export interface SalesChartDto {
    data: SalesChartPointDto[]
    totalRevenueInPeriod: number
    totalOrdersInPeriod: number
}

export interface TopSellingProductDto {
    productName: string
    totalQuantity: number
}

export interface BulkImportErrorDto {
    rowNumber: number
    sku?: string
    field?: string
    message: string
    severity: string
}

export interface BulkImportSuccessDto {
    rowNumber: number
    sku?: string
    message: string
    action: string
}

export interface BulkImportResultDto {
    totalRows: number
    successCount: number
    failedCount: number
    processingTimeMs: number
    errors: BulkImportErrorDto[]
    warnings: { rowNumber: number; sku?: string; message: string }[]
    successes: BulkImportSuccessDto[]
}
