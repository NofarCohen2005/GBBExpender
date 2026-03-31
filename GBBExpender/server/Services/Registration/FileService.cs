using System.IO;

namespace GbbExpender.Services.Registration
{
    public class FileService
    {
        public void SaveFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }

        public string[] ReadLines(string path) => File.Exists(path) ? File.ReadAllLines(path) : new string[0];
        public void WriteLines(string path, System.Collections.Generic.IEnumerable<string> lines) => File.WriteAllLines(path, lines);
    }
}
