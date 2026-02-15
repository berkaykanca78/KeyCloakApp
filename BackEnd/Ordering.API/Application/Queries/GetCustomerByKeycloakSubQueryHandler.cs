using MediatR;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Queries;

public class GetCustomerByKeycloakSubQueryHandler : IRequestHandler<GetCustomerByKeycloakSubQuery, Customer?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByKeycloakSubQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> Handle(GetCustomerByKeycloakSubQuery request, CancellationToken cancellationToken)
        => await _repository.GetByKeycloakSubAsync(request.KeycloakSub, cancellationToken);
}
