using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyApiApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _userContext;
        private readonly ContactDbContext _contactContext;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(ApplicationDbContext userContext, ContactDbContext contactContext, ILogger<ContactsController> logger)
        {
            _userContext = userContext;
            _contactContext = contactContext;
            _logger = logger;
        }

        // Добавление нового контакта
        [HttpPost]
        public async Task<IActionResult> AddContact([FromBody] Contact contact)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = await _userContext.Users.FindAsync(useradd);
                if (user == null)
                {
                    return Unauthorized("Пользователь не найден.");
                }

                contact.UserAdd = useradd;

                _contactContext.Contacts.Add(contact);
                await _contactContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении контакта.");
                return StatusCode(500, "Произошла ошибка при добавлении контакта.");
            }
        }

        // Получение всех контактов пользователя
        [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = await _userContext.Users.FindAsync(useradd);
                if (user == null)
                {
                    return Unauthorized("Пользователь не найден.");
                }

                var contacts = await _contactContext.Contacts.Where(c => c.UserAdd == useradd).ToListAsync();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех контактов.");
                return StatusCode(500, "Произошла ошибка при получении контактов.");
            }
        }

        // Получение одного контакта по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContact(int id)
        {
            try
            {
                var contact = await _contactContext.Contacts.FindAsync(id);

                if (contact == null)
                {
                    return NotFound();
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении контакта по ID.");
                return StatusCode(500, "Произошла ошибка при получении контакта.");
            }
        }

        // Удаление контакта по ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                var contact = await _contactContext.Contacts.FindAsync(id);

                if (contact == null)
                {
                    return NotFound();
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                _contactContext.Contacts.Remove(contact);
                await _contactContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении контакта.");
                return StatusCode(500, "Произошла ошибка при удалении контакта.");
            }
        }
        // Обновление информации о контакте по ID
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactRequest updatedContactRequest)
        {
            try
            {
                var contact = await _contactContext.Contacts.FindAsync(id);

                if (contact == null)
                {
                    return NotFound();
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                // Обновляем только те поля, которые были переданы
                contact.Name = updatedContactRequest.Name ?? contact.Name;
                contact.PhoneNumber = updatedContactRequest.PhoneNumber ?? contact.PhoneNumber;
                contact.Email = updatedContactRequest.Email ?? contact.Email;
                contact.Address = updatedContactRequest.Address ?? contact.Address;

                await _contactContext.SaveChangesAsync();

                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении контакта.");
                return StatusCode(500, "Произошла ошибка при обновлении контакта.");
            }
        }

        // Поиск контактов по строковому запросу
        [HttpPost("search")]
        public async Task<IActionResult> SearchContacts([FromBody] string query)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = await _userContext.Users.FindAsync(useradd);
                if (user == null)
                {
                    return Unauthorized("Пользователь не найден.");
                }

                var contacts = await _contactContext.Contacts
                    .Where(c => c.UserAdd == useradd && 
                                (c.Name != null && c.Name.Contains(query) || 
                                 c.PhoneNumber != null && c.PhoneNumber.Contains(query) || 
                                 (c.Email != null && c.Email.Contains(query))))
                    .ToListAsync();

                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске контактов.");
                return StatusCode(500, "Произошла ошибка при поиске контактов.");
            }
        }
    }
}