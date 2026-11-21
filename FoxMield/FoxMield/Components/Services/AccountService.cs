
using FoxMield.App_Data;
using FoxMield.Components.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace FoxMield.Components.Services
{
    public class AccountService
    {
        private readonly MongoDbContext _database;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();

        public AccountService(MongoDbContext database)
        {
            _database = database;
        }

        public async Task<ApplicationUser> Register(
            string username,
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Username is required.");

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required.");

            if (password.Length < 8)
                throw new Exception("Password must be at least 8 characters.");

            var existingUser = await _database.Users
                .Find(u => u.Username == username || u.Email == email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                throw new Exception("Username or email is already taken.");

            var user = new ApplicationUser
            {
                Username = username.Trim(),
                Email = email.Trim().ToLowerInvariant()
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                password
            );

            await _database.Users.InsertOneAsync(user);

            return user;
        }

        public async Task<ApplicationUser?> Login(
            string email,
            string password)
        {
            var user = await _database.Users
                .Find(u => u.Email == email.Trim().ToLowerInvariant())
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }
    }
}
