using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Classes.SQLBD
{
    public class TableBirdAndProgramm
    {
        public uint IdBird { get; set; }
        public uint IdProgram { get; set; }

        [NotMapped]
        public byte DaysUntilHatching { get; set; }
    }
}
