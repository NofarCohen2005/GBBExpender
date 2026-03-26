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

        /// <summary>
        /// Saves a string content to a file, creating the directory if it doesn't exist.
        /// </summary>
        private void SaveFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Updates the C++ Monitor header file with a new include and the monitor class definition.
        /// Includes are placed after the last existing include in the same folder (Descriptors/Messages).
        /// </summary>
        private void UpdateMonitor(string path, string objectName, string classCode, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            // 1. Maintain grouped includes
            if (!lines.Any(l => l.Contains(includeLine)))
            {
                var prefix = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/";
                var lastIndex = lines.FindLastIndex(l => l.Trim().StartsWith(prefix));
                if (lastIndex != -1) lines.Insert(lastIndex + 1, includeLine);
                else lines.Insert(0, includeLine);
            }

            // 2. Append monitor class definition
            if (!lines.Any(l => l.Contains($"class Monitor{objectName}")))
            {
                lines.Add(classCode);
            }
            
            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Updates enum definitions in both C++ and C# files.
        /// It finds the correct enum section and increments the ID based on the highest existing value.
        /// </summary>
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

        /// <summary>
        /// Updates the C++ DLL registration file with the new descriptor/message.
        /// Macros are added after the last existing macro of the same type (ADD_DESC1 or ADD_MESSAGE).
        /// </summary>
        private void UpdateCppRegistration(string path, string objectName, bool isMsg)
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path).ToList();
            var includeLine = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/{objectName}.h\"";
            
            // 1. Automated Include Placement - Ensures imports are grouped by folder
            if (!lines.Any(l => l.Contains(includeLine)))
            {
                var prefix = $"#include \"{(isMsg ? "Messages" : "Descriptors")}/";
                var lastIndex = lines.FindLastIndex(l => l.Trim().StartsWith(prefix));
                if (lastIndex != -1) 
                {
                    lines.Insert(lastIndex + 1, includeLine); // Insert after last include in the same folder
                }
                else 
                {
                    var lastInclude = lines.FindLastIndex(l => l.StartsWith("#include"));
                    lines.Insert(lastInclude + 1, includeLine); // Fallback to last generic include
                }
            }

            // 2. Automated Macro Placement - Ensures ADD_DESC1 and ADD_MESSAGE are grouped together
            if (!lines.Any(l => l.Contains(objectName) && (l.Contains("ADD_DESC1") || l.Contains("ADD_MESSAGE"))))
            {
                var macroPrefix = isMsg ? "ADD_MESSAGE" : "ADD_DESC1";
                var regCode = isMsg ? $"    ADD_MESSAGE(\"{objectName}\", {objectName}, {objectName});" : $"    ADD_DESC1({objectName});";
                
                // Find the last occurrence of the same macro type to maintain grouping
                var lastMacroIndex = lines.FindLastIndex(l => l.Trim().StartsWith(macroPrefix));
                
                if (lastMacroIndex != -1)
                {
                    lines.Insert(lastMacroIndex + 1, regCode);
                }
                else
                {
                    // Fallback to the end of the constructor if this is the first of its kind
                    var constructorEnd = lines.FindLastIndex(l => l.Trim() == "}");
                    if (constructorEnd != -1)
                    {
                        lines.Insert(constructorEnd, regCode);
                    }
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
            else
            {
                sb.AppendLine("#include <climits>");
                sb.AppendLine("#include <string.h>");
            }
            
            sb.AppendLine("\nnamespace HT {");
            sb.AppendLine($"    struct {request.ObjectName} {{");
            foreach (var prop in request.Properties)
            {
                var pascalName = ToPascalCase(prop.Name);
                var type = prop.DataType.ToLower() == "string" ? "char" : MapCppType(prop.DataType);
                var suffix = prop.DataType.ToLower() == "string" ? "[256]" : "";
                sb.AppendLine($"        {type} {pascalName}{suffix};");
            }
            
            sb.AppendLine($"\n        {request.ObjectName}(): ");
            var podProps = request.Properties.Where(p => p.DataType.ToLower() != "string").ToList();
            var stringProps = request.Properties.Where(p => p.DataType.ToLower() == "string").ToList();
            if (podProps.Any())
            {
                for (int i = 0; i < podProps.Count; i++)
                {
                    var p = podProps[i];
                    var separator = i < podProps.Count - 1 ? "," : "";
                    var braces = (i == podProps.Count - 1 && !stringProps.Any()) ? "{}" : "";
                    sb.AppendLine($"            {ToPascalCase(p.Name)}({MapCppDefaultValue(p.DataType, p.DefaultValue)}){separator}{braces}");
                }
            }
            
            if (stringProps.Any())
            {
                sb.AppendLine("        {");
                foreach (var prop in stringProps)
                {
                    var pascalName = ToPascalCase(prop.Name);
                    sb.AppendLine($"            memset({pascalName}, 0, sizeof({pascalName}));");
                    if (!string.IsNullOrEmpty(prop.DefaultValue) && prop.DefaultValue != "0")
                    {
                        var val = prop.DefaultValue.StartsWith("\"") ? prop.DefaultValue : $"\"{prop.DefaultValue}\"";
                        sb.AppendLine($"            strncpy({pascalName}, {val}, sizeof({pascalName}) - 1);");
                    }
                }
                sb.AppendLine("        }");
            }
            else if (!podProps.Any())
            {
                sb.AppendLine("        { }");
            }

            sb.AppendLine("    };\n}");
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

        private string MapCSharpDefaultValue(string dataType, string value, bool isMsg)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            var lowerValue = value.ToLower();
            
            // Handle max/min keywords
            if (lowerValue == "max")
            {
                return dataType.ToLower() switch { "int" => "int.MaxValue", "uint" => "uint.MaxValue", "double" => "double.MaxValue", "byte" => "byte.MaxValue", "bool" => isMsg ? "1" : "true", _ => "0" };
            }
            if (lowerValue == "min")
            {
                return dataType.ToLower() switch { "int" => "int.MinValue", "uint" => "0", "double" => "double.MinValue", "byte" => "0", "bool" => isMsg ? "0" : "false", _ => "0" };
            }

            // Handle boolean literals for byte types in messages
            if (dataType.ToLower() == "bool" && isMsg)
            {
                if (lowerValue == "true") return "1";
                if (lowerValue == "false") return "0";
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
                var pascalName = ToPascalCase(prop.Name);
                var macro = MapMonitorMacro(prop.DataType, isMsg);
                sb.AppendLine($"        AddField{macro}(\"{pascalName}\", {ptrType}->{pascalName});");
            }
            sb.AppendLine("    }\n};");
            return sb.ToString();
        }

        /// <summary>
        /// Generates the C# struct code with production-standard formatting.
        /// Enforces PascalCase for property names.
        /// </summary>
        private string GenerateCSharpStruct(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var sb = new StringBuilder();
            sb.AppendLine("using Globals;\n");
            sb.AppendLine($"namespace NavyGBBWrapper.{(isMsg ? "Messages" : "Descriptors")}\n{{");
            sb.AppendLine($"    public struct {request.ObjectName} : IBaseStruct\n    {{");
            // Property definitions
            foreach (var prop in request.Properties) sb.AppendLine($"        public {MapCSharpType(prop.DataType, isMsg)} {ToPascalCase(prop.Name)};");
            
            // SetDefault method with extra space before return for visibility
            sb.AppendLine("\n        public object SetDefault()\n        {");
            foreach (var prop in request.Properties)
            {
                sb.AppendLine($"            {ToPascalCase(prop.Name)} = {MapCSharpDefaultValue(prop.DataType, prop.DefaultValue, isMsg)};");
            }
            sb.AppendLine("\n            return this;\n        }");

            sb.AppendLine($"\n        public static {request.ObjectName} Default() => ({request.ObjectName})default({request.ObjectName}).SetDefault();");
            sb.AppendLine("    }\n}");
            return sb.ToString();
        }

        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length == 1) return input.ToUpper();
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private string MapCppType(string type) => type.ToLower() switch { "int" => "int", "uint" => "unsigned int", "double" => "double", "bool" => "bool", "byte" => "unsigned char", _ => "int" };
        private string MapCSharpType(string type, bool isMsg) => type.ToLower() switch { "int" => "int", "uint" => "uint", "double" => "double", "bool" => isMsg ? "byte" : "bool", "byte" => "byte", "string" => "string", _ => "int" };
        private string MapMonitorMacro(string type, bool isMsg) => type.ToLower() switch { "int" => "Int", "uint" => isMsg ? "METID" : "UInt", "double" => "Double", "bool" => "Bool", "byte" => "Byte", "string" => "String", _ => "Int" };
    }
}
