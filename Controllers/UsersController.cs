using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BookStore_API.Contracts;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _config;

        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
                ILoggerService logger, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Registration Attempt for {username} ");
                var user = new IdentityUser { Email = username, UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                if(!result.Succeeded)
                {
                    foreach(var error in result.Errors)
                    {
                        _logger.LogInfo($"{location}: {error.Code} {error.Description}");
                    }
                    return InternalError($"{location}: {username} User Registration Failed");
                }

                await _userManager.AddToRoleAsync(user, "Administrator");
                return Created("login", new { result.Succeeded });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// User Login Endpoint
        /// </summary>
        /// <param name=""></param>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;

                _logger.LogInfo($"{location}: Login Attempt from user {username} ");
                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location}: Successfully Authemticated");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenString});
                }

                _logger.LogInfo($"{location}: Failed Authentication");
                return Unauthorized(userDTO);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Issuer"],
                claims, null, expires: DateTime.Now.AddHours(5),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Contact the Admin.");
        }
    }
}
