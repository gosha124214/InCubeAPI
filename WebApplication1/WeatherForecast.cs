namespace WebApplication1
{
    using System.ComponentModel.DataAnnotations;

    public class WeatherForecast
    {
        [Key] // ���� ������� �������� �������� ��� ��������� ����
        public int Id { get; set; } // ��������� ����

        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
    }
}
