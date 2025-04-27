using Microsoft.AspNetCore.Mvc;
using WebApplication1.Classes.SQLBD;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using AppInCube.Classes.SQLBD;


namespace WebApplication1.Controllers;



[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly AppDbContext _context; // �������� ���� ������

    public WeatherForecastController(AppDbContext context)
    {
        _context = context; // �������������� ��������
    }

    // GET: /weatherforecast/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TableBird>> GetDopInfoId(uint id)
    {
        // �������� ������ �� ���� ������ �� ��������� id
        var tableBird = await _context.TableBird
            .FindAsync(id); // ���������� FindAsync ��� ������ �� ���������� �����

        if (tableBird == null)
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


}