using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceLayer.Abstraction;
using DAL.Entities;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Repository.Abstraction;
using Microsoft.Extensions.Options;
using CORE.Configuration;
using Microsoft.Extensions.Logging;
using Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ServiceLayer.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUser<User> _repository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<UserService> _logger;

        public UserService(IUser<User> repository, IOptions<JwtSettings> jwtSettings, ILogger<UserService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Attempting to log in user with username: {Username}", username);

                var users = await _repository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt for username: {Username}", username);
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }

                var passwordHasher = new PasswordHasher<User>();

                var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Invalid password for username: {Username}", username);
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }

                _logger.LogInformation("User {Username} successfully authenticated.", username);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                    Expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpiryDurationInDays),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt for username: {Username}", username);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for username: {Username}", username);
                throw new ServiceException("An error occurred during login.", ex);
            }
        }

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with username: {Username}", user.Username);

                var existingUsers = await _repository.GetAllAsync();
                if (existingUsers.Any(u => u.Username == user.Username || u.Email == user.Email))
                {
                    _logger.LogWarning("Registration failed. Username or email already exists for username: {Username}", user.Username);
                    throw new DuplicateUserException("A user with the same username or email already exists.");
                }

                await _repository.AddAsync(user);
                _logger.LogInformation("User {Username} registered successfully.", user.Username);

                return true;
            }
            catch (DuplicateUserException ex)
            {
                _logger.LogWarning(ex, "Duplicate user registration attempt for username: {Username}", user.Username);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during registration for username: {Username}", user.Username);
                throw new ServiceException("An error occurred during user registration.", ex);
            }
        }
    }

}
