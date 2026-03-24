using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IContributionService _contributionService;
    private readonly ILogger<MembersController> _logger;

    public MembersController(IMemberService memberService, IContributionService contributionService, ILogger<MembersController> logger)
    {
        _memberService = memberService;
        _contributionService = contributionService;
        _logger = logger;
    }

    /// <summary>
    /// Get current member profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _memberService.GetMemberProfileAsync(userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Profile not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get member by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMember(Guid id)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _memberService.GetMemberByIdAsync(id, userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = "Member not found", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Update member profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateMemberProfileRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _memberService.UpdateMemberProfileAsync(request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to update profile", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Accept group invitation
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _memberService.AcceptInvitationAsync(request, userId);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = "Failed to accept invitation", Detail = result.ErrorMessage });
    }

    /// <summary>
    /// Get my contribution history
    /// </summary>
    [HttpGet("me/contributions")]
    [ProducesResponseType(typeof(List<ContributionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyContributions()
    {
        var userId = User.Identity?.Name ?? "test-user";
        var result = await _contributionService.GetMyContributionsAsync(userId);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ProblemDetails { Title = "Failed to retrieve contributions", Detail = result.ErrorMessage });
    }
}
