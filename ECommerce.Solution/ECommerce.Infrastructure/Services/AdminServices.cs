using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class AdminProductService : IAdminProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<AdminProductDto>> GetProductsAsync(PagedRequest request)
    {
        var products = await _unitOfWork.Products.GetAllWithDetailsAsync();
        
        // Apply Search Filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            products = products.Where(p => 
                p.ProductName.ToLower().Contains(term) || 
                p.SKU.ToLower().Contains(term)
            );
        }

        if (request.LowStock)
        {
            // Filter products where TotalStock is <= 30
            // Since TotalStock is calculated from variants, we need to inspect them.
            // Note: EF Core translation might be tricky with Sum() in Where() depending on provider,
            // but for In-Memory/simple providers it works. For optimize SQL, ensure navigation property is loaded.
            products = products.Where(p => p.ProductVariants != null && p.ProductVariants.Sum(v => v.StockQuantity) <= 30);
        }

        var totalCount = products.Count();
        
        var productDtos = products
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new AdminProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                SKU = p.SKU,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                BasePrice = p.ProductVariants?.FirstOrDefault()?.BasePrice ?? 0,
                TotalStock = p.ProductVariants?.Sum(v => v.StockQuantity) ?? 0,
                IsActive = p.IsActive,
                VariantCount = p.ProductVariants?.Count ?? 0,
                CreatedAt = p.CreatedAt
            })
            .ToList();

        return new PagedResponse<AdminProductDto>
        {
            Data = productDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AdminProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new ECommerce.Domain.Entities.Product
        {
            ProductName = request.ProductName,
            SKU = request.SKU,
            CategoryId = request.CategoryId,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Brand = request.Brand,
            Manufacturer = request.Manufacturer,
            Model = request.Model,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            IsFeatured = request.IsFeatured,
            IsNewArrival = request.IsNewArrival,
            IsActive = request.IsActive,
            ProductSlug = GenerateSlug(request.ProductName),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsVariantProduct = true
        };

        // Create default variant for BasePrice
        var defaultVariant = new ECommerce.Domain.Entities.ProductVariant
        {
            VariantName = "Default",
            VariantSKU = request.SKU,
            BasePrice = request.BasePrice,
            StockQuantity = 0, 
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        product.ProductVariants.Add(defaultVariant);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new AdminProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SKU = product.SKU,
            CategoryId = product.CategoryId,
            BasePrice = request.BasePrice,
            TotalStock = 0,
            IsActive = product.IsActive,
            VariantCount = 1,
            CreatedAt = product.CreatedAt
        };

    }

    public async Task<AdminProductDto?> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId);
        if (product == null)
            return null;

        // Update properties
        product.ProductName = request.ProductName;
        product.SKU = request.SKU;
        product.CategoryId = request.CategoryId;
        product.ShortDescription = request.ShortDescription;
        product.LongDescription = request.LongDescription;
        product.Brand = request.Brand;
        product.Manufacturer = request.Manufacturer;
        product.Model = request.Model;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.MetaKeywords = request.MetaKeywords;
        product.IsFeatured = request.IsFeatured;
        product.IsNewArrival = request.IsNewArrival;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        // Update default variant base price if exists
        var defaultVariant = product.ProductVariants.FirstOrDefault(v => v.VariantName == "Default" || v.IsDefault);
        if (defaultVariant != null)
        {
            defaultVariant.BasePrice = request.BasePrice;
            defaultVariant.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.ProductVariants.Update(defaultVariant);
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return new AdminProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SKU = product.SKU,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.CategoryName ?? "",
            BasePrice = request.BasePrice,
            TotalStock = product.ProductVariants.Sum(v => v.StockQuantity),
            IsActive = product.IsActive,
            VariantCount = product.ProductVariants.Count,
            CreatedAt = product.CreatedAt
        };
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("\"", "");
            
        return $"{slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    }

    public async Task<AdminProductDetailDto?> GetProductDetailAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId);
        if (product == null)
            return null;

        return new AdminProductDetailDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Description = product.LongDescription ?? product.ShortDescription ?? "",
            SKU = product.SKU,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.CategoryName ?? "",
            BasePrice = product.ProductVariants?.FirstOrDefault()?.BasePrice ?? 0,
            IsActive = product.IsActive,
            
            // New product detail fields
            Brand = product.Brand,
            Manufacturer = product.Manufacturer,
            Model = product.Model,
            ShortDescription = product.ShortDescription,
            IsFeatured = product.IsFeatured,
            IsNewArrival = product.IsNewArrival,
            
            Variants = product.ProductVariants?.Select(v => new AdminVariantDto
            {
                VariantId = v.ProductVariantId,
                VariantName = v.VariantName,
                SKU = v.VariantSKU,
                Price = v.BasePrice,
                StockQuantity = v.StockQuantity,
                IsActive = v.IsActive

            }).ToList() ?? new List<AdminVariantDto>(),
            Images = product.ProductImages?.Select(i => new ProductImageDto
            {
                ImageId = i.ImageId,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                AltText = i.AltText
            }).OrderBy(i => i.DisplayOrder).ToList() ?? new List<ProductImageDto>(),
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<bool> ActivateProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeactivateProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId);
        if (product == null)
            return false;

        // Hard delete: Remove product images from storage and database
        if (product.ProductImages != null && product.ProductImages.Any())
        {
            var fileService = new AdvancedFileStorageService();
            foreach (var image in product.ProductImages.ToList())
            {
                try
                {
                    await fileService.DeleteFileAsync(image.ImageUrl);
                }
                catch { /* Ignore file deletion errors */ }
                _unitOfWork.ProductImages.Remove(image);
            }
        }

        // Remove all variants
        if (product.ProductVariants != null)
        {
            foreach (var variant in product.ProductVariants.ToList())
            {
                _unitOfWork.ProductVariants.Remove(variant);
            }
        }
        
        // Remove the product itself
        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    // Variant CRUD
    public async Task<AdminVariantDto?> CreateVariantAsync(int productId, CreateVariantRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return null;

        var variant = new Domain.Entities.ProductVariant
        {
            ProductId = productId,
            VariantName = request.VariantName,
            VariantSKU = request.SKU,
            BasePrice = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductVariants.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        return new AdminVariantDto
        {
            VariantId = variant.ProductVariantId,
            VariantName = variant.VariantName,
            SKU = variant.VariantSKU,
            Price = variant.BasePrice,
            StockQuantity = variant.StockQuantity,
            IsActive = variant.IsActive
        };
    }

    public async Task<bool> UpdateVariantAsync(int productId, int variantId, UpdateVariantRequest request)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId);
        if (variant == null || variant.ProductId != productId)
            return false;

        variant.VariantName = request.VariantName;
        variant.BasePrice = request.Price;
        variant.IsActive = request.IsActive;
        variant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteVariantAsync(int productId, int variantId)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId);
        if (variant == null || variant.ProductId != productId)
            return false;

        // Soft delete
        variant.IsActive = false;
        variant.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Stock Management
    public async Task<bool> UpdateStockAsync(int productId, int variantId, UpdateStockRequest request)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId);
        if (variant == null || variant.ProductId != productId)
            return false;

        variant.StockQuantity = request.Quantity;
        variant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> BulkUpdateStockAsync(BulkStockUpdateRequest request)
    {
        foreach (var item in request.Items)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.VariantId);
            if (variant != null)
            {
                variant.StockQuantity = item.Quantity;
                variant.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ProductVariants.Update(variant);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    // Image Management
    public async Task<ProductImageDto> UploadImageAsync(int productId, Stream imageStream, string fileName, UploadImageRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found");

        // Save file with WebP conversion and thumbnail
        var fileService = new AdvancedFileStorageService();
        var imageUrl = await fileService.SaveFileAsync(imageStream, fileName, $"products/{productId}");

        // Create ProductImage entity
        var productImage = new Domain.Entities.ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = request.SetAsPrimary,
            AltText = request.AltText,
            DisplayOrder = 0, // Will be set based on existing images
            CreatedAt = DateTime.UtcNow
        };

        // If this is the first image or set as primary, make it primary
        var existingImages = await _unitOfWork.ProductImages.GetByProductIdAsync(productId);
        if (!existingImages.Any() || request.SetAsPrimary)
        {
            productImage.IsPrimary = true;
            // Remove primary from others
            foreach (var img in existingImages.Where(i => i.IsPrimary))
            {
                img.IsPrimary = false;
                _unitOfWork.ProductImages.Update(img);
            }
        }

        productImage.DisplayOrder = existingImages.Count();

        await _unitOfWork.ProductImages.AddAsync(productImage);
        await _unitOfWork.SaveChangesAsync();

        return new ProductImageDto
        {
            ImageId = productImage.ImageId,
            ProductId = productImage.ProductId,
            ImageUrl = productImage.ImageUrl,
            ThumbnailUrl = fileService.GetThumbnailUrl(productImage.ImageUrl),
            IsPrimary = productImage.IsPrimary,
            DisplayOrder = productImage.DisplayOrder,
            AltText = productImage.AltText,
            CreatedAt = productImage.CreatedAt
        };
    }

    public async Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId)
    {
        var images = await _unitOfWork.ProductImages.GetByProductIdAsync(productId);
        var fileService = new AdvancedFileStorageService();
        return images.Select(img => new ProductImageDto
        {
            ImageId = img.ImageId,
            ProductId = img.ProductId,
            ImageUrl = img.ImageUrl,
            ThumbnailUrl = fileService.GetThumbnailUrl(img.ImageUrl),
            IsPrimary = img.IsPrimary,
            DisplayOrder = img.DisplayOrder,
            AltText = img.AltText,
            CreatedAt = img.CreatedAt
        });
    }

    public async Task<bool> DeleteImageAsync(int productId, int imageId)
    {
        var image = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
        if (image == null || image.ProductId != productId)
            return false;

        // Delete physical file and thumbnail
        var fileService = new AdvancedFileStorageService();
        await fileService.DeleteFileAsync(image.ImageUrl);

        // If deleting primary image, promote next image
        if (image.IsPrimary)
        {
            var otherImages = (await _unitOfWork.ProductImages.GetByProductIdAsync(productId))
                .Where(i => i.ImageId != imageId)
                .OrderBy(i => i.DisplayOrder)
                .ToList();

            if (otherImages.Any())
            {
                otherImages.First().IsPrimary = true;
                _unitOfWork.ProductImages.Update(otherImages.First());
            }
        }

        _unitOfWork.ProductImages.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
    {
        await _unitOfWork.ProductImages.SetPrimaryImageAsync(productId, imageId);
        return true;
    }

    public async Task<bool> ReorderImagesAsync(int productId, ReorderImagesRequest request)
    {
        foreach (var item in request.Images)
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(item.ImageId);
            if (image != null && image.ProductId == productId)
            {
                image.DisplayOrder = item.DisplayOrder;
                _unitOfWork.ProductImages.Update(image);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductImageDto>> BulkUploadImagesAsync(int productId, IEnumerable<(Stream stream, string fileName)> files)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found");

        var fileService = new AdvancedFileStorageService();
        var uploadedImages = new List<ProductImageDto>();
        var existingImages = await _unitOfWork.ProductImages.GetByProductIdAsync(productId);
        var currentOrder = existingImages.Count();
        var hasPrimary = existingImages.Any(i => i.IsPrimary);

        foreach (var (stream, fileName) in files)
        {
            try
            {
                // Save file with WebP conversion and thumbnail
                var imageUrl = await fileService.SaveFileAsync(stream, fileName, $"products/{productId}");

                // Create ProductImage entity
                var productImage = new Domain.Entities.ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    IsPrimary = !hasPrimary && uploadedImages.Count == 0, // First image becomes primary if none exists
                    DisplayOrder = currentOrder++,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProductImages.AddAsync(productImage);
                await _unitOfWork.SaveChangesAsync();

                uploadedImages.Add(new ProductImageDto
                {
                    ImageId = productImage.ImageId,
                    ProductId = productImage.ProductId,
                    ImageUrl = productImage.ImageUrl,
                    ThumbnailUrl = fileService.GetThumbnailUrl(productImage.ImageUrl),
                    IsPrimary = productImage.IsPrimary,
                    DisplayOrder = productImage.DisplayOrder,
                    CreatedAt = productImage.CreatedAt
                });
            }
            catch
            {
                // Continue with next file if one fails
                continue;
            }
        }

        return uploadedImages;
    }
}

public class AdminOrderService : IAdminOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminOrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<AdminOrderDto>> GetOrdersAsync(OrderFilterRequest request)
    {
        Console.WriteLine($"--> GetOrdersAsync Called. Page: {request.Page}, Status: {request.Status}, StartDate: {request.StartDate}");
        
        var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync();
        Console.WriteLine($"--> Orders fetched from Repo: {orders.Count()}");

        // Apply filters
        var filtered = orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            Console.WriteLine($"--> Filtering by Status: {request.Status}");
            filtered = filtered.Where(o => o.OrderStatus.ToString() == request.Status);
        }
        
        if (request.StartDate.HasValue)
        {
            Console.WriteLine($"--> Filtering by StartDate: {request.StartDate}");
            filtered = filtered.Where(o => o.CreatedAt >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
            filtered = filtered.Where(o => o.CreatedAt <= request.EndDate.Value);
        
        if (request.MinAmount.HasValue)
            filtered = filtered.Where(o => o.TotalAmount >= request.MinAmount.Value);
        
        if (request.MaxAmount.HasValue)
            filtered = filtered.Where(o => o.TotalAmount <= request.MaxAmount.Value);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            Console.WriteLine($"--> Filtering by SearchTerm: {term}");
            filtered = filtered.Where(o => 
                (o.OrderNumber != null && o.OrderNumber.ToLower().Contains(term)) ||
                (o.Customer != null && (
                    (o.Customer.FirstName != null && o.Customer.FirstName.ToLower().Contains(term)) ||
                    (o.Customer.LastName != null && o.Customer.LastName.ToLower().Contains(term)) ||
                    (o.Customer.Email != null && o.Customer.Email.ToLower().Contains(term))
                ))
            );
        }

        var totalCount = filtered.Count();
        Console.WriteLine($"--> Total Count after filter: {totalCount}");
        
        var orderDtos = filtered
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new AdminOrderDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "Unknown Customer",
                CustomerEmail = o.Customer != null ? o.Customer.Email : "No Email",
                CompanyName = (o.Customer != null && o.Customer.Company != null) ? o.Customer.Company.CompanyName : null,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus.ToString(),
                PaymentStatus = "Completed",
                CreatedAt = o.CreatedAt
            })
            .ToList();
            
        Console.WriteLine($"--> DTOs created: {orderDtos.Count}");

        return new PagedResponse<AdminOrderDto>
        {
            Data = orderDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AdminOrderDetailDto?> GetOrderDetailAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            return null;

        return new AdminOrderDetailDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Customer = new CustomerSummary
            {
                CustomerId = order.CustomerId,
                FullName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "Bilinmeyen Müşteri",
                Email = order.Customer?.Email ?? "",
                Phone = order.Customer?.Phone ?? ""
            },
            Company = (order.Customer?.Company != null) ? new CompanySummary
            {
                CompanyId = order.Customer.Company.CompanyId,
                CompanyName = order.Customer.Company.CompanyName,
                TaxNumber = order.Customer.Company.TaxNumber
            } : null,
            Items = order.OrderItems.Select(i => new OrderItemSummary
            {
                OrderItemId = i.OrderItemId,
                ProductName = i.ProductVariant?.Product?.ProductName ?? "Silinmiş Ürün",
                VariantName = i.ProductVariant?.VariantName ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            SubTotal = order.SubtotalAmount,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            OrderStatus = order.OrderStatus.ToString(),
            PaymentStatus = "Completed", // TODO: Fetch real payment status if available
            ShippingAddress = new AddressSummary
            {
                AddressLine = order.ShippingAddress?.AddressLine1 ?? "",
                City = order.ShippingAddress?.City ?? "",
                District = order.ShippingAddress?.District ?? "",
                PostalCode = order.ShippingAddress?.PostalCode ?? ""
            },
            BillingAddress = new AddressSummary
            {
                AddressLine = order.BillingAddress?.AddressLine1 ?? "",
                City = order.BillingAddress?.City ?? "",
                District = order.BillingAddress?.District ?? "",
                PostalCode = order.BillingAddress?.PostalCode ?? ""
            },
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<OrderStatisticsDto> GetStatisticsAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();

        return new OrderStatisticsDto
        {
            TotalOrders = orders.Count(),
            PendingOrders = orders.Count(o => o.OrderStatus == Domain.Enums.OrderStatus.Pending),
            ProcessingOrders = orders.Count(o => o.OrderStatus == Domain.Enums.OrderStatus.Processing),
            CompletedOrders = orders.Count(o => o.OrderStatus == Domain.Enums.OrderStatus.Delivered),
            TotalRevenue = orders.Where(o => o.OrderStatus == Domain.Enums.OrderStatus.Delivered).Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0
        };
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        if (Enum.TryParse<Domain.Enums.OrderStatus>(newStatus, out var status))
        {
            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        return false;
    }

    // Order Timeline
    public async Task<OrderTimelineDto?> GetOrderTimelineAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return null;

        var timeline = new OrderTimelineDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Events = new List<OrderTimelineEvent>
            {
                new OrderTimelineEvent
                {
                    EventType = "Created",
                    Description = "Order created",
                    Timestamp = order.CreatedAt,
                    PerformedBy = "System"
                }
            }
        };

        if (order.ApprovedAt.HasValue)
        {
            timeline.Events.Add(new OrderTimelineEvent
            {
                EventType = "Approved",
                Description = "Order approved",
                Timestamp = order.ApprovedAt.Value,
                PerformedBy = order.ApprovedBy ?? "Admin"
            });
        }

        if (order.ProcessedDate.HasValue)
        {
            timeline.Events.Add(new OrderTimelineEvent
            {
                EventType = "Processing",
                Description = "Order processing started",
                Timestamp = order.ProcessedDate.Value,
                PerformedBy = "System"
            });
        }

        if (order.ShippedDate.HasValue)
        {
            timeline.Events.Add(new OrderTimelineEvent
            {
                EventType = "Shipped",
                Description = "Order shipped",
                Timestamp = order.ShippedDate.Value,
                PerformedBy = "System"
            });
        }

        if (order.DeliveredDate.HasValue)
        {
            timeline.Events.Add(new OrderTimelineEvent
            {
                EventType = "Delivered",
                Description = "Order delivered",
                Timestamp = order.DeliveredDate.Value,
                PerformedBy = "System"
            });
        }

        if (order.CancelledDate.HasValue)
        {
            timeline.Events.Add(new OrderTimelineEvent
            {
                EventType = "Cancelled",
                Description = "Order cancelled",
                Timestamp = order.CancelledDate.Value,
                PerformedBy = "System"
            });
        }

        return timeline;
    }

    // Order Refund
    public async Task<bool> ProcessRefundAsync(int orderId, RefundRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        // Business rules
        if (order.OrderStatus != Domain.Enums.OrderStatus.Delivered)
            return false; // Can only refund delivered orders

        if (request.RefundAmount > order.TotalAmount)
            return false; // Cannot refund more than order total

        // Update order status
        order.OrderStatus = Domain.Enums.OrderStatus.Cancelled;
        order.CancelledDate = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId);
            if (variant != null)
            {
                variant.StockQuantity += item.Quantity;
                variant.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ProductVariants.Update(variant);
            }
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SalesChartDto> GetSalesChartDataAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var orders = await _unitOfWork.Orders.GetAllAsync();
        
        // Filter orders for the period and only delivered/completed ones (or all valid ones based on requirements)
        // Adjust status filter as needed. Assuming we want to show sales for completed orders.
        // Or show all except cancelled? Let's show all valid orders (excluding cancelled) for sales data.
        var periodOrders = orders
            .Where(o => o.CreatedAt >= startDate && o.OrderStatus != Domain.Enums.OrderStatus.Cancelled)
            .ToList();

        var dataPoints = new List<SalesChartPointDto>();
        
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dailyOrders = periodOrders.Where(o => o.CreatedAt.Date == date).ToList();
            
            dataPoints.Add(new SalesChartPointDto
            {
                Date = date.ToString("dd MMM", new System.Globalization.CultureInfo("tr-TR")),
                TotalRevenue = dailyOrders.Sum(o => o.TotalAmount),
                OrderCount = dailyOrders.Count
            });
        }

        return new SalesChartDto
        {
            Data = dataPoints,
            TotalRevenueInPeriod = periodOrders.Sum(o => o.TotalAmount),
            TotalOrdersInPeriod = periodOrders.Count
        };
    }

    public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int count = 5, DateTime? startDate = null)
    {
        var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync(); // Ensure we have details for order items
        
        // Flatten all order items from valid orders
        var query = orders.Where(o => o.OrderStatus != Domain.Enums.OrderStatus.Cancelled);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        var allItems = query
            .SelectMany(o => o.OrderItems)
            .ToList();

        var topProducts = allItems
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new
            {
                ProductVariantId = g.Key,
                ProductVariantName = g.First().ProductVariant?.VariantName ?? "Bilinmeyen Ürün",
                ProductName = g.First().ProductVariant?.Product?.ProductName ?? "Bilinmeyen Ürün",
                TotalQuantity = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(count)
            .Select(x => new TopSellingProductDto
            {
                ProductName = $"{x.ProductName} ({x.ProductVariantName})",
                TotalQuantity = x.TotalQuantity
            })
            .ToList();

        return topProducts;
    }

    // Order Approval (B2B)
    public async Task<bool> ApproveOrderAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        order.ApprovalStatus = "Approved";
        order.ApprovedAt = DateTime.UtcNow;
        order.ApprovedBy = "Admin"; // Should be actual admin user ID
        order.OrderStatus = Domain.Enums.OrderStatus.Processing;
        order.ProcessedDate = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectOrderAsync(int orderId, string reason)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        order.ApprovalStatus = $"Rejected: {reason}";
        order.OrderStatus = Domain.Enums.OrderStatus.Cancelled;
        order.CancelledDate = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId);
            if (variant != null)
            {
                variant.StockQuantity += item.Quantity;
                variant.ReservedQuantity -= item.Quantity;
                variant.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ProductVariants.Update(variant);
            }
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Dashboard
    public async Task<AdminDashboardDto> GetDashboardDataAsync(DateTime? startDate = null)
    {
        // If startDate is not provided, default to DateTime.MinValue (All Time)
        // User asked for "Today / Week / Month" filters, but also "Total" when no filter is selected.
        
        var filterDate = startDate ?? DateTime.MinValue;

        var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync(); 
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var products = await _unitOfWork.Products.GetAllAsync();
        var variants = await _unitOfWork.ProductVariants.GetAllAsync();

        var periodOrders = orders.Where(o => o.CreatedAt >= filterDate).ToList();

        var summary = new DashboardSummary
        {
            TodayOrders = periodOrders.Count(),
            TodayRevenue = periodOrders.Sum(o => o.TotalAmount),
            TotalCustomers = customers.Count(),
            TotalProducts = products.Count(),
            LowStockProducts = variants.Count(v => v.StockQuantity <= 30), // Warning threshold 30
            PendingOrders = orders.Count(o => o.OrderStatus == Domain.Enums.OrderStatus.Pending)
        };

        var recentOrders = orders
            .Where(o => o.CreatedAt >= filterDate)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new RecentOrderSummary
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "Unknown",
                TotalAmount = o.TotalAmount,
                Status = o.OrderStatus.ToString(),
                CreatedAt = o.CreatedAt
            })
            .ToList();

        return new AdminDashboardDto
        {
            Summary = summary,
            RecentOrders = recentOrders
        };
    }

    public async Task<bool> SeedHistoricalOrdersAsync()
    {
        var customer = (await _unitOfWork.Customers.GetAllAsync()).FirstOrDefault();
        if (customer == null) throw new Exception("No customer found to seed orders.");
        
        var variants = (await _unitOfWork.ProductVariants.GetAllAsync()).Take(5).ToList();
        if (!variants.Any()) throw new Exception("No variants found to seed orders.");

        var addresses = await _unitOfWork.Addresses.GetAllAsync(); // Just grab any addresses or customer's
        var shippingAddress = addresses.FirstOrDefault(a => a.AddressType == "Shipping") 
                              ?? new Domain.Entities.Address { AddressLine1 = "Test St", City = "Test", Country = "Test", PostalCode = "12345", AddressType = "Shipping", CustomerId = customer.CustomerId, AddressTitle = "Test Shipping" };
        var billingAddress = addresses.FirstOrDefault(a => a.AddressType == "Billing")
                             ?? new Domain.Entities.Address { AddressLine1 = "Test St", City = "Test", Country = "Test", PostalCode = "12345", AddressType = "Billing", CustomerId = customer.CustomerId, AddressTitle = "Test Billing" };

        if (shippingAddress.AddressId == 0) await _unitOfWork.Addresses.AddAsync(shippingAddress);
        if (billingAddress.AddressId == 0) await _unitOfWork.Addresses.AddAsync(billingAddress);
        await _unitOfWork.SaveChangesAsync();

        var random = new Random();
        var dates = new[]
        {
            DateTime.UtcNow, // Today
            DateTime.UtcNow.AddDays(-7), // 1 Week Ago
            DateTime.UtcNow.AddDays(-14), // 2 Weeks Ago
            DateTime.UtcNow.AddDays(-21) // 3 Weeks Ago
        };

        foreach (var date in dates)
        {
            // Create 2 orders per date
            for (int i = 0; i < 2; i++)
            {
                var variant = variants[random.Next(variants.Count)];
                var quantity = random.Next(1, 3);
                var price = variant.SalePrice ?? variant.BasePrice;
                
                var order = new Domain.Entities.Order
                {
                    OrderNumber = $"SEED-{date:yyyyMMdd}-{i}-{random.Next(1000)}",
                    CustomerId = customer.CustomerId,
                    OrderDate = date,
                    OrderStatus = Domain.Enums.OrderStatus.Delivered,
                    TotalAmount = price * quantity,
                    ShippingAddressId = shippingAddress.AddressId,
                    BillingAddressId = billingAddress.AddressId,
                    CreatedAt = date,
                    UpdatedAt = date,
                    OrderItems = new List<Domain.Entities.OrderItem>
                    {
                        new Domain.Entities.OrderItem
                        {
                            ProductVariantId = variant.ProductVariantId,
                            Quantity = quantity,
                            UnitPrice = price
                        }
                    }
                };
                await _unitOfWork.Orders.AddAsync(order);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
