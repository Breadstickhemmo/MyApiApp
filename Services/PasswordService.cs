using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MyApiApp.Services
{
    public class PasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }

        // Метод для хеширования пароля с солью
        public virtual string HashPassword(string password, out string salt)
        {
            try
            {
                using var sha256 = SHA256.Create();
                
                salt = Guid.NewGuid().ToString();

                var combinedPassword = Encoding.UTF8.GetBytes(password + salt);
                var hashedBytes = sha256.ComputeHash(combinedPassword);
                return Convert.ToBase64String(hashedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при хешировании пароля.");
                throw;
            }
        }

        public virtual bool VerifyPassword(string inputPassword, string storedHash, string salt)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var combinedPassword = Encoding.UTF8.GetBytes(inputPassword + salt);
                var hashedBytes = sha256.ComputeHash(combinedPassword);
                var inputHash = Convert.ToBase64String(hashedBytes);

                return inputHash == storedHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке пароля.");
                throw;
            }
        }
    }
}