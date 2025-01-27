using DAL.Entities;
using ServiceLayer.Abstraction;
using UserManagementApp.Services.Abstraction;
using UserRegistration.DTO;

namespace UserManagementApp.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterUserAsync(RegisterDTO model)
        {
            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Telephone = model.Telephone,
                Email = model.Email
            };

            var result = await _userService.RegisterAsync(user);

            if (!result)
            {
                return (false, "User already exists.");
            }

            return (true, null);
        }
    }

}
