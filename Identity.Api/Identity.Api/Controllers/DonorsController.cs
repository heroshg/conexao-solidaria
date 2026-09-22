using Identity.Application.Dtos;
using Identity.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("donors")]
public sealed class DonorsController(RegisterDonorUseCase registerDonorUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RegisterDonorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterDonorResponse>> Register(
        RegisterDonorRequest request, CancellationToken cancellationToken)
    {
        var response = await registerDonorUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }
}
