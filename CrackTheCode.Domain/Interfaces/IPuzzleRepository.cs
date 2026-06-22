using System;
using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Domain.Interfaces
{
    public interface IPuzzleRepository
    {
        Task<Puzzle?> GetByIdAsync(Guid id);
        Task CreateAsync(Puzzle puzzle);
    }
}