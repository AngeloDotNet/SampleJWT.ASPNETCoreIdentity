﻿using Identity.BusinessLayer.Services;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService identityService;

    public AuthController(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await identityService.LoginAsync(request);
        if (response != null)
        {
            return Ok(response);
        }

        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost("impersonate")]
    public async Task<IActionResult> Impersonate(Guid userId)
    {
        var response = await identityService.ImpersonateAsync(userId);
        if (response is not null)
        {
            return Ok(response);
        }

        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var response = await identityService.RefreshTokenAsync(request);
        if (response != null)
        {
            return Ok(response);
        }

        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await identityService.RegisterAsync(request);

        //return StatusCode(response.Succeeded ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, response);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var response = await identityService.ChangePasswordAsync(request);

        if (response.Result)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
}