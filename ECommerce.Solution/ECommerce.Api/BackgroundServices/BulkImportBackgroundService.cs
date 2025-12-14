using System.Text.Json;
using System.Threading.Channels;
using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.BackgroundServices;

public class BulkImportBackgroundService : BackgroundService
{
    private readonly Channel<Guid> _jobChannel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BulkImportBackgroundService> _logger;

    public BulkImportBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<BulkImportBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Unbounded channel for simplicity, can be bounded for backpressure
        _jobChannel = Channel.CreateUnbounded<Guid>();
    }

    public async Task QueueJobAsync(Guid jobId)
    {
        await _jobChannel.Writer.WriteAsync(jobId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bulk Import Background Service started.");

        await foreach (var jobId in _jobChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(jobId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk import job {JobId}", jobId);
            }
        }
    }

    private async Task ProcessJobAsync(Guid jobId, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        var importService = scope.ServiceProvider.GetRequiredService<IBulkProductImportService>();

        var job = await dbContext.BulkImportJobs.FindAsync(new object[] { jobId }, stoppingToken);
        if (job == null)
        {
            _logger.LogError("Job {JobId} not found", jobId);
            return;
        }

        try
        {
            // Update status to Processing
            job.Status = "Processing";
            job.StartedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(stoppingToken);

            // Get file path (assuming file is saved temporarily)
            var tempFilePath = Path.Combine(Path.GetTempPath(), job.FileName);
            
            if (!File.Exists(tempFilePath))
            {
                throw new FileNotFoundException($"Import file not found: {tempFilePath}");
            }

            using var fileStream = File.OpenRead(tempFilePath);
            var options = new BulkImportOptionsDto { StrictMode = false, DryRun = false }; // Could be stored in job
            
            // Execute import
            var result = await importService.ImportAsync(fileStream, job.FileName, options);

            // Update job with results
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            job.TotalRows = result.TotalRows;
            job.ProcessedRows = result.TotalRows;
            job.SuccessCount = result.SuccessCount;
            job.FailedCount = result.FailedCount;
            job.ResultJson = JsonSerializer.Serialize(result);

            _logger.LogInformation("Job {JobId} completed. Success: {Success}, Failed: {Failed}", 
                jobId, result.SuccessCount, result.FailedCount);
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Job {JobId} failed", jobId);
        }
        finally
        {
            await dbContext.SaveChangesAsync(stoppingToken);
            
            // Clean up temp file
            var tempFilePath = Path.Combine(Path.GetTempPath(), job.FileName);
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file {FilePath}", tempFilePath);
                }
            }
        }
    }
}
