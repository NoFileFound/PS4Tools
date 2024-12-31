using PS4UpdateTools.sys;
using System;
using System.Text.RegularExpressions;
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
                string? lang = localeElement.Attribute("lang")?.Value;
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
                    Logger.LogMsg($"{lang} -> {outputFilePath}");
                }
            }
        }

        public static void SplitCertificates(string inputFile, string outputFolder)
        {
            if (outputFolder == null)
                outputFolder = "certs";

            string inputFileContent = File.ReadAllText(inputFile);
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
                string outputFilePath = Path.Combine(outputFolder, certificateFileName);
                int startIndex = matches[i].Index + matches[i].Length;
                int endIndex = (i + 1 < matches.Count) ? matches[i + 1].Index : inputFileContent.Length;
                string certificateContent = inputFileContent.Substring(startIndex, endIndex - startIndex).Trim();
                File.WriteAllText(outputFilePath, certificateContent);
                Logger.LogMsg($"Found certificate: {certificateName}");
            }
        }
    }
}