namespace MyApiApp.Tests
{
    [TestFixture]
    public class ContactsControllerTests
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

                    services.AddDbContext<ContactDbContext>(options =>
                        options.UseInMemoryDatabase("TestContactDB"));

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

                var contactContext = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                contactContext.Database.EnsureDeleted();
                contactContext.Database.EnsureCreated();
            }
        }

        [Test]
        public async Task AddContact_ShouldReturnCreated_WhenContactIsAddedSuccessfully()
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

            var contact = new Contact
            {
                Name = "Test Contact",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                Address = "123 Test St"
            };

            var contactJson = JsonConvert.SerializeObject(contact);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("api/Contacts", contactContent);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetAllContacts_ShouldReturnOk_WhenContactsAreRetrievedSuccessfully()
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

            // Добавление тестового контакта
            var contact = new Contact
            {
                Name = "Test Contact",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                Address = "123 Test St"
            };

            var contactJson = JsonConvert.SerializeObject(contact);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");
            await _client.PostAsync("api/Contacts", contactContent);

            // Act
            var response = await _client.GetAsync("api/Contacts");

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var result = await response.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task EditContact_ShouldReturnOk_WhenContactIsUpdatedSuccessfully()
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

            // Добавление тестового контакта
            var contact = new Contact
            {
                Name = "Test Contact",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                Address = "123 Test St"
            };

            var contactJson = JsonConvert.SerializeObject(contact);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("api/Contacts", contactContent);
            var addedContactResult = await addResponse.Content.ReadAsStringAsync();
            var addedContact = JsonConvert.DeserializeObject<Contact>(addedContactResult);

            // Изменение контакта
            var updatedContact = new Contact
            {
                Name = "Updated Contact",
                PhoneNumber = "0987654321",
                Email = "updated@example.com",
                Address = "321 Updated St"
            };

            var updatedContactJson = JsonConvert.SerializeObject(updatedContact);
            var updatedContactContent = new StringContent(updatedContactJson, Encoding.UTF8, "application/json");

            // Act
            var editResponse = await _client.PatchAsync($"api/Contacts/{addedContact?.Id}", updatedContactContent);

            // Assert
            Assert.That(editResponse.IsSuccessStatusCode, Is.True);
            var result = await editResponse.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task DeleteContact_ShouldReturnOk_WhenContactIsDeletedSuccessfully()
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

            // Добавление тестового контакта
            var contact = new Contact
            {
                Name = "Test Contact",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                Address = "123 Test St"
            };

            var contactJson = JsonConvert.SerializeObject(contact);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("api/Contacts", contactContent);
            var addedContactResult = await addResponse.Content.ReadAsStringAsync();
            var addedContact = JsonConvert.DeserializeObject<Contact>(addedContactResult);
            Assert.That(addedContact, Is.Not.Null);

            // Act: Удаление контакта
            var deleteResponse = await _client.DeleteAsync($"api/Contacts/{addedContact?.Id}");

            // Assert
            Assert.That(deleteResponse.IsSuccessStatusCode, Is.True);
        }

        [Test]
        public async Task GetContactById_ShouldReturnOk_WhenContactExists()
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

            // Добавление тестового контакта
            var contact = new Contact
            {
                Name = "Test Contact",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                Address = "123 Test St"
            };

            var contactJson = JsonConvert.SerializeObject(contact);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("api/Contacts", contactContent);
            var addedContactResult = await addResponse.Content.ReadAsStringAsync();
            var addedContact = JsonConvert.DeserializeObject<Contact>(addedContactResult);

            // Act: Получение контакта по ID
            var getResponse = await _client.GetAsync($"api/Contacts/{addedContact?.Id}");

            // Assert
            Assert.That(getResponse.IsSuccessStatusCode, Is.True);
            var result = await getResponse.Content.ReadAsStringAsync();
            Assert.That(result, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _server?.Dispose();
        }
    }
}