using System.Text;
using GbbExpender.Models;
using GbbExpender.Utils;
using System.Linq;

namespace GbbExpender.Services.Generators
{
    public class CppHeaderGenerator
    {
        public string Generate(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var sb = new StringBuilder();
            sb.AppendLine("#pragma once");
            if (isMsg) AppendMessageIncludes(sb);
            else AppendDescriptorIncludes(sb);
            
            sb.AppendLine("\nnamespace HT {");
            var baseName = StringUtils.GetBaseName(request.ObjectName, isMsg);
            sb.AppendLine($"    struct {baseName} {{");
            foreach (var prop in request.Properties)
            {
                var pascalName = StringUtils.ToPascalCase(prop.Name);
                var type = prop.DataType.ToLower() == "string" ? "char" : MapType(prop.DataType);
                var suffix = prop.DataType.ToLower() == "string" ? $"[{(prop.Size ?? 128) * 2 + 1}]" : "";
                sb.AppendLine($"        {type} {pascalName}{suffix};");
            }
            
            sb.AppendLine($"\n        {baseName}():");
            sb.AppendLine("        {");
            foreach (var p in request.Properties)
            {
                var pascalName = StringUtils.ToPascalCase(p.Name);
                if (p.DataType.ToLower() == "string")
                {
                    var val = string.IsNullOrEmpty(p.DefaultValue) || p.DefaultValue == "0" ? "\"\"" : (p.DefaultValue.StartsWith("\"") ? p.DefaultValue : $"\"{p.DefaultValue}\"");
                    sb.AppendLine($"            strcpy({pascalName}, {val});");
                }
                else sb.AppendLine($"            {pascalName} = {MapDefaultValue(p.DataType, p.DefaultValue)};");
            }
            sb.AppendLine("        }\n    };\n}");
            return sb.ToString();
        }

        private void AppendMessageIncludes(StringBuilder sb)
        {
            sb.AppendLine("#include \"Descriptors/MonitorUtilDef.h\"");
            sb.AppendLine("#include \"GeneralTypes.h\"\n#include \"AppObjectDefs.h\"\n#include \"Inc/AppObjectDefs.h\"");
        }

        private void AppendDescriptorIncludes(StringBuilder sb)
        {
            sb.AppendLine("#include <climits>\n#include <string.h>");
        }

        private string MapType(string type) => type.ToLower() switch { "int" => "int", "uint" => "Unsigned int", "double" => "double", "bool" => "bool", "byte" => "unsigned char", "short" => "unsigned short", _ => "int" };

        private string MapDefaultValue(string dataType, string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            var lower = value.ToLower();
            if (lower == "max") return dataType.ToLower() switch { "int" => "INT_MAX", "uint" => "UINT_MAX", "double" => "DBL_MAX", "byte" => "255", "bool" => "true", _ => "0" };
            if (lower == "min") return dataType.ToLower() switch { "int" => "INT_MIN", "uint" => "0", "double" => "DBL_MIN", "byte" => "0", "bool" => "false", _ => "0" };
            return value;
        }
    }
}