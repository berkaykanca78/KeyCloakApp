using MediatR;

namespace Ordering.API.Application.Commands;

public record CreateCustomerMeCommand(
    string KeycloakSub,
    string FirstName,
    string LastName,
    string? Address = null,
    int? CityId = null,
    int? DistrictId = null,
    string? CardLast4 = null) : IRequest<CreateCustomerCommandResult>;
