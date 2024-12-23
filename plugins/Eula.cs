using System;
using System.Xml.Linq;

namespace PS4UpdateTools.plugins
{
    internal class Eula
    {
        public static void ReadEulaFile(string eulaFile, string outputFolder)
        {
            if(outputFolder == null)
                outputFolder = "eula";

            XDocument xmlDoc = XDocument.Load(eulaFile);
            foreach (var localeElement in xmlDoc.Descendants("locale"))
            {
                string lang = localeElement.Attribute("lang")?.Value;
                if (!string.IsNullOrEmpty(lang))
                {
                    string outputFilePath = Path.Combine(outputFolder, $"{lang}.txt");
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