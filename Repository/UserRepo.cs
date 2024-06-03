using AngularApi.Context;
using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;
        public UserRepo(AppDbContext appDbContext) 
        {
            _context = appDbContext;
        }
        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;

        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CheckUsernameExistsAsync(string userName)
        {
           return await _context.Users.AnyAsync(u=>u.Username == userName);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
           return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
