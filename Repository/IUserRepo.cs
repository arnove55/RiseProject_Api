using AngularApi.Models;

namespace AngularApi.Repository
{
    public interface IUserRepo
    {
        Task<User>GetUserByUsernameAsync(string username);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckUsernameExistsAsync(string userName);
        Task<User>AddUserAsync(User user);  
        Task UpdateUserAsync(User user);
    }
}
