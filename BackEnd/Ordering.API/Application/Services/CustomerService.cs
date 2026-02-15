using System.Security.Claims;
using MediatR;
using Ordering.API.Application.Commands;
using Ordering.API.Application.DTOs;
using Ordering.API.Application.Ports;
using Ordering.API.Application.Queries;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Services;

/// <summary>
/// Primary adapter: Müşteri port implementasyonu. MediatR (CQRS) üzerinden handler'lara yönlendirir; handler'lar repository kullanır.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IMediator _mediator;

    public CustomerService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<(Customer? Customer, bool AlreadyExisted)> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(request), cancellationToken);
        return (result.Customer, result.AlreadyExisted);
    }

    public Task<Customer?> GetByKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default)
        => _mediator.Send(new GetCustomerByKeycloakSubQuery(keycloakSub), cancellationToken);

    public async Task<(Customer? Customer, bool AlreadyExisted)> CreateMeAsync(string keycloakSub, string firstName, string lastName, string? address = null, int? cityId = null, int? districtId = null, string? cardLast4 = null, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CreateCustomerMeCommand(keycloakSub, firstName, lastName, address, cityId, districtId, cardLast4), cancellationToken);
        return (result.Customer, result.AlreadyExisted);
    }

    public async Task<GetMeResult> GetMeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
            return GetMeResult.FromUnauthorized();
        var customer = await _mediator.Send(new GetCustomerByKeycloakSubQuery(sub), cancellationToken);
        if (customer == null)
            return GetMeResult.NotFound("Müşteri kaydı bulunamadı. Önce kayıt olun.");
        return GetMeResult.Success(customer);
    }

    public async Task<CreateMeResult> CreateMeAsync(ClaimsPrincipal user, CreateCustomerMeRequest? request, CancellationToken cancellationToken = default)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
            return CreateMeResult.FromUnauthorized();

        var firstName = request?.FirstName?.Trim();
        var lastName = request?.LastName?.Trim();
        if (string.IsNullOrEmpty(firstName))
            firstName = user.FindFirst("given_name")?.Value ?? user.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
        if (string.IsNullOrEmpty(lastName))
            lastName = user.FindFirst("family_name")?.Value ?? user.FindFirst(ClaimTypes.Surname)?.Value ?? "";
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        {
            var name = user.FindFirst("name")?.Value ?? user.Identity?.Name ?? "";
            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = parts.Length > 0 ? parts[0] : "Kullanıcı";
            lastName = parts.Length > 1 ? parts[1] : "";
        }
        if (string.IsNullOrEmpty(firstName))
            firstName = "Kullanıcı";

        var (customer, alreadyExisted) = await CreateMeAsync(sub, firstName, lastName, request?.Address, request?.CityId, request?.DistrictId, request?.CardLast4, cancellationToken);
        if (customer == null)
            return CreateMeResult.ServerError("Müşteri kaydı oluşturulamadı.");
        return CreateMeResult.Success(customer, alreadyExisted);
    }
}
