using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Commands;

public record CreateCustomerCommandResult(Customer? Customer, bool AlreadyExisted);
