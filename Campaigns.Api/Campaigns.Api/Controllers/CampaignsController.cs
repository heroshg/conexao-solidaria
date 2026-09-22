using Campaigns.Application.Dtos;
using Campaigns.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campaigns.Api.Controllers;

[ApiController]
[Route("campaigns")]
public sealed class CampaignsController(
    CreateCampaignUseCase createCampaignUseCase,
    UpdateCampaignUseCase updateCampaignUseCase,
    GetCampaignByIdUseCase getCampaignByIdUseCase,
    GetPublicCampaignsUseCase getPublicCampaignsUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "NgoManager")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CampaignResponse>> Create(
        CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        var response = await createCampaignUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "NgoManager")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CampaignResponse>> Update(
        Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        var response = await updateCampaignUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(response);
    }

    // Authenticated (any role), not anonymous: the public contract (GET /campaigns/public)
    // deliberately limits anonymous callers to Title/FinancialGoal/AmountRaised on Active
    // campaigns only — this endpoint returns full details (incl. Description, dates, Status of
    // any campaign) so it stays behind login, same trust boundary as everything else a logged
    // -in Donor can see before deciding to donate.
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getCampaignByIdUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PublicCampaignResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicCampaignResponse>>> GetPublic(CancellationToken cancellationToken)
    {
        var response = await getPublicCampaignsUseCase.ExecuteAsync(cancellationToken);
        return Ok(response);
    }
}
