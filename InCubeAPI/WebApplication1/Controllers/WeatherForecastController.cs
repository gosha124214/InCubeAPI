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

        // �����������
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email �� ����� ���� ������.");

            var existingUser = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);
            if (existingUser != null)
                return BadRequest("������������ � ����� email ��� ����������.");

            string code = GenerateCode();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            _verificationCodes[request.Email] = (code, expiry);

            bool emailSent = await SendEmailAsync(request.Email, code);
            if (!emailSent)
                return StatusCode(500, "�� ������� ��������� ��� �� �����.");

            return Ok("��� ��������� �� ����� ��� ���������� �����������.");
        }

        // �����������
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email �� ����� ���� ������.");

            var user = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);
            if (user == null)
                return BadRequest("������������ � ����� email �� ������. �����������������.");

            string code = GenerateCode();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            _verificationCodes[request.Email] = (code, expiry);

            bool emailSent = await SendEmailAsync(request.Email, code);
            if (!emailSent)
                return StatusCode(500, "�� ������� ��������� ��� �� �����.");

            return Ok("��� ��������� �� ����� ��� �����.");
        }

        // �������� ���� (������������ � ��� �����������, � ��� �����������)
        [HttpPost("verifycode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Email � ��� ������ ���� ���������.");

            if (!_verificationCodes.TryGetValue(request.Email, out var codeInfo))
                return BadRequest("��� �� ������ ��� �����.");

            if (DateTime.UtcNow > codeInfo.Expiry)
            {
                _verificationCodes.TryRemove(request.Email, out _);
                return BadRequest("��� �����.");
            }

            if (codeInfo.Code != request.Code)
                return BadRequest("�������� ���.");

            var user = await _context.TableUsers.FirstOrDefaultAsync(u => u.IMail == request.Email);

            // ���� ������������ ��� (����������� ��������� ��������)
            if (user == null)
            {
                user = new TableUser { IMail = request.Email };
                _context.TableUsers.Add(user);
                await _context.SaveChangesAsync();
                _verificationCodes.TryRemove(request.Email, out _);
                return Ok(new { Message = "����������� ���������", UserId = user.IdUser });
            }

            // ���� ������������ ���� (����������� ������������)
            _verificationCodes.TryRemove(request.Email, out _);
            return Ok(new { Message = "���� ������� ��������", UserId = user.IdUser });
        }

        // ��������� 4-�������� ����
        private string GenerateCode()
        {
            var random = new Random();
            return random.Next(1000, 10000).ToString("D4");
        }

        // �������� email � �����
        private async Task<bool> SendEmailAsync(string email, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("InCube", "i23293533@gmail.com")); // �������� �� ���� �����
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "��� �������������";
                message.Body = new TextPart("plain")
                {
                    Text = $"��� ��� �������������: {code}"
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    await client.AuthenticateAsync("i23293533@gmail.com", "podz ptui iwso suas"); // ����������� ������ ����������
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


        // GET: /weatherforecast/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TableBird>> GetDopInfoId(uint id)
        {
            var tableBird = await _context.TableBird.FindAsync(id);

            if (tableBird != null)
            {
                int count = await _context.TableDays.CountAsync(b => b.IdProgram == tableBird.IdProgram);
                tableBird.DaysUntilHatching = (byte)Math.Min(count, 255);
            }
            else
            {
                return NotFound();
            }

            return Ok(tableBird);
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
                    Min�mountTurn = td.TableTurn.Min�mountTurn,
                    Max�mountTurn = td.TableTurn.Max�mountTurn,
                    �mountCooling = td.TableCooling.�mountCooling,
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
            int count = await _context.TableBird.CountAsync();

            if (count < 0)
            {
                return BadRequest("���������� ������� �� ����� ���� �������������.");
            }

            if (count == 0)
            {
                return NotFound("������ �� �������.");
            }

            return Ok((uint)count);
        }
    }
}
