using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Classes.SQLBD; // Импортируем пространство имен для Entity Framework
using MySql.EntityFrameworkCore.Extensions; // Для MySqlServerVersion

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Добавляем контекст базы данных
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql("User Id=goshalikhoy12;password=nv5_yFYgj-W_haJ;Host=db4free.net;Database=goshalikhoy12;Persist Security Info=True;",
            new MySqlServerVersion(new Version(5, 2, 2)))); // Укажите вашу версию MySQL

        // Добавляем сервисы в контейнер
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Настройка Kestrel для работы с HTTPS
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(5162, listenOptions =>
            {
                listenOptions.UseHttps(); // Включаем HTTPS
            });
        });

        var app = builder.Build();

        // Настройка конвейера HTTP-запросов
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}