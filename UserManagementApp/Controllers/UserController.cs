using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Models;
using System.Net.Http;
using UserManagementApp.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace UserManagementApp.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;


        public UserController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, JsonSerializerOptions jsonSerializerOptions)
        {
            _httpClient = httpClientFactory.CreateClient();
            _jsonSerializerOptions = jsonSerializerOptions;
            var baseUrl = apiSettings.Value.BaseUrl;
            _httpClient.BaseAddress = new Uri(baseUrl);

        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    HttpContext.Response.Cookies.Append("JwtToken", tokenResponse.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict, 
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    return RedirectToAction("Authview", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }


        [HttpPost]
        [Route("/user/register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var registerDto = new
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Telephone = model.Telephone,
                Email = model.Email
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");

            }

            var errorContent = await response.Content.ReadAsStringAsync();


            var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(
                errorContent, _jsonSerializerOptions);

            if (errorResponse?.Errors != null)
            {
                foreach (var error in errorResponse.Errors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
            }

            return View(model);
        }

        public class ValidationErrorResponse
        {
            public Dictionary<string, List<string>> Errors { get; set; }
        }

        [HttpGet]
        [Route("/user/register")]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }
    }
}
