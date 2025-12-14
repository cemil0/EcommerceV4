using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ECommerce.Infrastructure.Import;

public class CsvParser : IFileParser
{
    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<BulkProductImportRowDto>> ParseAsync(Stream fileStream, string fileName)
    {
        var rows = new List<BulkProductImportRowDto>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<BulkProductImportRowDtoMap>();

        await foreach (var record in csv.GetRecordsAsync<BulkProductImportRowDto>())
        {
            rows.Add(record);
        }

        return rows;
    }
}

public class BulkProductImportRowDtoMap : ClassMap<BulkProductImportRowDto>
{
    public BulkProductImportRowDtoMap()
    {
        Map(m => m.ProductName).Name("ProductName");
        Map(m => m.CategorySlug).Name("CategorySlug");
        Map(m => m.Manufacturer).Name("Manufacturer").Optional();
        Map(m => m.Model).Name("Model").Optional();
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.VariantSKU).Name("VariantSKU");
        Map(m => m.Color).Name("Color").Optional();
        Map(m => m.Size).Name("Size").Optional();
        Map(m => m.RAM).Name("RAM").Optional();
        Map(m => m.Storage).Name("Storage").Optional();
        Map(m => m.BasePrice).Name("BasePrice");
        Map(m => m.CostPrice).Name("CostPrice").Optional();
        Map(m => m.StockQuantity).Name("StockQuantity").Optional().Default(0);
        Map(m => m.ImageUrls).Name("ImageUrls").Optional();
        Map(m => m.PrimaryImage).Name("PrimaryImage").Optional().Default(false)
            .TypeConverter<BooleanConverter>();
        Map(m => m.IsActive).Name("IsActive").Optional().Default(true)
            .TypeConverter<BooleanConverter>();
    }
}

public class BooleanConverter : CsvHelper.TypeConversion.DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim().ToUpperInvariant();
        return text == "TRUE" || text == "1" || text == "YES" || text == "Y";
    }
}
