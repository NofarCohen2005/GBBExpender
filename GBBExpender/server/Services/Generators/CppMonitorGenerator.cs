using System.Text;
using GbbExpender.Models;
using GbbExpender.Utils;

namespace GbbExpender.Services.Generators
{
    public class CppMonitorGenerator
    {
        public string Generate(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var baseHandler = isMsg ? "CMessageHandler" : "CDescHandler";
            var ptrType = isMsg ? "pMessage" : "pDesc";
            var sb = new StringBuilder();
            
            var baseName = StringUtils.GetBaseName(request.ObjectName, isMsg);
            sb.AppendLine($"\nclass Monitor{baseName} : public GBBMonitor::{baseHandler} {{");
            sb.AppendLine("public:");
            sb.AppendLine($"    Monitor{baseName}(int id) : GBBMonitor::{baseHandler}({request.ObjectName}) {{}}");
            sb.AppendLine("\n    virtual void FillData(char* pData)\n    {");
            sb.AppendLine($"        HT::{baseName}* {ptrType} = (HT::{baseName}*)pData;");
            
            foreach (var prop in request.Properties)
            {
                var pascalName = StringUtils.ToPascalCase(prop.Name);
                var macro = MapMonitorMacro(prop.DataType);
                sb.AppendLine($"        AddField{macro}(\"{pascalName}\", {ptrType}->{pascalName});");
            }
            sb.AppendLine("    }\n};");
            return sb.ToString();
        }

        private string MapMonitorMacro(string type) => type.ToLower() switch { "int" => "Int", "uint" => "METID", "double" => "Double", "bool" => "Bool", "byte" => "Byte", "string" => "String", "short" => "Short", _ => "Int" };
    }
}