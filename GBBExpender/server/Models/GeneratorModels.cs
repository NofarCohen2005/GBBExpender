using System.Collections.Generic;

namespace GbbExpender.Models
{
    public class PropertyModel
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = "int";
        public string DefaultValue { get; set; } = "0";
        public int? Size { get; set; }
    }

    public class GeneratorRequest
    {
        public string EntryType { get; set; } = "Descriptor";
        public string ObjectName { get; set; } = string.Empty;
        public List<PropertyModel> Properties { get; set; } = new();
    }
}