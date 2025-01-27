using UserRegistration.DTO;

namespace UserManagementApp.Services.Abstraction
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string ErrorMessage)> RegisterUserAsync(RegisterDTO model);
    }
}
