using System.Security.Claims;
using Campaigns.Application.Dtos;
using Campaigns.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campaigns.Api.Controllers;

[ApiController]
[Route("donations")]
[Authorize(Roles = "Donor")]
public sealed class DonationsController(CreateDonationUseCase createDonationUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateDonationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateDonationResponse>> Create(
        CreateDonationRequest request, CancellationToken cancellationToken)
    {
        // "sub" claim set by Identity.Api's JwtTokenGenerator is remapped by the JWT bearer
        // handler's default inbound claim mapping to ClaimTypes.NameIdentifier — see CLAUDE.md
        // (Campaigns.Api only validates the token, it never calls Identity.Api at runtime).
        var donorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await createDonationUseCase.ExecuteAsync(request, donorId, cancellationToken);
        return Accepted(response);
    }
}
