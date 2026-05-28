using ClaimsModule.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Storage;

public class LocalFileSystemStorageService(IConfiguration configuration, ILogger<LocalFileSystemStorageService> logger) : IStorageService
{
    private readonly string _basePath = configuration["LocalStorage:BasePath"] ?? "/uploads";

    public async Task<string> UploadAsync(Stream content, string blobPath, string contentType, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);

        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);
        logger.LogInformation("Stored file at {Path}", fullPath);
        return blobPath;
    }

    public Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        // Return a local API download URL — API layer will serve the file
        var url = $"/api/storage/download?path={Uri.EscapeDataString(blobPath)}";
        return Task.FromResult(url);
    }

    public Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
