using System;
using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task CreateAsync(User user);
    }
}
