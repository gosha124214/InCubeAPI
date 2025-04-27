using System.ComponentModel.DataAnnotations;

public class TableTemperature
{
    [Key]
    public uint IdTemperature { get; set; }
    public float MinTemperature { get; set; }
    public float MaxTemperature { get; set; }
}