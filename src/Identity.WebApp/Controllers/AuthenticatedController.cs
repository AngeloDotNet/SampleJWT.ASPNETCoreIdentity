﻿using Identity.BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticatedController : ControllerBase
    {
        private readonly IAuthenticatedService authenticatedService;

        public AuthenticatedController(IAuthenticatedService authenticatedService)
        {
            this.authenticatedService = authenticatedService;
        }

        /// <summary>
        /// Open the AuthenticatedService.RunAsync method to see the correct way to retrieve the user name from a Business Layer Service,
        /// without explicitly use the HttpContext (refer also to the IUserService interface).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Run()
        {
            await authenticatedService.RunAsync();
            return NoContent();
        }
    }
}
