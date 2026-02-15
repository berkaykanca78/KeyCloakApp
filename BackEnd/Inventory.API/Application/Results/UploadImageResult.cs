namespace Inventory.API.Application.Results;

/// <summary>Resim yükleme işlemi sonucu.</summary>
public record UploadImageResult(bool Success, string? ImageKey, string? Error, UploadImageErrorKind ErrorKind)
{
    public static UploadImageResult Ok(string imageKey) => new(true, imageKey, null, UploadImageErrorKind.None);
    public static UploadImageResult BadRequest(string error) => new(false, null, error, UploadImageErrorKind.BadRequest);
    public static UploadImageResult NotFound(string error) => new(false, null, error, UploadImageErrorKind.NotFound);
    public static UploadImageResult ServerError(string error) => new(false, null, error, UploadImageErrorKind.ServerError);

    public bool IsNotFound => ErrorKind == UploadImageErrorKind.NotFound;
}

public enum UploadImageErrorKind { None, BadRequest, NotFound, ServerError }
