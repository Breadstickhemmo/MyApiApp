namespace MyApiApp.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private TestServer? _server;
        private HttpClient? _client;

        [SetUp]
        public void Setup()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<Server>()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("TestDB"));

                    services.AddSession();

                    services.Configure<TestJwtSettingsConfig>(options =>
                    {
                        options.Key = "qwertyuiopasdfghjklzxcvbnm123456";
                    });

                    services.AddLogging(configure => configure.AddConsole());

                    services.AddTransient<PasswordService>();
                    services.AddTransient<TokenService>(provider => 
                        new TokenService("qwertyuiopasdfghjklzxcvbnm123456", provider.GetRequiredService<ILogger<TokenService>>()));
                });

            _server = new TestServer(webHostBuilder);
            _client = _server.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var scope = _server.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var json = JsonConvert.SerializeObject(userRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("api/Auth/register", content);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenUserIsNotRegisteredSuccessfully()
        {
            // Arrange
            var userRequest = new UserRegisterRequest
            {
                Username = "",
                Password = "Password123!"
            };

            var json = JsonConvert.SerializeObject(userRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("api/Auth/register", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Login_ShouldReturnOk_WhenUserLogsInSuccessfully()
        {
            // Arrange
            var registerRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var registerJson = JsonConvert.SerializeObject(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            // Регистрация пользователя
            await _client!.PostAsync("api/Auth/register", registerContent);

            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("api/Auth/login", loginContent);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenUserLogsInWithWrongCredentials()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("api/Auth/login", loginContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task ChangePassword_ShouldReturnOk_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var registerRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var registerJson = JsonConvert.SerializeObject(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            // Регистрация пользователя
            await _client!.PostAsync("api/Auth/register", registerContent);

            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act: Вход для получения токена
            var loginResponse = await _client.PostAsync("api/Auth/login", loginContent);
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonConvert.DeserializeObject<TestLoginResponse>(loginResult);
            var token = loginData?.Token;

            Assert.That(token, Is.Not.Null);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "Password123!",
                NewPassword = "NewPassword123!"
            };

            var changePasswordJson = JsonConvert.SerializeObject(changePasswordRequest);
            var changePasswordContent = new StringContent(changePasswordJson, Encoding.UTF8, "application/json");

            // Act: Смена пароля
            var response = await _client.PatchAsync("api/Auth/password", changePasswordContent);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task ChangePassword_ShouldReturnUnauthorized_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var registerRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var registerJson = JsonConvert.SerializeObject(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            // Регистрация пользователя
            await _client!.PostAsync("api/Auth/register", registerContent);

            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act: Вход для получения токена
            var loginResponse = await _client.PostAsync("api/Auth/login", loginContent);
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonConvert.DeserializeObject<TestLoginResponse>(loginResult);
            var token = loginData?.Token;

            Assert.That(token, Is.Not.Null);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123!"
            };

            var changePasswordJson = JsonConvert.SerializeObject(changePasswordRequest);
            var changePasswordContent = new StringContent(changePasswordJson, Encoding.UTF8, "application/json");

            // Act: Смена пароля с неправильным текущим паролем
            var response = await _client.PatchAsync("api/Auth/password", changePasswordContent);

            // Assert: Должен вернуть Unauthorized (401) при неправильном текущем пароле
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _server?.Dispose();
        }
    }
}