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
    private readonly AppDbContext _context; // Контекст базы данных

    public WeatherForecastController(AppDbContext context)
    {
        _context = context; // Инициализируем контекст
    }

    // GET: /weatherforecast/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TableBird>> GetDopInfoId(uint id)
    {
        // Получаем запись из базы данных по заданному id
        var tableBird = await _context.TableBird
            .FindAsync(id); // Используем FindAsync для поиска по первичному ключу

        if (tableBird == null)
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


}