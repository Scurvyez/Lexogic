namespace Lexogic
{
    public static class PrefixedConsole
    {
        private static string _prefix = "[Lexogic Debugging]";
        private static ConsoleColor _prefixColor = ConsoleColor.Cyan;

        // Allows setting a custom prefix if needed
        public static void SetPrefix(string prefix)
        {
            _prefix = prefix;
        }

        // Allows setting a custom color for the prefix
        public static void SetPrefixColor(ConsoleColor color)
        {
            _prefixColor = color;
        }

        public static void WriteLine(string message)
        {
            Console.ForegroundColor = _prefixColor;
            Console.Write($"{_prefix} ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.ForegroundColor = _prefixColor;
            Console.Write($"{_prefix} ");
            Console.ResetColor();
            Console.WriteLine(format, args);
        }
    }
}