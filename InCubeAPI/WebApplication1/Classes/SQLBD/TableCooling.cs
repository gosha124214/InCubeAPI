using System.ComponentModel.DataAnnotations;

public class TableCooling
{
    [Key]
    public uint IdCooling { get; set; }
    public byte? АmountCooling { get; set; }
    public TimeSpan? MinTimeCooling { get; set; }
    public TimeSpan? MaxTimeCooling { get; set; }
}