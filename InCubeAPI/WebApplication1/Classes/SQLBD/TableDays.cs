using System.ComponentModel.DataAnnotations;

public class TableDays
{
    [Key]
    public uint IdProgram { get; set; }
    public byte Day { get; set; }
    public uint IdTemperature { get; set; }
    public uint IdHumidity { get; set; }
    public uint? IdTurn { get; set; }
    public uint? IdCooling { get; set; }

    // Навигационные свойства
    public virtual TableTemperature TableTemperature { get; set; }
    public virtual TableHumidity TableHumidity { get; set; }
    public virtual TableTurn TableTurn { get; set; }
    public virtual TableCooling TableCooling { get; set; }
}