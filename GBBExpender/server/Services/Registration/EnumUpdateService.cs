using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GbbExpender.Services.Registration
{
    public class EnumUpdateService
    {
        public void Update(string path, string objectName, string enumName, string baseConst)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            if (lines.Any(l => l.Contains($"{objectName} ="))) return;

            var enumStart = lines.FindIndex(l => l.Contains($"enum {enumName}") || l.Contains($"enum  {enumName}"));
            if (enumStart == -1) return;

            var maxId = 0;
            var insertIndex = -1;
            for (int i = enumStart; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                var match = Regex.Match(line, $@"{baseConst}\s*\+\s*(\d+)");
                if (match.Success)
                {
                    var val = int.Parse(match.Groups[1].Value);
                    if (val < 1000 && val > maxId) maxId = val;
                }
                if (line.Contains("ExtendedMax") || line == "};" || line == "}")
                {
                    insertIndex = i;
                    break;
                }
            }

            if (insertIndex != -1) lines.Insert(insertIndex, $"    {objectName} = {baseConst} + {maxId + 1},");
            File.WriteAllLines(path, lines);
        }
    }
}
