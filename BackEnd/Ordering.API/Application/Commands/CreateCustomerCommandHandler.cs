using MediatR;
using Ordering.API.Application.DTOs;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerCommandResult>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateCustomerCommandResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;
            var existing = await _repository.GetByKeycloakSubAsync(req.KeycloakSub, cancellationToken);
            if (existing != null)
                return new CreateCustomerCommandResult(existing, true);

            var customer = Customer.Create(
                req.KeycloakSub,
                req.FirstName,
                req.LastName,
                req.Address,
                req.CityId,
                req.DistrictId,
                req.CardLast4);
            _repository.Add(customer);
            await _repository.SaveChangesAsync(cancellationToken);
            return new CreateCustomerCommandResult(customer, false);
        }
        catch (ArgumentException)
        {
            return new CreateCustomerCommandResult(null, false);
        }
    }
}
