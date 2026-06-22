using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Interfaces;
using CrackTheCode.Infrastructure.Data;

namespace CrackTheCode.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CrackTheCodeDbContext _context;

        public UserRepository(CrackTheCodeDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
