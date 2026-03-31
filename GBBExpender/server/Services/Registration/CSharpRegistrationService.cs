using System.IO;
using System.Linq;

namespace GbbExpender.Services.Registration
{
    public class CSharpRegistrationService
    {
        public void UpdateAgent(string path, string objectName, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var methodName = isMsg ? "NavyMessages" : "NavyDescriptors";
            var methodStart = lines.FindIndex(l => l.Contains($"void {methodName}"));
            if (methodStart == -1) return;

            var regCall = isMsg ? $"    MapMessage(NavyGBBMessageName.{objectName}, typeof({objectName}));" : $"    MapDescriptors(NavyGBBDescriptorName.Descriptor{objectName}, typeof({objectName}));";
            if (!lines.Any(l => l.Contains(regCall)))
            {
                for (int i = methodStart; i < lines.Count; i++)
                {
                    if (lines[i].Trim() == "}") { lines.Insert(i, regCall); break; }
                }
            }
            File.WriteAllLines(path, lines);
        }
    }
}