namespace GbbExpender.Utils
{
    public static class StringUtils
    {
        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length == 1) return input.ToUpper();
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static string GetBaseName(string name, bool isMsg)
        {
            if (!isMsg || string.IsNullOrEmpty(name)) return name;
            return name.StartsWith("Message") ? name.Substring(7) : name;
        }
    }
}