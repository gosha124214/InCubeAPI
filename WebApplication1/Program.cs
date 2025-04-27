using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Classes.SQLBD; // ����������� ������������ ���� ��� Entity Framework
using MySql.EntityFrameworkCore.Extensions; // ��� MySqlServerVersion

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ��������� �������� ���� ������
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql("User Id=goshalikhoy12;password=nv5_yFYgj-W_haJ;Host=db4free.net;Database=goshalikhoy12;Persist Security Info=True;",
            new MySqlServerVersion(new Version(5, 2, 2)))); // ������� ���� ������ MySQL

        // ��������� ������� � ���������
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // ��������� Kestrel ��� ������ � HTTPS
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(5162, listenOptions =>
            {
                listenOptions.UseHttps(); // �������� HTTPS
            });
        });

        var app = builder.Build();

        // ��������� ��������� HTTP-��������
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