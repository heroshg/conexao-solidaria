using Identity.Application.Exceptions;

namespace Identity.Api.ErrorHandling;

public sealed class CpfAlreadyInUseExceptionHandler : ExceptionHandlerTemplate<CpfAlreadyInUseException>
{
    protected override int GetStatusCode(CpfAlreadyInUseException exception) => StatusCodes.Status409Conflict;
}
