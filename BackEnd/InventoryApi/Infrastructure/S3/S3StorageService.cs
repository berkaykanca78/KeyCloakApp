using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using InventoryApi.Application.Contracts;

namespace InventoryApi.Infrastructure.S3;

/// <summary>
/// MinIO / S3 uyumlu storage: AWSSDK.S3 ile upload, presigned URL ve stream download.
/// </summary>
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public S3StorageService(IAmazonS3 s3, string bucketName)
    {
        _s3 = s3;
        _bucketName = bucketName;
    }

    public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };
        await _s3.PutObjectAsync(request, cancellationToken);
        return key;
    }

    public async Task<string> GetDownloadUrlAsync(string key, int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
        };
        var url = await Task.Run(() => _s3.GetPreSignedURL(request), cancellationToken);
        return url;
    }

    public async Task<Stream?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var (stream, _) = await GetWithContentTypeAsync(key, cancellationToken);
        return stream;
    }

    public async Task<(Stream? Stream, string? ContentType)> GetWithContentTypeAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest { BucketName = _bucketName, Key = key };
            var response = await _s3.GetObjectAsync(request, cancellationToken);
            return (response.ResponseStream, response.Headers?.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return (null, null);
        }
    }
}
