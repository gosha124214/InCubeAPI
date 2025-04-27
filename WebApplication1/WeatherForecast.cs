namespace WebApplication1
{
    using System.ComponentModel.DataAnnotations;

    public class WeatherForecast
    {
        [Key] // Этот атрибут помечает свойство как первичный ключ
        public int Id { get; set; } // Первичный ключ

        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
    }
}
