using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GovernanceController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<GovernanceController> _logger;

    public GovernanceController(IGovernanceService governanceService, ILogger<GovernanceController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Create group constitution (GG-01)
    /// </summary>
    [HttpPost("constitutions")]
    [ProducesResponseType(typeof(ConstitutionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConstitution([FromBody] CreateConstitutionRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.CreateConstitutionAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetConstitution), new { groupId = result.Data!.GroupId }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to create constitution", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get constitution by group ID
    /// </summary>
    [HttpGet("constitutions/{groupId}")]
    [ProducesResponseType(typeof(ConstitutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConstitution(Guid groupId)
    {
        var result = await _governanceService.GetConstitutionAsync(groupId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Constitution not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Create vote proposal (GG-02, GG-03)
    /// </summary>
    [HttpPost("votes")]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVote([FromBody] CreateVoteRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.CreateVoteAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetVote), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to create vote", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Cast vote (GG-04)
    /// </summary>
    [HttpPost("votes/{id}/cast")]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CastVote(Guid id, [FromBody] CastVoteRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.CastVoteAsync(id, request, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to cast vote", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get vote status and results
    /// </summary>
    [HttpGet("votes/{id}")]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVote(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.GetVoteDetailsAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Vote not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Raise dispute (GG-06)
    /// </summary>
    [HttpPost("disputes")]
    [ProducesResponseType(typeof(DisputeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RaiseDispute([FromBody] RaiseDisputeRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.RaiseDisputeAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetDispute), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to raise dispute", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get dispute details
    /// </summary>
    [HttpGet("disputes/{id}")]
    [ProducesResponseType(typeof(DisputeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDispute(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.GetDisputeDetailsAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Dispute not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Add message to dispute thread (GG-07)
    /// </summary>
    [HttpPost("disputes/{id}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDisputeMessage(Guid id, [FromBody] AddDisputeMessageRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.AddDisputeMessageAsync(id, request, userId);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails { Title = "Failed to add message", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Resolve dispute (GG-07)
    /// </summary>
    [HttpPost("disputes/{id}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _governanceService.ResolveDisputeAsync(id, request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to resolve dispute", Detail = result.ErrorMessage });
    }
}
