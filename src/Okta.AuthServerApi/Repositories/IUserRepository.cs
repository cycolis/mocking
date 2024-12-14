using Okta.AuthServerApi.Data;

namespace Okta.AuthServerApi.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(string username, string oobCode);
    Task<UserRecord> GetUserAsync(string oobCode);
    Task<bool> SetUserValidatedAsync(string username);
}
