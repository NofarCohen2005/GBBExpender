using System.Text;
using GbbExpender.Models;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using System.IO;
using System.Linq;

namespace GbbExpender.Services
{
    public class GbbGeneratorService
    {
        private readonly GbbPaths _paths;

        public GbbGeneratorService()
        {
            _paths = new GbbPaths
            {
                WorkspaceRoot = ConfigurationManager.AppSettings["WorkspaceRoot"] ?? "",
                CppDescriptorsPath = ConfigurationManager.AppSettings["CppDescriptorsPath"] ?? "",
                CppMessagesPath = ConfigurationManager.AppSettings["CppMessagesPath"] ?? "",
                CppIncPath = ConfigurationManager.AppSettings["CppIncPath"] ?? "",
                CppDllPath = ConfigurationManager.AppSettings["CppDllPath"] ?? "",
                CsDescriptorsPath = ConfigurationManager.AppSettings["CsDescriptorsPath"] ?? "",
                CsMessagesPath = ConfigurationManager.AppSettings["CsMessagesPath"] ?? "",
                CsEnumsPath = ConfigurationManager.AppSettings["CsEnumsPath"] ?? "",
                CsAgentPath = ConfigurationManager.AppSettings["CsAgentPath"] ?? ""
            };
        }

        public void Generate(GeneratorRequest request)
        {
            var isMsg = string.Equals(request.EntryType, "Message", System.StringComparison.OrdinalIgnoreCase);
            
            // 1. Generate Content
            var cppHeader = GenerateCppHeader(request);
            var cppMonitor = GenerateCppMonitor(request);
            var csStruct = GenerateCSharpStruct(request);

            // 2. Resolve Paths
            var cppHeaderFile = Path.Combine(_paths.WorkspaceRoot, isMsg ? _paths.CppMessagesPath : _paths.CppDescriptorsPath, $"{request.ObjectName}.h");
            var monitorPath = Path.Combine(_paths.WorkspaceRoot, isMsg ? _paths.CppMessagesPath : _paths.CppDescriptorsPath, isMsg ? "ExtendedMessagesMonitor.h" : "ExtendedMonitor.h");
            var cppEnumPath = Path.Combine(_paths.WorkspaceRoot, _paths.CppIncPath, "Extended_HT_GBB.h");
            var cppDllPath = Path.Combine(_paths.WorkspaceRoot, _paths.CppDllPath, "ExtendedSizeDLL.cpp");
            var csStructFile = Path.Combine(_paths.WorkspaceRoot, isMsg ? _paths.CsMessagesPath : _paths.CsDescriptorsPath, $"{request.ObjectName}.cs");
            var csEnumPath = Path.Combine(_paths.WorkspaceRoot, _paths.CsEnumsPath);
            var csAgentPath = Path.Combine(_paths.WorkspaceRoot, _paths.CsAgentPath);

            // 3. Persist
            SaveFile(cppHeaderFile, cppHeader);
            UpdateMonitor(monitorPath, request.ObjectName, cppMonitor, isMsg);
            UpdateEnumFile(cppEnumPath, request.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor", false);
            UpdateCppRegistration(cppDllPath, request.ObjectName, isMsg);
            
            SaveFile(csStructFile, csStruct);
            UpdateEnumFile(csEnumPath, request.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor", true);
            UpdateCSharpAgent(csAgentPath, request.ObjectName, isMsg);
        }

        private void SaveFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }

        private void UpdateMonitor(string path, string objectName, string classCode, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var content = File.ReadAllText(path);
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            if (!content.Contains(includeLine)) content = includeLine + "\n" + content;
            if (!content.Contains($"class Monitor{objectName}")) content += "\n" + classCode;
            
            File.WriteAllText(path, content);
        }

        private void UpdateEnumFile(string path, string objectName, string enumName, string baseConst, bool addSummary)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var enumStart = lines.FindIndex(l => l.Contains($"enum {enumName}") || l.Contains($"enum  {enumName}"));
            if (enumStart == -1 || lines.Any(l => l.Contains($"{objectName} ="))) return;

            var maxId = 0;
            var insertIndex = -1;
            for (int i = enumStart; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                var match = System.Text.RegularExpressions.Regex.Match(line, $@"{baseConst}\s*\+\s*(\d+)");
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

            if (insertIndex != -1)
            {
                if (addSummary) lines.Insert(insertIndex++, $"    ///<summary>Descriptor for {objectName}</summary>");
                lines.Insert(insertIndex, $"    {objectName} = {baseConst} + {maxId + 1},");
            }
            File.WriteAllLines(path, lines);
        }

        private void UpdateCppRegistration(string path, string objectName, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            if (!lines.Any(l => l.Contains(includeLine)))
            {
                var lastInclude = lines.FindLastIndex(l => l.StartsWith("#include"));
                lines.Insert(lastInclude + 1, includeLine);
            }

            if (!lines.Any(l => l.Contains(objectName) && (l.Contains("ADD_DESC1") || l.Contains("ADD_MESSAGE"))))
            {
                var constructorEnd = lines.FindLastIndex(l => l.Trim() == "}");
                if (constructorEnd != -1)
                {
                    var regCode = isMsg ? $"    ADD_MESSAGE(\"{objectName}\", {objectName}, {objectName});" : $"    ADD_DESC1({objectName});";
                    lines.Insert(constructorEnd, regCode);
                }
            }
            File.WriteAllLines(path, lines);
        }

        private void UpdateCSharpAgent(string path, string objectName, bool isMsg)
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

        private string GenerateCppHeader(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var sb = new StringBuilder();
            sb.AppendLine("#pragma once");
            if (isMsg)
            {
                sb.AppendLine("#include \"Descriptors/MonitorUtilDef.h\"");
                sb.AppendLine("#include \"GeneralTypes.h\"");
                sb.AppendLine("#include \"AppObjectDefs.h\"");
                sb.AppendLine("#include \"Inc/AppObjectDefs.h\"");
            }
            else sb.AppendLine("#include <string.h>");
            
            sb.AppendLine("\nnamespace HT {");
            sb.AppendLine($"    struct {request.ObjectName} {{");
            foreach (var prop in request.Properties)
            {
                var type = prop.DataType.ToLower() == "string" ? "char" : MapCppType(prop.DataType);
                var suffix = prop.DataType.ToLower() == "string" ? "[256]" : "";
                sb.AppendLine($"        {type} {prop.Name}{suffix};");
            }
            
            sb.AppendLine($"\n        {request.ObjectName}()");
            var podProps = request.Properties.Where(p => p.DataType.ToLower() != "string").ToList();
            if (podProps.Any())
            {
                sb.Append("            : ");
                sb.AppendLine(string.Join(", ", podProps.Select(p => $"{p.Name}({MapCppDefaultValue(p.DataType, p.DefaultValue)})")));
            }
            sb.AppendLine("        {");
            foreach (var prop in request.Properties.Where(p => p.DataType.ToLower() == "string"))
            {
                sb.AppendLine($"            memset({prop.Name}, 0, sizeof({prop.Name}));");
                if (!string.IsNullOrEmpty(prop.DefaultValue) && prop.DefaultValue != "0")
                {
                    var val = prop.DefaultValue.StartsWith("\"") ? prop.DefaultValue : $"\"{prop.DefaultValue}\"";
                    sb.AppendLine($"            strncpy({prop.Name}, {val}, sizeof({prop.Name}) - 1);");
                }
            }
            sb.AppendLine("        }\n    };\n}");
            return sb.ToString();
        }

        private string MapCppDefaultValue(string dataType, string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            if (value.ToLower() == "max")
            {
                return dataType.ToLower() switch { "int" => "INT_MAX", "uint" => "UINT_MAX", "double" => "DBL_MAX", "byte" => "255", "bool" => "true", _ => "0" };
            }
            if (value.ToLower() == "min")
            {
                return dataType.ToLower() switch { "int" => "INT_MIN", "uint" => "0", "double" => "DBL_MIN", "byte" => "0", "bool" => "false", _ => "0" };
            }
            return value;
        }

        private string MapCSharpDefaultValue(string dataType, string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            var lowerValue = value.ToLower();
            if (lowerValue == "max")
            {
                return dataType.ToLower() switch { "int" => "int.MaxValue", "uint" => "uint.MaxValue", "double" => "double.MaxValue", "byte" => "byte.MaxValue", "bool" => "true", _ => "0" };
            }
            if (lowerValue == "min")
            {
                return dataType.ToLower() switch { "int" => "int.MinValue", "uint" => "0", "double" => "double.MinValue", "byte" => "0", "bool" => "false", _ => "0" };
            }
            if (dataType.ToLower() == "uint" && value.Equals("UINT_MAX", System.StringComparison.OrdinalIgnoreCase)) return "uint.MaxValue";
            if (dataType.ToLower() == "string" && !value.StartsWith("\"")) return $"\"{value}\"";
            return value;
        }

        private string GenerateCppMonitor(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var baseHandler = isMsg ? "CMessageHandler" : "CDescHandler";
            var ptrType = isMsg ? "pMessage" : "pDesc";
            var sb = new StringBuilder();
            sb.AppendLine($"\nclass Monitor{request.ObjectName} : public GBBMonitor::{baseHandler} {{");
            sb.AppendLine("public:");
            sb.AppendLine($"    Monitor{request.ObjectName}(int id) : GBBMonitor::{baseHandler}({request.ObjectName}) {{}}");
            sb.AppendLine("\n    virtual void FillData(char* pData)\n    {");
            sb.AppendLine($"        HT::{request.ObjectName}* {ptrType} = (HT::{request.ObjectName}*)pData;");
            foreach (var prop in request.Properties)
            {
                var macro = MapMonitorMacro(prop.DataType, isMsg);
                sb.AppendLine($"        AddField{macro}(\"{prop.Name}\", {ptrType}->{prop.Name});");
            }
            sb.AppendLine("    }\n};");
            return sb.ToString();
        }

        private string GenerateCSharpStruct(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var sb = new StringBuilder();
            sb.AppendLine("using Globals;\n");
            sb.AppendLine($"namespace NavyGBBWrapper.{(isMsg ? "Messages" : "Descriptors")}\n{{");
            sb.AppendLine($"    public struct {request.ObjectName} : IBaseStruct\n    {{");
            foreach (var prop in request.Properties) sb.AppendLine($"        public {MapCSharpType(prop.DataType, isMsg)} {prop.Name};");
            
            sb.AppendLine("\n        public object SetDefault()\n        {");
            foreach (var prop in request.Properties)
            {
                sb.AppendLine($"            {prop.Name} = {MapCSharpDefaultValue(prop.DataType, prop.DefaultValue)};");
            }
            sb.AppendLine("            return this;\n        }");

            sb.AppendLine($"\n        public static {request.ObjectName} Default() => ({request.ObjectName})default({request.ObjectName}).SetDefault();");
            sb.AppendLine("    }\n}");
            return sb.ToString();
        }

        private string MapCppType(string type) => type.ToLower() switch { "int" => "int", "uint" => "unsigned int", "double" => "double", "bool" => "bool", "byte" => "unsigned char", _ => "int" };
        private string MapCSharpType(string type, bool isMsg) => type.ToLower() switch { "int" => "int", "uint" => "uint", "double" => "double", "bool" => isMsg ? "byte" : "bool", "byte" => "byte", "string" => "string", _ => "int" };
        private string MapMonitorMacro(string type, bool isMsg) => type.ToLower() switch { "int" => "Int", "uint" => isMsg ? "METID" : "UInt", "double" => "Double", "bool" => "Bool", "byte" => "Byte", "string" => "String", _ => "Int" };
    }
}
