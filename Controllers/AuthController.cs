using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project_X.Configuration;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project_X.Database;
using Microsoft.EntityFrameworkCore;
using Project_X.Models;
using Project_X.Services;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (ModelState.IsValid)
            {
                var authResult = await _authService.LoginAsync(loginRequest.Email, loginRequest.Password);

                if (!authResult.Succeeded)
                {
                    var errorResponse = new ErrorResponse
                    {
                        Errors = authResult.Errors
                    };

                    return BadRequest(errorResponse);
                }

                var authResponse = new AuthResponse
                {
                    Token = authResult.Token,
                    RefreshToken = authResult.RefreshToken
                };

                return Ok(authResponse);
            }
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (ModelState.IsValid)
            {
                var authResult = await _authService.RegisterAsync(registerRequest.Email, registerRequest.password);

                if (!authResult.Succeeded)
                {
                    var errorResponse = new ErrorResponse
                    {
                        Errors = authResult.Errors
                    };

                    return BadRequest(errorResponse);
                }

                var authResponse = new AuthResponse
                {
                    Token = authResult.Token,
                    RefreshToken = authResult.RefreshToken
                };

                return Ok(authResponse);
            }
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpPost]
        [Route("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if (ModelState.IsValid)
            {
                var authResult = await _authService.RefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken);

                if (!authResult.Succeeded)
                {
                    var errorResponse = new ErrorResponse
                    {
                        Errors = authResult.Errors
                    };

                    return BadRequest(errorResponse);
                }

                var authResponse = new AuthResponse
                {
                    Token = authResult.Token,
                    RefreshToken = authResult.RefreshToken
                };

                return Ok(authResponse);
            }
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }
    }
}
