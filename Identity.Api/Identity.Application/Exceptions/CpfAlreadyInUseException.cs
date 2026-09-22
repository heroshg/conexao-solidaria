namespace Identity.Application.Exceptions;

public sealed class CpfAlreadyInUseException(string cpf)
    : Exception($"Cpf '{cpf}' is already registered.");
