namespace MyApiApp.Tests
{
    [TestFixture]
    public class RequestHistoryControllerTests
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

                    services.AddDbContext<HistoryDbContext>(options =>
                        options.UseInMemoryDatabase("TestHistoryDB"));

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

                var historyContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
                historyContext.Database.EnsureDeleted();
                historyContext.Database.EnsureCreated();
            }
        }

        [Test]
        public async Task GetRequestHistory_ShouldReturnOk_WhenHistoryExists()
        {
            // Arrange
            var userRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var json = JsonConvert.SerializeObject(userRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Регистрация пользователя
            await _client!.PostAsync("api/Auth/register", content);

            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Вход для получения токена
            var loginResponse = await _client.PostAsync("api/Auth/login", loginContent);
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonConvert.DeserializeObject<TestLoginResponse>(loginResult);
            var token = loginData?.Token;

            Assert.That(token, Is.Not.Null);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Perform an action that logs a request
            var getContactsResponse = await _client.GetAsync("api/Contacts");
            Assert.That(getContactsResponse.IsSuccessStatusCode, Is.True);

            // Act
            var response = await _client.GetAsync("api/RequestHistory");

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task DeleteRequestHistory_ShouldReturnNoContent_WhenHistoryIsDeleted()
        {
            // Arrange
            var userRequest = new UserRegisterRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var json = JsonConvert.SerializeObject(userRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Регистрация пользователя
            await _client!.PostAsync("api/Auth/register", content);

            var loginRequest = new UserLoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var loginJson = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Вход для получения токена
            var loginResponse = await _client.PostAsync("api/Auth/login", loginContent);
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonConvert.DeserializeObject<TestLoginResponse>(loginResult);
            var token = loginData?.Token;

            Assert.That(token, Is.Not.Null);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Perform an action that logs a request
            var getContactsResponse = await _client.GetAsync("api/Contacts");
            Assert.That(getContactsResponse.IsSuccessStatusCode, Is.True);

            // Act: Удаление истории запросов
            var deleteResponse = await _client.DeleteAsync("api/RequestHistory");

            // Assert
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _server?.Dispose();
        }
    }
}