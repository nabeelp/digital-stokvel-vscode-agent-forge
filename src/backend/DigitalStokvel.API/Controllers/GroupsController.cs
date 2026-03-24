using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IWalletService _walletService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, IWalletService walletService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _walletService = walletService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new stokvel group (GM-01, GM-02, GM-06)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user"; // TODO: Get from claims
        var result = await _groupService.CreateGroupAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetGroup), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to create group", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get group details with balance and members (GM-07)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GroupDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.GetGroupDetailsAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Group not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get current user's groups
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<GroupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.GetMyGroupsAsync(userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve groups", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Update group details (GM-08)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.UpdateGroupAsync(id, request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to update group", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Archive group (GM-09)
    /// </summary>
    [HttpPut("{id}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArchiveGroup(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.ArchiveGroupAsync(id, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to archive group", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Invite member to group (GM-03, GM-04)
    /// </summary>
    [HttpPost("{id}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteMember(Guid id, [FromBody] InviteMemberRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.AddMemberAsync(id, request, userId);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails { Title = "Failed to invite member", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Remove member from group (GM-05)
    /// </summary>
    [HttpDelete("{id}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId, [FromBody] RemoveMemberRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _groupService.RemoveMemberAsync(id, memberId, request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to remove member", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get group ledger (GW-05, GW-08)
    /// </summary>
    [HttpGet("{id}/ledger")]
    [ProducesResponseType(typeof(PagedLedgerResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupLedger(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var pagination = new PaginationRequest(pageNumber, pageSize);
        var result = await _walletService.GetLedgerEntriesAsync(id, pagination, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve ledger", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get group balance (GW-02)
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(typeof(GroupBalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupBalance(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _walletService.GetGroupBalanceAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve balance", Detail = result.ErrorMessage });
    }
}
