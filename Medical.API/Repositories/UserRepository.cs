using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MedicalDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneAsync(string phoneNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }
}

