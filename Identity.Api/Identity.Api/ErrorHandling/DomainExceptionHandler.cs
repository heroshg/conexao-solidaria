using Identity.Domain.Exceptions;

namespace Identity.Api.ErrorHandling;

public sealed class DomainExceptionHandler : ExceptionHandlerTemplate<DomainException>
{
    protected override int GetStatusCode(DomainException exception) => StatusCodes.Status400BadRequest;
}
