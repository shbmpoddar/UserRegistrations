using DAL.Entities;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstraction;
using UserRegistration.DTO;

namespace UserRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
               
                if (!ModelState.IsValid)
                    return BadRequest(new { Error = "Invalid input", Details = ModelState });

                var user = new User
                {
                    Username = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Telephone = model.Telephone,
                    Email = model.Email
                };

                var passwordHasher = new PasswordHasher<User>();
                user.Password = passwordHasher.HashPassword(user, model.Password);

                var result = await _userService.RegisterAsync(user);

                if (!result)
                    return Conflict(new { Error = "User already exists." });

                return Ok(new { Message = "Registration successful." });
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var token = await _userService.LoginAsync(model.Username, model.Password);

            if (token == null)
                return Unauthorized("Invalid username or password.");

            return Ok(new { Token = token });
        }
    }
}
