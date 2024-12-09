namespace MyApiApp.Tests
{
    [TestFixture]
    public class PasswordServiceTests
    {
        private PasswordService? _passwordService;

        [SetUp]
        public void Setup()
        {
            _passwordService = new PasswordService(new Logger<PasswordService>(new LoggerFactory()));
        }

        [Test]
        public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
        {
            // Arrange
            var password = "Password123!";

            // Act
            string salt1 = string.Empty;
            string hash1 = _passwordService?.HashPassword(password, out salt1) ?? string.Empty;

            string salt2 = string.Empty;
            string hash2 = _passwordService?.HashPassword(password, out salt2) ?? string.Empty;

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
        {
            // Arrange
            var password = "Password123!";
            string salt = string.Empty;
            string hash = _passwordService?.HashPassword(password, out salt) ?? string.Empty;

            // Act
            bool result = _passwordService?.VerifyPassword(password, hash, salt) ?? false;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
        {
            // Arrange
            var correctPassword = "Password123!";
            var incorrectPassword = "WrongPassword123!";
            string salt = string.Empty;
            string hash = _passwordService?.HashPassword(correctPassword, out salt) ?? string.Empty;

            // Act
            bool result = _passwordService?.VerifyPassword(incorrectPassword, hash, salt) ?? false;

            // Assert
            Assert.That(result, Is.False);
        }
    }
}