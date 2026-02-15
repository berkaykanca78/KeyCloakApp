using Ordering.API.Application.DTOs;
using Ordering.API.Application.Ports;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Services;

/// <summary>
/// Primary adapter: Müşteri port implementasyonu. Outbound port (ICustomerRepository) üzerinden kalıcılık.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<(Customer? Customer, bool AlreadyExisted)> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _customerRepository.GetByKeycloakSubAsync(request.KeycloakSub, cancellationToken);
            if (existing != null)
                return (existing, true);

            var customer = Customer.Create(
                request.KeycloakSub,
                request.FirstName,
                request.LastName,
                request.Address,
                request.CityId,
                request.DistrictId,
                request.CardLast4);
            _customerRepository.Add(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);
            return (customer, false);
        }
        catch (ArgumentException)
        {
            return (null, false);
        }
    }

    public Task<Customer?> GetByKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default)
        => _customerRepository.GetByKeycloakSubAsync(keycloakSub, cancellationToken);

    public async Task<(Customer? Customer, bool AlreadyExisted)> CreateMeAsync(string keycloakSub, string firstName, string lastName, string? address = null, int? cityId = null, int? districtId = null, string? cardLast4 = null, CancellationToken cancellationToken = default)
    {
        var existing = await _customerRepository.GetByKeycloakSubAsync(keycloakSub, cancellationToken);
        if (existing != null)
            return (existing, true);

        var customer = Customer.Create(keycloakSub, firstName, lastName, address, cityId, districtId, cardLast4);
        _customerRepository.Add(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        return (customer, false);
    }
}
