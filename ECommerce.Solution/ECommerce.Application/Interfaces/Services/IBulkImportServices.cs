using ECommerce.Application.DTOs.Admin;

namespace ECommerce.Application.Interfaces.Services;

public interface IFileParser
{
    Task<List<BulkProductImportRowDto>> ParseAsync(Stream fileStream, string fileName);
    bool CanParse(string fileName);
}

public interface IBulkProductImportService
{
    Task<BulkImportResultDto> ImportAsync(Stream fileStream, string fileName, BulkImportOptionsDto options);
}
