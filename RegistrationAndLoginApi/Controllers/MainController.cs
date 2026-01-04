using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationAndLoginApi.Auth;
using RegistrationAndLoginApi.DataAccess;
using RegistrationAndLoginApi.DataAccess.Entities;
using RegistrationAndLoginApi.Models;

namespace RegistrationAndLoginApi.Controllers;

[Authorize]
[ApiController]
[Route("/v1/[controller]")]
public class MainController(AppDbContext dbContext, IAuthService authService, ILogger<MainController> logger) : ControllerBase
{
    [HttpGet(nameof(Test))]
    public IActionResult Test()
    {
        logger.LogInformation("Test was executed successfully");
        
        return Ok();
    }

    [HttpPost(nameof(AssignAdmin))]
    public async Task<ActionResult<AuthResponseDto>> AssignAdmin()
    {
        logger.LogTrace("AssignAdmin Start");
        
        Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            logger.LogDebug("AssignAdmin | Rejected - User ID claim was null");
            return Unauthorized();
        }

        Guid userId = Guid.Parse(userIdClaim.Value);
        User? user = await dbContext.Users.FindAsync(userId);

        if (user is null)
        {
            logger.LogDebug("AssignAdmin | Rejected - User not found");
            return NotFound("User not found");
        }

        if (user.Role == Roles.Admin)
        {
            logger.LogDebug("AssignAdmin | Rejected - Already is admin");
            return BadRequest("User is already admin");
        }
        
        logger.LogDebug("AssignAdmin | Accepted");

        user.Role = Roles.Admin;

        await dbContext.SaveChangesAsync();
        
        logger.LogDebug("AssignAdmin | Changed user's {userId} role to {role}", userId, Roles.Admin);

        (string accessToken, string refreshToken) = authService.IssueTokens(user);
        
        logger.LogTrace("Issued tokens for user {id} after change of roles", userId);

        return new AuthResponseDto(accessToken, refreshToken);
    }
    
    [HttpGet(nameof(AdminTest))]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult AdminTest()
    {
        logger.LogInformation("AdminTest was executed successfully");
        
        return Ok();
    }
}