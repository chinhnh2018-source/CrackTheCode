using System;
using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(string username, string password);
        Task<User?> LoginAsync(string username, string password);
    }
}
