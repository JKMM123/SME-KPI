using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SmeKpiDashboard.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID claim is missing. Please log in again.");
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID claim. Please log in again.");
        return userId;
    }
}
