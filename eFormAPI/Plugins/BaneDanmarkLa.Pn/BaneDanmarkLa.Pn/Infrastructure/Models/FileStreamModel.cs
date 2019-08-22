using System.IO;

namespace BaneDanmarkLa.Pn.Infrastructure.Models
{
    public class FileStreamModel
    {
        public string FilePath { get; set; }
        public FileStream FileStream { get; set; }
    }
}