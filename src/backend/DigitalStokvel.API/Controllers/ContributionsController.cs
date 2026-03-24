using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContributionsController : ControllerBase
{
    private readonly IContributionService _contributionService;
    private readonly ILogger<ContributionsController> _logger;

    public ContributionsController(IContributionService contributionService, ILogger<ContributionsController> logger)
    {
        _contributionService = contributionService;
        _logger = logger;
    }

    /// <summary>
    /// Submit contribution (CC-01, CC-10)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContributionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContribution([FromBody] CreateContributionRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.ProcessContributionAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetContribution), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to process contribution", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get contribution receipt (CC-06, CC-09)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContribution(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.GetContributionReceiptAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Contribution not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get my contributions
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ContributionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyContributions()
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.GetMyContributionsAsync(userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve contributions", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Setup recurring debit order (CC-02)
    /// </summary>
    [HttpPost("recurring")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetupRecurringContribution([FromBody] SetupRecurringContributionRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.SetupRecurringContributionAsync(request, userId);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails { Title = "Failed to setup recurring contribution", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Cancel recurring debit order
    /// </summary>
    [HttpDelete("recurring/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelRecurringContribution(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.CancelRecurringContributionAsync(id, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to cancel recurring contribution", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Download PDF receipt
    /// </summary>
    [HttpGet("{id}/receipt/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceiptPdf(Guid id)
    {
        // TODO: Implement PDF generation
        return Ok(new { message = "PDF generation not yet implemented" });
    }
}
