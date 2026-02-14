namespace Shared.Api;

/// <summary>
/// Genel API yanıt modeli: Data, IsSuccess, Message (ve isteğe bağlı hata listesi).
/// </summary>
public class ResultDto<T>
{
    public T? Data { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string>? Errors { get; init; }

    public static ResultDto<T> Success(T? data, string message = "İşlem başarılı.")
        => new() { Data = data, IsSuccess = true, Message = message };

    public static ResultDto<T> Failure(string message, IReadOnlyList<string>? errors = null)
        => new() { Data = default, IsSuccess = false, Message = message, Errors = errors };
}
