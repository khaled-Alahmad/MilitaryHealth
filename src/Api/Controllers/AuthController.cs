using Application.DTOs.Auth;
using Application.DTOs.Auth; // هذا المسار حسب هيكلك
using Asp.Versioning;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AuthController(AppDbContext context, UserManager<ApplicationUser> userManager,IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
        _userManager = userManager;
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Application.DTOs.Auth.LoginRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(ApiResult.Fail("Validation errors", 400, errors, HttpContext.TraceIdentifier));
        }
        var result = await _mediator.Send(new LoginCommand(req));
        return Ok(ApiResult.Ok(result, "Login successful", 200, HttpContext.TraceIdentifier));
    }

    [HttpPost("hash-all-passwords")]
    public async Task<IActionResult> HashAllPasswords([FromServices] AppDbContext context)
    {
        var users = context.Users.ToList();
        var passwordHasher = new PasswordHasher<User>();

        foreach (var user in users)
        {
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.PasswordHash = passwordHasher.HashPassword(user, user.Password);
                user.NormalizedUserName = user.Username.ToUpper();
                user.NormalizedEmail = user.Email?.ToUpper();

                // اختياري: إزالة كلمة السر الأصلية بعد التشفير
                // user.Password = null;

                context.Update(user);
            }
        }

        await context.SaveChangesAsync();
        return Ok(new { Succeeded = true, Message = "Passwords hashed successfully" });
    }
    [AllowAnonymous]
    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(request), ct);
        return Ok(result);
    }
    [AllowAnonymous]

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(ApiResult.Fail("Validation errors", 400, errors, HttpContext.TraceIdentifier));
        }
        var success = await _mediator.Send(new LogoutCommand(request));
        return Ok(ApiResult.Ok(success, "Logout successful", 200, HttpContext.TraceIdentifier));
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(req));
        return Ok(ApiResult.Ok(result, "Token refreshed", 200, HttpContext.TraceIdentifier));
    }
}
