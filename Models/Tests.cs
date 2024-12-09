namespace MyApiApp.Models
{
    public class TestJwtSettingsConfig
    {
        public required string Key { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
    }
}