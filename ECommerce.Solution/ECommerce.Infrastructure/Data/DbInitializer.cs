#nullable enable
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ECommerceDbContext context)
    {
        Console.WriteLine("--> STARTED SEEDING...");

        try
        {
            // 1. Seed Categories
            if (!context.Categories.Any())
            {
                Console.WriteLine("--> Seeding Categories...");
                var categories = new List<Category>
                {
                    new Category { CategoryName = "Elektronik", CategorySlug = "elektronik", Description = "Elektronik ürünler", IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryName = "Bilgisayar", CategorySlug = "bilgisayar", Description = "Bilgisayar ve aksesuarları", IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryName = "Telefon", CategorySlug = "telefon", Description = "Akıllı telefonlar", IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
            else
            {
                // Fix: Translate existing English categories to Turkish
                // specific Where clause might miss items, so we fetch all and check in memory
                var allCategories = context.Categories.ToList();
                var hasChanges = false;
                
                var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Electronics", "Elektronik" },
                    { "Computers", "Bilgisayar" },
                    { "Smartphones", "Telefon" },
                    { "Laptops", "Dizüstü Bilgisayar" },
                    { "Tablets", "Tablet" },
                    { "Audio", "Ses Sistemleri" },
                    { "Wearables", "Giyilebilir Teknoloji" },
                    { "Cameras", "Kamera & Fotoğraf" },
                    { "Gaming", "Oyuncu Ekipmanları" },
                    { "Smart Home", "Akıllı Ev Sistemleri" },
                    { "Accessories", "Aksesuarlar" },
                    { "Storage", "Depolama Birimleri" },
                    { "Monitors", "Monitör" },
                    { "Keyboards", "Klavye" },
                    { "Mice", "Mouse" },
                    { "Printers", "Yazıcı" },
                    { "Scanners", "Tarayıcı" },
                    { "Networking", "Ağ Ürünleri" },
                    { "Software", "Yazılım" },
                    { "Cables", "Kablolar" },
                    { "Chargers", "Şarj Aletleri" },
                    { "Cases", "Kılıflar" },
                    { "Headphones", "Kulaklık" },
                    { "Speakers", "Hoparlör" },
                    { "Components", "Bilgisayar Bileşenleri" }
                };

                Console.WriteLine("--> Checking for English categories to translate...");
                foreach (var cat in allCategories)
                {
                    if (translations.ContainsKey(cat.CategoryName))
                    {
                        var turkishName = translations[cat.CategoryName];
                        // Only update if it's different (though key check implies it is, unless already Turkish map exists which it doesn't)
                        if (cat.CategoryName != turkishName)
                        {
                            Console.WriteLine($"--> Translating '{cat.CategoryName}' to '{turkishName}'...");
                            cat.CategoryName = turkishName;
                            // Update slug as well
                            cat.CategorySlug = turkishName.ToLower()
                                .Replace(" ", "-")
                                .Replace("&", "")
                                .Replace("ı", "i")
                                .Replace("ğ", "g")
                                .Replace("ü", "u")
                                .Replace("ş", "s")
                                .Replace("ö", "o")
                                .Replace("ç", "c")
                                .Replace("--", "-"); // prevent double dashes
                            
                            hasChanges = true;
                        }
                    }
                }

                if (hasChanges)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine("--> Categories translated successfully.");
                }
            }

            var catElectronics = context.Categories.FirstOrDefault(c => c.CategorySlug == "elektronik");
            var catComputer = context.Categories.FirstOrDefault(c => c.CategorySlug == "bilgisayar");
            var catPhone = context.Categories.FirstOrDefault(c => c.CategorySlug == "telefon");

            // 2. Seed Products
            if (!context.Products.Any())
            {
                Console.WriteLine("--> Seeding Products...");
                var products = new List<Product>
                {
                    new Product { SKU = "LAPTOP-001", ProductName = "Dell XPS 15", ProductSlug = "dell-xps-15", ShortDescription = "Yüksek performanslı dizüstü bilgisayar", Brand = "Dell", CategoryId = catComputer?.CategoryId ?? 1, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { SKU = "LAPTOP-002", ProductName = "MacBook Pro 14", ProductSlug = "macbook-pro-14", ShortDescription = "Apple'ın güçlü dizüstü bilgisayarı", Brand = "Apple", CategoryId = catComputer?.CategoryId ?? 1, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { SKU = "PHONE-001", ProductName = "iPhone 15 Pro", ProductSlug = "iphone-15-pro", ShortDescription = "Apple'ın en yeni akıllı telefonu", Brand = "Apple", CategoryId = catPhone?.CategoryId ?? 1, IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { SKU = "PHONE-002", ProductName = "Samsung Galaxy S24", ProductSlug = "samsung-galaxy-s24", ShortDescription = "Samsung'un amiral gemisi telefonu", Brand = "Samsung", CategoryId = catPhone?.CategoryId ?? 1, IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // Lookup Products
            var p1 = context.Products.FirstOrDefault(p => p.SKU == "LAPTOP-001");
            var p2 = context.Products.FirstOrDefault(p => p.SKU == "LAPTOP-002");
            var p3 = context.Products.FirstOrDefault(p => p.SKU == "PHONE-001");
            var p4 = context.Products.FirstOrDefault(p => p.SKU == "PHONE-002");

            // 3. Seed Product Variants
            if (!context.ProductVariants.Any() && p1 != null && p2 != null && p3 != null && p4 != null)
            {
                Console.WriteLine("--> Seeding Variants...");
                var variants = new List<ProductVariant>
                {
                    new ProductVariant { ProductId = p1.ProductId, VariantSKU = "LAPTOP-001-16GB-512GB", VariantName = "16GB RAM, 512GB SSD", RAM = "16GB", Storage = "512GB", BasePrice = 45000m, SalePrice = 42000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new ProductVariant { ProductId = p1.ProductId, VariantSKU = "LAPTOP-001-32GB-1TB", VariantName = "32GB RAM, 1TB SSD", RAM = "32GB", Storage = "1TB", BasePrice = 55000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new ProductVariant { ProductId = p2.ProductId, VariantSKU = "LAPTOP-002-16GB-512GB", VariantName = "M3 Pro, 16GB, 512GB", RAM = "16GB", Storage = "512GB", BasePrice = 75000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new ProductVariant { ProductId = p3.ProductId, VariantSKU = "PHONE-001-128GB-BLACK", VariantName = "128GB Siyah Titanyum", Color = "Siyah Titanyum", Storage = "128GB", BasePrice = 52000m, SalePrice = 49000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new ProductVariant { ProductId = p3.ProductId, VariantSKU = "PHONE-001-256GB-BLUE", VariantName = "256GB Mavi Titanyum", Color = "Mavi Titanyum", Storage = "256GB", BasePrice = 58000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new ProductVariant { ProductId = p4.ProductId, VariantSKU = "PHONE-002-256GB-BLACK", VariantName = "256GB Siyah", Color = "Siyah", Storage = "256GB", BasePrice = 38000m, SalePrice = 35000m, Currency = "TRY", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                context.ProductVariants.AddRange(variants);
                await context.SaveChangesAsync();
            }

            var v1 = context.ProductVariants.FirstOrDefault(v => v.VariantSKU == "LAPTOP-001-16GB-512GB");
            var v2 = context.ProductVariants.FirstOrDefault(v => v.VariantSKU == "PHONE-001-128GB-BLACK");

            // 4. Seed Customer
            var customer = context.Customers.FirstOrDefault(c => c.Email == "cemil@example.com");
            if (customer == null)
            {
                Console.WriteLine("--> Seeding Customer...");
                customer = new Customer
                {
                    FirstName = "Cemil",
                    LastName = "Öztürk",
                    Email = "cemil@example.com",
                    Phone = "5551234567",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }

            // 5. Seed Orders
            if (!context.Orders.Any() && customer != null && v1 != null && v2 != null)
            {
                Console.WriteLine("--> Seeding Orders...");
                var orders = new List<Order>
                {
                    new Order
                    {
                        OrderNumber = "ORD-2025-001",
                        CustomerId = customer.CustomerId,
                        OrderDate = DateTime.UtcNow.AddDays(-2),
                        TotalAmount = 42000m,
                        SubtotalAmount = 35000m,
                        TaxAmount = 7000m,
                        ShippingAmount = 0m,
                        OrderStatus = OrderStatus.Processing,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2),
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductVariantId = v1.ProductVariantId,
                                Quantity = 1,
                                UnitPrice = 42000m,
                                CreatedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        }
                    },
                    new Order
                    {
                        OrderNumber = "ORD-2025-002",
                        CustomerId = customer.CustomerId,
                        OrderDate = DateTime.UtcNow.AddDays(-1),
                        TotalAmount = 49000m,
                        SubtotalAmount = 40000m,
                        TaxAmount = 9000m,
                        ShippingAmount = 0m,
                        OrderStatus = OrderStatus.Delivered,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1),
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductVariantId = v2.ProductVariantId,
                                Quantity = 1,
                                UnitPrice = 49000m,
                                CreatedAt = DateTime.UtcNow.AddDays(-1)
                            }
                        }
                    }
                };

                context.Orders.AddRange(orders);
                await context.SaveChangesAsync();
                Console.WriteLine("--> Orders Seeded Successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> SEEDING ERROR: {ex.Message}");
            throw;
        }

        Console.WriteLine("--> SEEDING COMPLETED.");
    }
}
