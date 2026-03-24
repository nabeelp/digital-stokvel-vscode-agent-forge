using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayoutsController : ControllerBase
{
    private readonly IPayoutService _payoutService;
    private readonly ILogger<PayoutsController> _logger;

    public PayoutsController(IPayoutService payoutService, ILogger<PayoutsController> logger)
    {
        _payoutService = payoutService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate payout (PE-02, PE-05)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayout([FromBody] InitiatePayoutRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.InitiatePayoutAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPayout), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to initiate payout", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Approve payout (PE-03)
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApprovePayout(Guid id, [FromBody] ApprovePayoutRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.ApprovePayoutAsync(id, request, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to approve payout", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Reject payout
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectPayout(Guid id, [FromBody] RejectPayoutRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.RejectPayoutAsync(id, request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to reject payout", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get payout details (PE-09)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayout(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.GetPayoutDetailsAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Payout not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get pending approvals for treasurer
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<PayoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.GetPendingApprovalsAsync(userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve pending approvals", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get group payout history
    /// </summary>
    [HttpGet("groups/{groupId}/history")]
    [ProducesResponseType(typeof(List<PayoutHistoryItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupPayoutHistory(Guid groupId)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _payoutService.GetGroupPayoutHistoryAsync(groupId, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve payout history", Detail = result.ErrorMessage });
    }
}
