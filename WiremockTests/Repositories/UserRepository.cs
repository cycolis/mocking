using Microsoft.EntityFrameworkCore;
using MockOktaApi.Data;

namespace MockOktaApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserAsync(string username, string oobCode)
        {
            var userRecord = new UserRecord
            {
                Username = username,
                OobCode = oobCode,
                Validated = false
            };

            _dbContext.Users.Add(userRecord);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserRecord> GetUserAsync(string oobCode)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.OobCode == oobCode);
        }

        public async Task<bool> SetUserValidatedAsync(string username)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.Validated)
            {
                return false;
            }

            user.Validated = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}