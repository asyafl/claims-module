using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ClaimsModule.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Storage;

public class AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger) : IStorageService
{
    private readonly string _connectionString = configuration["AzureBlob:ConnectionString"]!;
    private readonly string _container = configuration["AzureBlob:Container"] ?? "claim-documents";

    public async Task<string> UploadAsync(Stream content, string blobPath, string contentType, CancellationToken cancellationToken = default)
    {
        var client = new BlobServiceClient(_connectionString);
        var container = client.GetBlobContainerClient(_container);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobPath);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        logger.LogInformation("Uploaded blob {Path}", blobPath);
        return blobPath;
    }

    public Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var client = new BlobServiceClient(_connectionString);
        var container = client.GetBlobContainerClient(_container);
        var blob = container.GetBlobClient(blobPath);

        var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ttl));
        return Task.FromResult(sasUri.ToString());
    }

    public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var client = new BlobServiceClient(_connectionString);
        var container = client.GetBlobContainerClient(_container);
        var blob = container.GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
