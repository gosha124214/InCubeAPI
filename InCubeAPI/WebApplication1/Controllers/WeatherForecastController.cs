using Microsoft.AspNetCore.Mvc;
using WebApplication1.Classes.SQLBD;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using WebApplication1.Classes.Authorization;
using AppInCube.Classes.SQLBD;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private static ConcurrentDictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();

        public WeatherForecastController(AppDbContext context)
        {
            _context = context;
        }

        // Регистрация
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email не может быть пустым.");

            var existingUser = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);
            if (existingUser != null)
                return BadRequest("Пользователь с таким email уже существует.");

            string code = GenerateCode();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            _verificationCodes[request.Email] = (code, expiry);

            bool emailSent = await SendEmailAsync(request.Email, code);
            if (!emailSent)
                return StatusCode(500, "Не удалось отправить код на почту.");

            return Ok("Код отправлен на почту для завершения регистрации.");
        }

        // Авторизация
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email не может быть пустым.");

            var user = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);
            if (user == null)
                return BadRequest("Пользователь с таким email не найден. Зарегистрируйтесь.");

            string code = GenerateCode();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            _verificationCodes[request.Email] = (code, expiry);

            bool emailSent = await SendEmailAsync(request.Email, code);
            if (!emailSent)
                return StatusCode(500, "Не удалось отправить код на почту.");

            return Ok("Код отправлен на почту для входа.");
        }


        // Проверка кода (используется и для регистрации, и для авторизации)
        [HttpPost("verifycode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
           if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                 return BadRequest("Email и код должны быть заполнены.");

           if (!_verificationCodes.TryGetValue(request.Email, out var codeInfo))
                return BadRequest("Код не найден или истек.");

           if (DateTime.UtcNow > codeInfo.Expiry)
           {
                _verificationCodes.TryRemove(request.Email, out _);
                return BadRequest("Код истек.");
           }

           if (codeInfo.Code != request.Code)
                return BadRequest("Неверный код.");

           var user = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);

           // Если пользователя нет (регистрация завершает создание)
           if (user == null)
           {
              user = new TableUser { IMail = request.Email };
              _context.TableUsers.Add(user);
              await _context.SaveChangesAsync();
           }

           // Генерация JWT токена
           var token = GenerateJwtToken(user);

           // Удаляем код после успешной проверки
           _verificationCodes.TryRemove(request.Email, out _);

        return Ok(new { Message = user == null ? "Регистрация завершена" : "Вход успешно выполнен", UserId = user.IdUser, Token = token });
        }

        // Метод для генерации JWT токена
        private string GenerateJwtToken(TableUser user)
        {
            // Утверждения (claims) для токена
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.IdUser .ToString()), // Идентификатор пользователя
        new Claim(JwtRegisteredClaimNames.Email, user.IMail) // Электронная почта пользователя
    };

            // Секретный ключ (замените на ваш сгенерированный ключ)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("G5h8jK9lM2nP3qR4sT5uV6wX7yZ8aB9cD0eF1gH2iJ3kL4mN5oP6qR7sT8uV9wX")); // Пример сгенерированного ключа
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Создание токена
            var token = new JwtSecurityToken(
                issuer: "https://37.230.158.204:5162", // Ваш статический IP
                audience: "https://37.230.158.204:5162", // Ваш статический IP
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Установите срок действия токена
                signingCredentials: creds
            );

            // Возврат токена в виде строки
            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        // Генерация 4-значного кода
        private string GenerateCode()
        {
            var random = new Random();
            return random.Next(1000, 10000).ToString("D4");
        }

        // Отправка email с кодом
        private async Task<bool> SendEmailAsync(string email, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("InCube", "i23293533@gmail.com")); // Замените на свой адрес
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Код подтверждения";
                message.Body = new TextPart("plain")
                {
                    Text = $"Ваш код подтверждения: {code}"
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    await client.AuthenticateAsync("i23293533@gmail.com", "podz ptui iwso suas"); // Используйте пароль приложения
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки почты: {ex.Message}");
                return false;
            }
        }


        // GET: /weatherforecast/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TableBirdAndProgramm>> GetInfoId(uint id)
        {
            var tableBirdAndProgramm = await _context.TableBirdAndProgramm.FindAsync(id);

            if (tableBirdAndProgramm != null)
            {
                int count = await _context.TableDays.CountAsync(b => b.IdProgram == tableBirdAndProgramm.IdProgram);
                tableBirdAndProgramm.DaysUntilHatching = (byte)Math.Min(count, 255);
            }
            else
            {
                return NotFound();
            }

            return Ok(tableBirdAndProgramm);
        }

        // GET: /weatherforecast/dop/{id}
        [HttpGet("dop/{id}")]
        public async Task<ActionResult<List<TableProgram>>> GetWeatherForecastById(uint id)
        {
            var tablePrograms = await _context.TableDays
                .Include(td => td.TableTemperature)
                .Include(td => td.TableHumidity)
                .Include(td => td.TableTurn)
                .Include(td => td.TableCooling)
                .Where(td => td.IdProgram == id)
                .OrderBy(td => td.Day)
                .Select(td => new TableProgram
                {
                    IdProgram = td.IdProgram,
                    Day = td.Day,
                    MinTemperature = td.TableTemperature.MinTemperature,
                    MaxTemperature = td.TableTemperature.MaxTemperature,
                    MinHumidity = td.TableHumidity.MinHumidity,
                    MaxHumidity = td.TableHumidity.MaxHumidity,
                    MinАmountTurn = td.TableTurn.MinАmountTurn,
                    MaxАmountTurn = td.TableTurn.MaxАmountTurn,
                    АmountCooling = td.TableCooling.АmountCooling,
                    MinTimeCooling = td.TableCooling.MinTimeCooling,
                    MaxTimeCooling = td.TableCooling.MaxTimeCooling
                })
                .ToListAsync();

            if (tablePrograms == null || !tablePrograms.Any())
            {
                return NotFound();
            }

            return Ok(tablePrograms);
        }

        // GET: /weatherforecast/countallprogram
        [HttpGet("countallprogram")]
        public async Task<ActionResult<uint>> GetTotalCount()
        {
            int count = await _context.TableBirdAndProgramm.CountAsync();

            if (count < 0)
            {
                return BadRequest("Количество записей не может быть отрицательным.");
            }

            if (count == 0)
            {
                return NotFound("Записи не найдены.");
            }

            return Ok((uint)count);
        }
    }
}
