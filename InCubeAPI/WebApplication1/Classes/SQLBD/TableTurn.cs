using System.ComponentModel.DataAnnotations;

public class TableTurn
{
    [Key]
    public uint IdTurn { get; set; }
    public byte? MinАmountTurn { get; set; }
    public byte? MaxАmountTurn { get; set; }
}