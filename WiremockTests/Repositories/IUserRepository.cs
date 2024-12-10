using MockOktaApi.Data;

namespace MockOktaApi.Repositories
{
    public interface IUserRepository
    {
        Task AddUserAsync(string username, string oobCode);
        Task<UserRecord> GetUserAsync(string oobCode);
        Task<bool> SetUserValidatedAsync(string username);
    }
}