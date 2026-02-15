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
}
