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
    private readonly AppDbContext _context; // �������� ���� ������
                                            // ��������� ��������� ��� ���� (email -> ���)
    private static ConcurrentDictionary<string, string> _verificationCodes = new();
    public WeatherForecastController(AppDbContext context)
    {
        _context = context; // �������������� ��������
    }

    // GET: /weatherforecast/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TableBird>> GetDopInfoId(uint id)
    {
        var tableBird = await _context.TableBird
            .FindAsync(id); // ���������� FindAsync ��� ������ �� ���������� �����

        if (tableBird != null) // ���������, ��� ������ ������
        {
            // �������� ���������� ����� � TableProgram � �������� IdProgram
            int count = await _context.TableDays
                .CountAsync(b => b.IdProgram == tableBird.IdProgram); // ������� ���������� �����

            // �������� � byte, ��������, ��� �������� �� ��������� 255
            tableBird.DaysUntilHatching = (byte)Math.Min(count, 255);
        }
        else
        {
            return NotFound(); // ���������� 404, ���� ������ �� �������
        }

        return Ok(tableBird); // ���������� ��������� ������
    }

    // GET: /weatherforecast/dop/{id}
    [HttpGet("dop/{id}")]
    public async Task<ActionResult<List<TableProgram>>> GetWeatherForecastById(uint id)
    {
        // �������� ��� ������ �� ���� ������ �� ��������� idProgram
        var tablePrograms = await _context.TableDays
            .Include(td => td.TableTemperature) // ��������������, ��� � ��� ���� ������������� ��������
            .Include(td => td.TableHumidity) // ��������������, ��� � ��� ���� ������������� ��������
            .Include(td => td.TableTurn) // ��������������, ��� � ��� ���� ������������� ��������
            .Include(td => td.TableCooling) // ��������������, ��� � ��� ���� ������������� ��������
            .Where(td => td.IdProgram == id) // ��������� �� IdProgram
            .OrderBy(td => td.Day) // ��������� �� ���� Day
            .Select(td => new TableProgram
            {
                IdProgram = td.IdProgram,
                Day = td.Day,
                MinTemperature = td.TableTemperature.MinTemperature,
                MaxTemperature = td.TableTemperature.MaxTemperature,
                MinHumidity = td.TableHumidity.MinHumidity,
                MaxHumidity = td.TableHumidity.MaxHumidity,
                Min�mountTurn = td.TableTurn.Min�mountTurn,
                Max�mountTurn = td.TableTurn.Max�mountTurn,
                �mountCooling = td.TableCooling.�mountCooling,
                MinTimeCooling = td.TableCooling.MinTimeCooling,
                MaxTimeCooling = td.TableCooling.MaxTimeCooling
            })
            .ToListAsync();

        if (tablePrograms == null || !tablePrograms.Any())
        {
            return NotFound(); // ���������� 404, ���� ������ �� �������
        }

        return Ok(tablePrograms); // ���������� ��������� ������
    }


    // GET: /weatherforecast/countallprogram
    [HttpGet("countallprogram")]
    public async Task<ActionResult<uint>> GetTotalCount()
    {
        // �������� ����� ���������� ������� � ������� TableBird
        int count = await _context.TableBird.CountAsync();

        if (count < 0)
        {
            return BadRequest("���������� ������� �� ����� ���� �������������."); // ��������� ������, ���� count < 0
        }

        // ���������, ���� ������ �� �������
        if (count == 0)
        {
            return NotFound("������ �� �������."); // ���������� 404 � ����������
        }

        return Ok((uint)count); // ���������� ����� ���������� ������� ��� uint
    }


    [HttpPost("sendcode")]
    public async Task<IActionResult> SendCode([FromBody] EmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email �� ����� ���� ������.");

        string code = GenerateCode();

        bool emailSent = await SendEmailAsync(request.Email, code);
        if (!emailSent)
            return StatusCode(500, "�� ������� ��������� ��� �� �����.");

        _verificationCodes[request.Email] = code;

        return Ok("��� ��������� �� �����.");
    }

    [HttpPost("verifycode")]
    public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Email � ��� ������ ���� ���������.");

        if (!_verificationCodes.TryGetValue(request.Email, out var storedCode))
            return BadRequest("��� �� ������ ��� �����.");

        if (storedCode != request.Code)
            return BadRequest("�������� ���.");

        _verificationCodes.TryRemove(request.Email, out _);

        return Ok("��� �����������.");
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
            message.From.Add(new MailboxAddress("InCube", "i23293533@gmail.com")); // �������� �� ��� �����
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "��� �������������";
            message.Body = new TextPart("plain")
            {
                Text = $"��� ��� �������������: {code}"
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
            Console.WriteLine($"������ �������� �����: {ex.Message}");
            return false;
        }
    }
}


