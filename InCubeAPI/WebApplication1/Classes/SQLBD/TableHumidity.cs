using System.ComponentModel.DataAnnotations;

public class TableHumidity
{
    [Key]
    public uint IdHumidity { get; set; }
    public int MinHumidity { get; set; }
    public int MaxHumidity { get; set; }
}
