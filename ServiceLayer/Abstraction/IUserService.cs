using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace ServiceLayer.Abstraction
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(User user);
        Task<string> LoginAsync(string username, string password);
    }
}
