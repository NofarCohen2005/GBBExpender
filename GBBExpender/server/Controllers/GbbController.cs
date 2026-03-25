using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GbbExpender.Controllers
{
    public class PropertyModel
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = "int";
        public string DefaultValue { get; set; } = "0";
    }

    public class GeneratorRequest
    {
        public string EntryType { get; set; } = "Descriptor";
        public string ObjectName { get; set; } = string.Empty;
        public List<PropertyModel> Properties { get; set; } = new();
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GbbController : ControllerBase
    {
        [HttpPost("generate")]
        public ActionResult<dynamic> Generate([FromBody] GeneratorRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ObjectName))
                return BadRequest("Invalid request");

            request.ObjectName = Capitalize(request.ObjectName);
            request.EntryType = Capitalize(request.EntryType); // Ensure "message" -> "Message" for consistency
            foreach (var prop in request.Properties)
            {
                prop.Name = Capitalize(prop.Name);
            }

            var isMsg = string.Equals(request.EntryType, "Message", System.StringComparison.OrdinalIgnoreCase);
            var root = "/Users/nofar/Desktop/gbbexpender";

            return Ok(new
            {
                CppHeader = new { 
                    Code = GenerateCppHeader(request), 
                    Path = $"{root}/Extended_GBB/Applications/SDK/{(isMsg ? "Messages" : "Descriptors")}/{request.ObjectName}.h" 
                },
                CppMonitor = new { 
                    Code = GenerateCppMonitor(request), 
                    Path = $"{root}/Extended_GBB/Applications/SDK/{(isMsg ? "Messages/ExtendedMessagesMonitor.h" : "Descriptors/ExtendedMonitor.h")}" 
                },
                CppRegistration = new { 
                    Code = GenerateCppRegistration(request), 
                    Paths = new[] {
                        $"{root}/Extended_GBB/Applications/SDK/Inc/Extended_HT_GBB.h",
                        $"{root}/Extended_GBB/Applications/SDK/ExtendedSizeDLL/ExtendedSizeDLL.cpp"
                    }
                },
                CSharpStruct = new { 
                    Code = GenerateCSharpStruct(request), 
                    Path = $"{root}/HartechExtension_CS/NavyGBBWrapper/{(isMsg ? "Messages" : "Descriptors")}/{request.ObjectName}.cs" 
                },
                CSharpRegistration = new { 
                    Code = GenerateCSharpRegistration(request), 
                    Paths = new[] {
                        $"{root}/HartechExtension_CS/NavyGBBWrapper/NavyEnums.cs",
                        $"{root}/HartechExtension_CS/NavyGBBWrapper/NavyBaseAgent.cs"
                    }
                }
            });
        }

        private string Capitalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : "");
        }

        private string MapCppType(string type)
        {
            return type.ToLower() switch
            {
                "int" => "int",
                "uint" => "unsigned int",
                "double" => "double",
                "bool" => "bool",
                "byte" => "unsigned char",
                "string" => "char",
                _ => "int"
            };
        }

        private string MapCSharpType(string type)
        {
            return type.ToLower() switch
            {
                "int" => "int",
                "uint" => "uint",
                "double" => "double",
                "bool" => "bool",
                "byte" => "byte",
                "string" => "string",
                _ => "int"
            };
        }

        private string MapMonitorMacro(string type)
        {
            return type.ToLower() switch
            {
                "int" => "Int",
                "uint" => "UInt",
                "double" => "Double",
                "bool" => "Bool",
                "byte" => "Byte",
                "string" => "String",
                _ => "Int"
            };
        }

        private string GenerateCppHeader(GeneratorRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#include <string.h>"); // Required for memset/strncpy
            sb.AppendLine();
            sb.AppendLine("namespace HT {");
            sb.AppendLine($"    struct {request.ObjectName} {{");
            
            foreach (var prop in request.Properties)
            {
                if (prop.DataType.ToLower() == "string")
                    sb.AppendLine($"        char {prop.Name}[256];");
                else
                    sb.AppendLine($"        {MapCppType(prop.DataType)} {prop.Name};");
            }
            
            sb.AppendLine();
            sb.Append($"        {request.ObjectName}()");
            
            var podProps = request.Properties.Where(p => p.DataType.ToLower() != "string").ToList();
            if (podProps.Any())
            {
                sb.Append(" : ");
                var initializers = podProps.Select(p => $"{p.Name}({p.DefaultValue})");
                sb.Append(string.Join(", ", initializers));
            }
            
            sb.AppendLine(" {");
            
            foreach (var prop in request.Properties.Where(p => p.DataType.ToLower() == "string"))
            {
                 sb.AppendLine($"            memset({prop.Name}, 0, sizeof({prop.Name}));");
                 if (!string.IsNullOrEmpty(prop.DefaultValue) && prop.DefaultValue != "0" && prop.DefaultValue != "\"\"")
                 {
                     string val = prop.DefaultValue.StartsWith("\"") ? prop.DefaultValue : $"\"{prop.DefaultValue}\"";
                     sb.AppendLine($"            strncpy({prop.Name}, {val}, sizeof({prop.Name}) - 1);");
                 }
            }
            
            sb.AppendLine("        }");
            sb.AppendLine("    };");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateCppMonitor(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var baseHandler = isMsg ? "CMessageHandler" : "CDescHandler";
            var ptrType = isMsg ? "pMessage" : "pDesc";
            
            var sb = new StringBuilder();
            sb.AppendLine($"class Monitor{request.ObjectName} : public GBBMonitor::{baseHandler} {{");
            sb.AppendLine("public:");
            sb.AppendLine($"    Monitor{request.ObjectName}(int id) : GBBMonitor::{baseHandler}({request.ObjectName}) {{}}");
            sb.AppendLine();
            sb.AppendLine("    virtual void FillData(char* pData)");
            sb.AppendLine("    {");
            sb.AppendLine($"        HT::{request.ObjectName}* {ptrType} = (HT::{request.ObjectName}*)pData;");
            
            foreach (var prop in request.Properties)
            {
                sb.AppendLine($"        AddField{MapMonitorMacro(prop.DataType)}(\"{prop.Name}\", {ptrType}->{prop.Name});");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("};");
            return sb.ToString();
        }

        private string GenerateCppRegistration(GeneratorRequest request)
        {
            var sb = new StringBuilder();
            var isMsg = request.EntryType == "Message";
            var enumBase = isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor";
            
            sb.AppendLine("// Add to Extended_HT_GBB.h Enum:");
            sb.AppendLine($"{request.ObjectName} = {enumBase} + 1,");
            sb.AppendLine();
            sb.AppendLine("// Add to ExtendedSizeDLL.cpp:");
            var folder = isMsg ? "Messages" : "Descriptors";
            sb.AppendLine($"#include \"{folder}/{request.ObjectName}.h\"");
            if (isMsg)
                sb.AppendLine($"ADD_MESSAGE(\"{request.ObjectName}\", {request.ObjectName}, {request.ObjectName});");
            else
                sb.AppendLine($"ADD_DESC1({request.ObjectName});");
                
            return sb.ToString();
        }

        private string GenerateCSharpStruct(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var category = isMsg ? "Messages" : "Descriptors";
            
            var sb = new StringBuilder();
            sb.AppendLine($"namespace NavyGBBWrapper.{category}");
            sb.AppendLine("{");
            sb.AppendLine($"    public struct {request.ObjectName} : IBaseStruct");
            sb.AppendLine("    {");
            
            foreach (var prop in request.Properties)
            {
                sb.AppendLine($"        public {MapCSharpType(prop.DataType)} {prop.Name};");
            }
            
            sb.AppendLine();
            sb.AppendLine("        public object SetDefault()");
            sb.AppendLine("        {");
            sb.AppendLine("            return Default();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public static {request.ObjectName} Default()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return new {request.ObjectName}");
            sb.AppendLine("            {");
            
            var initializers = request.Properties.Select(p => {
                string val = p.DefaultValue;
                if (p.DataType.ToLower() == "string" && !val.StartsWith("\"")) val = $"\"{val}\"";
                return $"                {p.Name} = {val}";
            });
            sb.AppendLine(string.Join("," + Environment.NewLine, initializers));
            
            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateCSharpRegistration(GeneratorRequest request)
        {
            var isMsg = request.EntryType == "Message";
            var mapMethod = isMsg ? "MapMessages()" : "MapDescriptors()";
            var enumType = isMsg ? "NavyGBBMessageName" : "NavyGBBDescriptorName";
            var prefix = isMsg ? "" : "Descriptor";
            
            return $"{mapMethod}{enumType}.{prefix}{request.ObjectName}, typeof({request.ObjectName}));";
        }
    }
}
