using MediatR;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Queries;

public record GetCustomerByKeycloakSubQuery(string KeycloakSub) : IRequest<Customer?>;
