using System;
using System.Text.RegularExpressions;

namespace CONFReader
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Give details about .conf file.");
                Console.WriteLine($"Usage: {appName} [.CONF]");
                return;
            }

            string[] lines = File.ReadAllLines(args[0]);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                bool isCommentedOut = line.Trim().StartsWith("#");
                string[] parts = isCommentedOut ? line.Substring(1).Split(',') : line.Split(',');

                if (parts.Length != 3)
                    continue;

                Console.WriteLine($"{parts[1].Trim()} -> {!isCommentedOut}");
            }
        }
    }
}