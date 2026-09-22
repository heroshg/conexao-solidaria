using Campaigns.Domain.Exceptions;

namespace Campaigns.Api.ErrorHandling;

public sealed class DomainExceptionHandler : ExceptionHandlerTemplate<DomainException>
{
    protected override int GetStatusCode(DomainException exception) => StatusCodes.Status400BadRequest;
}
