namespace Identity.Application.Exceptions;

public sealed class EmailAlreadyInUseException(string email)
    : Exception($"Email '{email}' is already registered.");
