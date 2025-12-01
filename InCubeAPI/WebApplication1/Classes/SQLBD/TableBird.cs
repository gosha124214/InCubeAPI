using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Classes.SQLBD
{
    public class TableBird
    {
        [Key]
        public uint Id { get; set; }
      //  public uint IdProgram { get; set; }
        public string NameBird { get; set; }
        public string Content { get; set; }
        public DateTime DateTimeValue { get; set; }
        public byte[] ImageBirdFile { get; set; }

        [NotMapped]
        public ImageSource ImageSource { get; set; } // свойство для хранения изображения


    }
}
