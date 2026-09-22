using Identity.Application.Exceptions;

namespace Identity.Api.ErrorHandling;

public sealed class EmailAlreadyInUseExceptionHandler : ExceptionHandlerTemplate<EmailAlreadyInUseException>
{
    protected override int GetStatusCode(EmailAlreadyInUseException exception) => StatusCodes.Status409Conflict;
}
