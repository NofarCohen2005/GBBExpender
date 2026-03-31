using System.Text;
using GbbExpender.Models;

namespace GbbExpender.Services.Generators
{
    public class CSharpGenerator
    {
        public string Generate(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var sb = new StringBuilder();
            sb.AppendLine("using Globals;");
            sb.AppendLine("using System.Runtime.InteropServices;\n");
            sb.AppendLine($"namespace NavyGBBWrapper.{(isMsg ? "Messages" : "Descriptors")}\n{{");
            sb.AppendLine($"    public struct {request.ObjectName} : IBaseStruct\n    {{");
            
            foreach (var prop in request.Properties)
            {
                if (prop.DataType.ToLower() == "string")
                {
                    var size = (prop.Size ?? 128) * 2 + 1;
                    sb.AppendLine($"        [MarshalAs(UnmanagedType.ByValArray, SizeConst = {size})]");
                }
                sb.AppendLine($"        public {MapType(prop.DataType)} {ToPascalCase(prop.Name)};");
            }
            
            sb.AppendLine("\n        public object SetDefault()\n        {");
            foreach (var prop in request.Properties)
            {
                var pascalName = ToPascalCase(prop.Name);
                if (prop.DataType.ToLower() == "string")
                {
                    var size = (prop.Size ?? 128) * 2 + 1;
                    var val = string.IsNullOrEmpty(prop.DefaultValue) || prop.DefaultValue == "0" ? "\"\"" : (prop.DefaultValue.StartsWith("\"") ? prop.DefaultValue : $"\"{prop.DefaultValue}\"");
                    sb.AppendLine($"            {pascalName} = NavyConverters.ToSByteArray({val}, {size});");
                }
                else
                {
                    sb.AppendLine($"            {pascalName} = {MapDefaultValue(prop.DataType, prop.DefaultValue)};");
                }
            }
            sb.AppendLine("\n            return this;\n        }");
            sb.AppendLine($"\n        public static {request.ObjectName} Default() => ({request.ObjectName})default({request.ObjectName}).SetDefault();");
            sb.AppendLine("    }\n}");
            return sb.ToString();
        }

        private string MapType(string type) => type.ToLower() switch { "int" => "int", "uint" => "uint", "double" => "double", "bool" => "byte", "byte" => "byte", "string" => "sbyte[]", "short" => "ushort", _ => "int" };

        private string MapDefaultValue(string dataType, string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            var lowerValue = value.ToLower();
            if (lowerValue == "max") return dataType.ToLower() switch { "int" => "int.MaxValue", "uint" => "uint.MaxValue", "double" => "double.MaxValue", "byte" => "byte.MaxValue", "bool" => "1", _ => "0" };
            if (lowerValue == "min") return dataType.ToLower() switch { "int" => "int.MinValue", "uint" => "0", "double" => "double.MinValue", "byte" => "0", "bool" => "0", _ => "0" };
            if (dataType.ToLower() == "bool") return lowerValue == "true" ? "1" : "0";
            return value;
        }

        private string ToPascalCase(string input) => string.IsNullOrEmpty(input) ? input : (input.Length == 1 ? input.ToUpper() : char.ToUpper(input[0]) + input.Substring(1));
    }
}
