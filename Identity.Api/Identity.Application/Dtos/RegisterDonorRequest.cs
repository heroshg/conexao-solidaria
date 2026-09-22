namespace Identity.Application.Dtos;

public sealed record RegisterDonorRequest(string FullName, string Email, string Cpf, string Password);

public sealed record RegisterDonorResponse(Guid Id, string FullName, string Email);
