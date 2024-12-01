using System.Text.RegularExpressions;

namespace CERTParser
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Splits the certificates into different files.");
                Console.WriteLine($"Usage: {appName} [CA_LIST.cer]");
                return;
            }

            string inputFileContent = File.ReadAllText(args[0]);
            var regex = new Regex(@"#([A-Za-z0-9\s]+)(?=\r?\n)", RegexOptions.Multiline);
            var matches = regex.Matches(inputFileContent);
            if (!Directory.Exists("certs"))
            {
                Directory.CreateDirectory("certs");
            }

            for (int i = 0; i < matches.Count; i++)
            {
                string certificateName = matches[i].Groups[1].Value.Trim();
                string certificateFileName = certificateName.Replace(" ", "_") + ".crt";
                string outputFilePath = Path.Combine("certs", certificateFileName);
                int startIndex = matches[i].Index + matches[i].Length;
                int endIndex = (i + 1 < matches.Count) ? matches[i + 1].Index : inputFileContent.Length;
                string certificateContent = inputFileContent.Substring(startIndex, endIndex - startIndex).Trim();
                File.WriteAllText(outputFilePath, certificateContent);
                Console.WriteLine($"Found certificate: {certificateName}");
            }
        }
    }
}