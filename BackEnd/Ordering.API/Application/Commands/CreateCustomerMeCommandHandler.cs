using MediatR;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Commands;

public class CreateCustomerMeCommandHandler : IRequestHandler<CreateCustomerMeCommand, CreateCustomerCommandResult>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerMeCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateCustomerCommandResult> Handle(CreateCustomerMeCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByKeycloakSubAsync(request.KeycloakSub, cancellationToken);
        if (existing != null)
            return new CreateCustomerCommandResult(existing, true);

        var customer = Customer.Create(
            request.KeycloakSub,
            request.FirstName,
            request.LastName,
            request.Address,
            request.CityId,
            request.DistrictId,
            request.CardLast4);
        _repository.Add(customer);
        await _repository.SaveChangesAsync(cancellationToken);
        return new CreateCustomerCommandResult(customer, false);
    }
}
