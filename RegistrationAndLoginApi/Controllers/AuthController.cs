using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrationAndLoginApi.Auth;
using RegistrationAndLoginApi.DataAccess;
using RegistrationAndLoginApi.DataAccess.Entities;
using RegistrationAndLoginApi.Models;
using RegistrationAndLoginApi.Services;

namespace RegistrationAndLoginApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/v1/[controller]")]
public class AuthController(IPasswordService passwordService, IAuthService authService, AppDbContext dbContext, 
    ILogger<AuthController> logger)
    : ControllerBase
{
    [HttpPost(nameof(Register))]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto registerRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogDebug("Register | Model invalid");
            return BadRequest();
        }
        
        string sanitizedUsername = Regex.Replace(registerRequest.Username, @"[^\w\d\-]", "");
        string sanitizedFullName = Regex.Replace(registerRequest.FullName, @"[^\w\d\s\-]", "");

        registerRequest.Username = sanitizedUsername.Trim();
        registerRequest.FullName = sanitizedFullName;
        
        logger.LogTrace("Register Start");

        if (dbContext.Users.Any(u => u.Username == registerRequest.Username))
        {
            return Conflict("Username already exists");
        }

        var user = new User
        {
            Username = registerRequest.Username,
            FullName = registerRequest.FullName
        };
        
        user.PasswordHash = passwordService.HashPassword(user, registerRequest.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Registered {username}", user.Username);

        (string accessToken, string refreshToken) = authService.IssueTokens(user);

        logger.LogTrace("Issued tokens for user {id} after registration", user.Id);

        return new AuthResponseDto(accessToken, refreshToken);
    }

    [HttpPost(nameof(Login))]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogDebug("Login | Model invalid");
            return Unauthorized();
        }
        
        logger.LogTrace("Login Start");
        
        loginRequest.Username = loginRequest.Username.Trim();

        User? user = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == loginRequest.Username);

        if (user is null)
        {
            logger.LogDebug("Login | Rejected (no such user)");
            return Unauthorized();
        }

        if (!passwordService.VerifyPassword(user, loginRequest.Password))
        {
            logger.LogDebug("Login | Rejected (incorrect password)");
            return Unauthorized();
        }

        (string accessToken, string refreshToken) = authService.IssueTokens(user);

        logger.LogTrace("Issued tokens for user {id}", user.Id);

        return new AuthResponseDto(accessToken, refreshToken);
    }

    [HttpPost(nameof(Refresh))]
    public ActionResult<AuthResponseDto> Refresh([FromBody] RefreshRequestDto refreshRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogDebug("Refresh | Model invalid");
            return Unauthorized();
        }
        
        logger.LogTrace("Refresh Start");

        try
        {
            (string accessToken, string refreshToken) = authService.RefreshTokens(refreshRequest.RefreshToken);
            
            logger.LogTrace("Issued tokens for a refresh token");
            
            return new AuthResponseDto(accessToken, refreshToken);
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogDebug(e, "Refresh | Rejected (invalid token)");

            return Unauthorized();
        }
    }
}