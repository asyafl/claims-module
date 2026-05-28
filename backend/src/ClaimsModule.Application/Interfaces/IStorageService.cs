namespace ClaimsModule.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream content, string blobPath, string contentType, CancellationToken cancellationToken = default);
    Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);
}
