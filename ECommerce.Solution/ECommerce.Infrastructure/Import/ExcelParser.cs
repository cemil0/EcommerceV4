using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using OfficeOpenXml;

namespace ECommerce.Infrastructure.Import;

public class ExcelParser : IFileParser
{
    // Column name aliases (English -> possible alternatives)
    private static readonly Dictionary<string, string[]> ColumnAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ProductName", new[] { "ProductName", "Ürün Adı", "UrunAdi", "Ürün", "Product" } },
        { "CategorySlug", new[] { "CategorySlug", "Kategori", "Category", "KategoriSlug" } },
        { "VariantSKU", new[] { "VariantSKU", "SKU", "StokKodu", "Stok Kodu", "Variant SKU" } },
        { "BasePrice", new[] { "BasePrice", "Fiyat", "Price", "BazFiyat" } },
        { "StockQuantity", new[] { "StockQuantity", "Stok", "Stock", "StokMiktari", "Miktar" } },
        { "Brand", new[] { "Brand", "Marka" } },
        { "Manufacturer", new[] { "Manufacturer", "Üretici", "Uretici" } },
        { "Model", new[] { "Model" } },
        { "Description", new[] { "Description", "Açıklama", "Aciklama", "LongDescription" } },
        { "ShortDescription", new[] { "ShortDescription", "Kısa Açıklama", "KisaAciklama" } },
        { "Color", new[] { "Color", "Renk" } },
        { "Size", new[] { "Size", "Beden", "Boyut" } },
        { "RAM", new[] { "RAM" } },
        { "Storage", new[] { "Storage", "Depolama" } },
        { "CostPrice", new[] { "CostPrice", "Maliyet", "Cost" } },
        { "ImageUrls", new[] { "ImageUrls", "Görseller", "Images", "Görsel" } },
        { "PrimaryImage", new[] { "PrimaryImage", "AnaGörsel" } },
        { "IsFeatured", new[] { "IsFeatured", "ÖneÇıkan", "Featured" } },
        { "IsNewArrival", new[] { "IsNewArrival", "YeniÜrün", "NewArrival" } },
        { "IsActive", new[] { "IsActive", "Aktif", "Active" } }
    };

    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<BulkProductImportRowDto>> ParseAsync(Stream fileStream, string fileName)
    {
        var rows = new List<BulkProductImportRowDto>();

        using var package = new OfficeOpenXml.ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0]; // First sheet
        
        if (worksheet.Dimension == null)
            return rows;

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        // Read header row and map to standard column names
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(header))
                continue;

            // Try to match header to a standard column name
            foreach (var kvp in ColumnAliases)
            {
                foreach (var alias in kvp.Value)
                {
                    if (header.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!columnMap.ContainsKey(kvp.Key))
                            columnMap[kvp.Key] = col;
                        break;
                    }
                }
            }
        }

        // Parse data rows (skip header)
        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var rowDto = new BulkProductImportRowDto
                {
                    ProductName = GetCellValue(worksheet, row, columnMap, "ProductName"),
                    CategorySlug = GetCellValue(worksheet, row, columnMap, "CategorySlug"),
                    Brand = GetCellValue(worksheet, row, columnMap, "Brand"),
                    Manufacturer = GetCellValue(worksheet, row, columnMap, "Manufacturer"),
                    Model = GetCellValue(worksheet, row, columnMap, "Model"),
                    ShortDescription = GetCellValue(worksheet, row, columnMap, "ShortDescription"),
                    Description = GetCellValue(worksheet, row, columnMap, "Description"),
                    VariantSKU = GetCellValue(worksheet, row, columnMap, "VariantSKU"),
                    Color = GetCellValue(worksheet, row, columnMap, "Color"),
                    Size = GetCellValue(worksheet, row, columnMap, "Size"),
                    RAM = GetCellValue(worksheet, row, columnMap, "RAM"),
                    Storage = GetCellValue(worksheet, row, columnMap, "Storage"),
                    BasePrice = GetDecimalValue(worksheet, row, columnMap, "BasePrice"),
                    CostPrice = GetNullableDecimalValue(worksheet, row, columnMap, "CostPrice"),
                    StockQuantity = GetIntValue(worksheet, row, columnMap, "StockQuantity"),
                    ImageUrls = GetCellValue(worksheet, row, columnMap, "ImageUrls"),
                    PrimaryImage = GetBoolValue(worksheet, row, columnMap, "PrimaryImage"),
                    IsFeatured = GetBoolValue(worksheet, row, columnMap, "IsFeatured"),
                    IsNewArrival = GetBoolValue(worksheet, row, columnMap, "IsNewArrival"),
                    IsActive = GetBoolValue(worksheet, row, columnMap, "IsActive", defaultValue: true)
                };

                rows.Add(rowDto);
            }
            catch
            {
                // Skip invalid rows during parsing, will be caught in validation
                continue;
            }
        }

        return await Task.FromResult(rows);
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.ContainsKey(columnName))
            return string.Empty;

        var col = columnMap[columnName];
        var cell = worksheet.Cells[row, col];
        
        // Try multiple ways to get cell value
        var value = cell.Text?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            value = cell.Value?.ToString()?.Trim();
        }
        
        return value ?? string.Empty;
    }

    private decimal GetDecimalValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        var value = GetCellValue(worksheet, row, columnMap, columnName);
        return decimal.TryParse(value, out var result) ? result : 0;
    }

    private decimal? GetNullableDecimalValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        var value = GetCellValue(worksheet, row, columnMap, columnName);
        return decimal.TryParse(value, out var result) ? result : null;
    }

    private int GetIntValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        var value = GetCellValue(worksheet, row, columnMap, columnName);
        return int.TryParse(value, out var result) ? result : 0;
    }

    private bool GetBoolValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName, bool defaultValue = false)
    {
        var value = GetCellValue(worksheet, row, columnMap, columnName);
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("YES", StringComparison.OrdinalIgnoreCase);
    }
}
