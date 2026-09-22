using Identity.Application.Exceptions;

namespace Identity.Api.ErrorHandling;

public sealed class InvalidCredentialsExceptionHandler : ExceptionHandlerTemplate<InvalidCredentialsException>
{
    protected override int GetStatusCode(InvalidCredentialsException exception) => StatusCodes.Status401Unauthorized;
}
