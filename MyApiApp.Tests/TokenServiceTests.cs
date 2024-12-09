using NUnit.Framework;
using MyApiApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace MyApiApp.Tests
{
    [TestFixture]
    public class TokenServiceTests
    {
        private TokenService? _tokenService;

        [SetUp]
        public void Setup()
        {
            var secretKey = "qwertyuiopasdfghjklzxcvbnm123456";
            _tokenService = new TokenService(secretKey, new Logger<TokenService>(new LoggerFactory()));
        }

        [Test]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var username = "testuser";
            var userId = 1;

            // Act
            var token = _tokenService?.GenerateToken(username, userId);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            Assert.That(jwtToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }

        [Test]
        public void GenerateToken_ShouldIncludeUsernameAndUserIdInClaims()
        {
            // Arrange
            var username = "testuser";
            var userId = 1;

            // Act
            var token = _tokenService?.GenerateToken(username, userId);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();

            Assert.That(claims.FirstOrDefault(c => c.Type == "unique_name")?.Value, Is.EqualTo(username));
            Assert.That(claims.FirstOrDefault(c => c.Type == "nameid")?.Value, Is.EqualTo(userId.ToString()));
        }
    }
}