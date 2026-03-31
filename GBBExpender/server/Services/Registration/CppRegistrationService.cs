using System.IO;
using System.Linq;

namespace GbbExpender.Services.Registration
{
    public class CppRegistrationService
    {
        public void UpdateMonitor(string path, string objectName, string classCode, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            if (!lines.Any(l => l.Contains(includeLine)))
            {
                var prefix = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/";
                var lastIndex = lines.FindLastIndex(l => l.Trim().StartsWith(prefix));
                if (lastIndex != -1) lines.Insert(lastIndex + 1, includeLine);
                else lines.Insert(0, includeLine);
            }

            if (!lines.Any(l => l.Contains($"class Monitor{objectName}"))) lines.Add(classCode);
            File.WriteAllLines(path, lines);
        }

        public void UpdateCppRegistration(string path, string objectName, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            if (!lines.Any(l => l.Contains(includeLine)))
            {
                var prefix = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/";
                var lastIndex = lines.FindLastIndex(l => l.Trim().StartsWith(prefix));
                if (lastIndex != -1) lines.Insert(lastIndex + 1, includeLine);
                else lines.Insert(lines.FindLastIndex(l => l.StartsWith("#include")) + 1, includeLine);
            }

            if (!lines.Any(l => l.Contains(objectName) && (l.Contains("ADD_DESC1") || l.Contains("ADD_MESSAGE"))))
            {
                var prefix = isMsg ? "ADD_MESSAGE" : "ADD_DESC1";
                var regCode = isMsg ? $"    ADD_MESSAGE(\"{objectName}\", {objectName}, {objectName});" : $"    ADD_DESC1({objectName});";
                var lastMacroIdx = lines.FindLastIndex(l => l.Trim().StartsWith(prefix));
                if (lastMacroIdx != -1) lines.Insert(lastMacroIdx + 1, regCode);
                else lines.Insert(lines.FindLastIndex(l => l.Trim() == "}") , regCode);
            }
            File.WriteAllLines(path, lines);
        }
    }
}