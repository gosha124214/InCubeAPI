using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Classes.SQLBD; // Импортируем пространство имен для Entity Framework
using MySql.EntityFrameworkCore.Extensions; // Для MySqlServerVersion
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        // Настройка JWT аутентификации
        var key = Encoding.UTF8.GetBytes("G5h8jK9lM2nP3qR4sT5uV6wX7yZ8aB9cD0eF1gH2iJ3kL4mN5oP6qR7sT8uV9wX"); // Ваш секретный ключ
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://37.230.158.204:5162", // Ваш статический IP
                ValidAudience = "https://37.230.158.204:5162", // Ваш статический IP
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

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

        app.UseAuthentication(); // Добавляем аутентификацию
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
