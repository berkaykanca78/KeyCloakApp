using System.Security.Claims;
using Ordering.API.Application.DTOs;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Ports;

/// <summary>
/// Inbound port: Müşteri uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface ICustomerService
{
    /// <summary>Müşteri oluşturur veya mevcut olanı döner. Item2: true = zaten vardı, false = yeni oluşturuldu.</summary>
    Task<(Customer? Customer, bool AlreadyExisted)> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Customer?> GetByKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default);
    /// <summary>Giriş yapan kullanıcı için müşteri kaydı oluşturur. Item2: true = zaten vardı, false = yeni oluşturuldu.</summary>
    Task<(Customer? Customer, bool AlreadyExisted)> CreateMeAsync(string keycloakSub, string firstName, string lastName, string? address = null, int? cityId = null, int? districtId = null, string? cardLast4 = null, CancellationToken cancellationToken = default);

    /// <summary>Token'dan sub alıp müşteri döner; sub yoksa Unauthorized, müşteri yoksa NotFound.</summary>
    Task<GetMeResult> GetMeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
    /// <summary>Token ve isteğe göre ad bilgisi alıp müşteri oluşturur veya mevcut döner.</summary>
    Task<CreateMeResult> CreateMeAsync(ClaimsPrincipal user, CreateCustomerMeRequest? request, CancellationToken cancellationToken = default);
}
