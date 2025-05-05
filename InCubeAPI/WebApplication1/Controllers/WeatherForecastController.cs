using Microsoft.AspNetCore.Mvc;
using WebApplication1.Classes.SQLBD;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using AppInCube.Classes.SQLBD;
using System.Net.Mail;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using WebApplication1.Classes.Authorization;
using MailKit.Security;
using MailKit;


namespace WebApplication1.Controllers;



[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly AppDbContext _context; // Контекст базы данных
                                            // Временное хранилище для кода (email -> код)
    private static ConcurrentDictionary<string, string> _verificationCodes = new();
    public WeatherForecastController(AppDbContext context)
    {
        _context = context; // Инициализируем контекст
    }

    // GET: /weatherforecast/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TableBird>> GetDopInfoId(uint id)
    {
        var tableBird = await _context.TableBird
            .FindAsync(id); // Используем FindAsync для поиска по первичному ключу

        if (tableBird != null) // Проверяем, что объект найден
        {
            // Получаем количество строк в TableProgram с заданным IdProgram
            int count = await _context.TableDays
                .CountAsync(b => b.IdProgram == tableBird.IdProgram); // Считаем количество строк

            // Приводим к byte, проверяя, что значение не превышает 255
            tableBird.DaysUntilHatching = (byte)Math.Min(count, 255);
        }
        else
        {
            return NotFound(); // Возвращаем 404, если запись не найдена
        }

        return Ok(tableBird); // Возвращаем найденную запись
    }

    // GET: /weatherforecast/dop/{id}
    [HttpGet("dop/{id}")]
    public async Task<ActionResult<List<TableProgram>>> GetWeatherForecastById(uint id)
    {
        // Получаем все записи из базы данных по заданному idProgram
        var tablePrograms = await _context.TableDays
            .Include(td => td.TableTemperature) // Предполагается, что у вас есть навигационное свойство
            .Include(td => td.TableHumidity) // Предполагается, что у вас есть навигационное свойство
            .Include(td => td.TableTurn) // Предполагается, что у вас есть навигационное свойство
            .Include(td => td.TableCooling) // Предполагается, что у вас есть навигационное свойство
            .Where(td => td.IdProgram == id) // Фильтруем по IdProgram
            .OrderBy(td => td.Day) // Сортируем по полю Day
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
            return NotFound(); // Возвращаем 404, если записи не найдены
        }

        return Ok(tablePrograms); // Возвращаем найденные записи
    }


    // GET: /weatherforecast/countallprogram
    [HttpGet("countallprogram")]
    public async Task<ActionResult<uint>> GetTotalCount()
    {
        // Получаем общее количество записей в таблице TableBird
        int count = await _context.TableBird.CountAsync();

        if (count < 0)
        {
            return BadRequest("Количество записей не может быть отрицательным."); // Обработка случая, если count < 0
        }

        // Проверяем, если записи не найдены
        if (count == 0)
        {
            return NotFound("Записи не найдены."); // Возвращаем 404 с сообщением
        }

        return Ok((uint)count); // Возвращаем общее количество записей как uint
    }


    [HttpPost("sendcode")]
    public async Task<IActionResult> SendCode([FromBody] EmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email не может быть пустым.");

        string code = GenerateCode();

        bool emailSent = await SendEmailAsync(request.Email, code);
        if (!emailSent)
            return StatusCode(500, "Не удалось отправить код на почту.");

        _verificationCodes[request.Email] = code;

        return Ok("Код отправлен на почту.");
    }

    [HttpPost("verifycode")]
    public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Email и код должны быть заполнены.");

        if (!_verificationCodes.TryGetValue(request.Email, out var storedCode))
            return BadRequest("Код не найден или истек.");

        if (storedCode != request.Code)
            return BadRequest("Неверный код.");

        _verificationCodes.TryRemove(request.Email, out _);

        return Ok("Код подтвержден.");
    }

    private string GenerateCode()
    {
        var random = new Random();
        return random.Next(1000, 10000).ToString("D4");
    }
    private async Task<bool> SendEmailAsync(string email, string code)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("InCube", "i23293533@gmail.com")); // замените на ваш адрес
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Код подтверждения";
            message.Body = new TextPart("plain")
            {
                Text = $"Ваш код подтверждения: {code}"
            };


            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("i23293533@gmail.com", "podz ptui iwso suas");
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
}


