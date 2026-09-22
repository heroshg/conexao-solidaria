using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Identity.Application.Exceptions;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Domain.ValueObjects;

namespace Identity.Application.UseCases;

public sealed class RegisterDonorUseCase(
    IUserAccountRepository repository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
{
    public async Task<RegisterDonorResponse> ExecuteAsync(
        RegisterDonorRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(request.Email);
        var cpf = Cpf.Create(request.Cpf);

        if (await repository.ExistsByEmailAsync(email, cancellationToken))
            throw new EmailAlreadyInUseException(email.Value);

        if (await repository.ExistsByCpfAsync(cpf, cancellationToken))
            throw new CpfAlreadyInUseException(cpf.Value);

        var password = Password.Create(request.Password);
        var passwordHash = passwordHasher.Hash(password.Value);

        var donor = Donor.Register(request.FullName, email, cpf, passwordHash);
        await repository.AddAsync(donor, cancellationToken);

        // The Exists* checks above can still race with a concurrent request for the same
        // email/CPF — the DB unique constraint is the real guard; IdentityUnitOfWork classifies
        // that violation as a conflict here instead of a raw 500. On conflict, re-check which
        // field actually collided instead of assuming it was the email.
        var result = await unitOfWork.CommitAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            if (await repository.ExistsByCpfAsync(cpf, cancellationToken))
                throw new CpfAlreadyInUseException(cpf.Value);

            throw new EmailAlreadyInUseException(email.Value);
        }

        return new RegisterDonorResponse(donor.Id, donor.FullName, donor.Email.Value);
    }
}
