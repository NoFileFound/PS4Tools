using System.Xml.Linq;

namespace EulaParser
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Splits the eula into different files.");
                Console.WriteLine($"Usage: {appName} [EULA.XML]");
                return;
            }
            XDocument xmlDoc = XDocument.Load(args[0]);
            foreach (var localeElement in xmlDoc.Descendants("locale"))
            {
                string lang = localeElement.Attribute("lang")?.Value;
                if (!string.IsNullOrEmpty(lang))
                {
                    string outputFilePath = Path.Combine("eula", $"{lang}.txt");
                    var outputParentPath = Path.GetDirectoryName(outputFilePath);
                    if (string.IsNullOrEmpty(outputParentPath) == false)
                    {
                        Directory.CreateDirectory(outputParentPath);
                    }

                    using (StreamWriter writer = new StreamWriter(outputFilePath, false))
                    {
                        foreach (var element in localeElement.Elements())
                        {
                            writer.WriteLine($"{element.Value}");
                        }
                    }
                    Console.WriteLine($"{lang} -> {outputFilePath}");
                }
            }
        }
    }
}