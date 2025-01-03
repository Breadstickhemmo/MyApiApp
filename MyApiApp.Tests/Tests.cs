namespace MyApiApp.Tests
{
    public class TestJwtSettingsConfig
    {
        public required string Key { get; set; }
    }

    public class TestLoginResponse
    {
        public required string Token { get; set; }
    }
}