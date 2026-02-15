namespace InventoryApi.Application.Contracts;

/// <summary>
/// S3 uyumlu (MinIO) object storage: ürün resmi yükleme ve indirme URL'si.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Dosyayı S3'e yükler. Key olarak bucket içindeki yol döner (örn: products/1/abc.jpg).
    /// </summary>
    Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen key için geçici indirme URL'si (presigned) döner. Süre saniye cinsinden.
    /// </summary>
    Task<string> GetDownloadUrlAsync(string key, int expirySeconds = 3600, CancellationToken cancellationToken = default);

    /// <summary>
    /// Key'e ait dosyayı stream olarak döner (proxy download için).
    /// </summary>
    Task<Stream?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Key'e ait dosyayı stream ve Content-Type ile döner (proxy için).
    /// </summary>
    Task<(Stream? Stream, string? ContentType)> GetWithContentTypeAsync(string key, CancellationToken cancellationToken = default);
}
