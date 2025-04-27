using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInCube.Classes.SQLBD
{
    public class TableProgram
    {
        [Key]
        public uint IdProgram { get; set; }
        public byte Day { get; set; }
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public int MinHumidity { get; set; }
        public int MaxHumidity { get; set; }
        public byte? MinАmountTurn { get; set; }
        public byte? MaxАmountTurn { get; set; }
        public byte? АmountCooling { get; set; }
        public TimeSpan? MinTimeCooling { get; set; }
        public TimeSpan? MaxTimeCooling { get; set; }

    }
}
