using AppInCube.Classes.SQLBD;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1;

// Контекст базы данных для Entity Framework
namespace WebApplication1.Classes.SQLBD
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet для работы с таблицей WeatherForecasts
        public DbSet<WeatherForecast> WeatherForecasts { get; set; }

        public DbSet<TableBird> TableBird { get; set; }
        public DbSet<TableProgram> TableProgram { get; set; } // Исправлено: должно быть TableDays, а не TableProgram
        public DbSet<TableTemperature> TableTemperature { get; set; } // Добавлено
        public DbSet<TableHumidity> TableHumidity { get; set; } // Добавлено
        public DbSet<TableTurn> TableTurn { get; set; } // Добавлено
        public DbSet<TableCooling> TableCooling { get; set; } // Добавлено
        public DbSet<TableDays> TableDays { get; set; }

        // Добавляем DbSet для пользователей
        public DbSet<TableUser> TableUsers { get; set; } // Добавлено

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableDays>()
                .HasKey(td => new { td.IdProgram, td.Day });

            modelBuilder.Entity<TableDays>()
                .HasOne(td => td.TableTemperature)
                .WithMany() // Укажите, если у вас есть навигационное свойство в TableTemperature
                .HasForeignKey(td => td.IdTemperature);

            modelBuilder.Entity<TableDays>()
                .HasOne(td => td.TableHumidity)
                .WithMany() // Укажите, если у вас есть навигационное свойство в TableHumidity
                .HasForeignKey(td => td.IdHumidity);

            modelBuilder.Entity<TableDays>()
                .HasOne(td => td.TableTurn)
                .WithMany() // Укажите, если у вас есть навигационное свойство в TableTurn
                .HasForeignKey(td => td.IdTurn);

            modelBuilder.Entity<TableDays>()
                .HasOne(td => td.TableCooling)
                .WithMany() // Укажите, если у вас есть навигационное свойство в TableCooling
                .HasForeignKey(td => td.IdCooling);

            // Настройка для TableUser , если необходимо
            modelBuilder.Entity<TableUser>()
                .HasKey(u => u.IdUser); // Установка первичного ключа

            modelBuilder.Entity<TableUser>()
                .Property(u => u.IMail)
                .IsRequired() // Убедитесь, что email обязателен
                .HasMaxLength(100); // Установка максимальной длины для email
        }
    }
}
