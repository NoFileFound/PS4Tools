using PS4UpdateTools.plugins;
using PS4UpdateTools.sys;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Program
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("Universal tools to work with PS4 update file.");

            // add all commands
            rootCommand.AddCommand(CreateCommand_SLB2Extract());
            rootCommand.AddCommand(CreateCommand_MakeSLB2());
            rootCommand.AddCommand(CreateCommand_ENVDecrypt());
            rootCommand.AddCommand(CreateCommand_ENVEncrypt());

            rootCommand.AddCommand(CreateCommand_RCOExtract());

            rootCommand.AddCommand(CreateCommand_EulaSplit());
            rootCommand.AddCommand(CreateCommand_CERTSplitter());
            rootCommand.AddCommand(CreateCommand_SFOInform());

            rootCommand.AddCommand(CreateCommand_VAG2WAV());
            rootCommand.AddCommand(CreateCommand_WAV2VAG());

            // add all arguments
            Argument log = CreateArgumehnt_WriteLog();
            rootCommand.AddArgument(log);

            if (args.Length == 0)
            {
                args = new[] { "--help" }; // stupid way
            }

            await rootCommand.InvokeAsync(args);
        }

        private static Command CreateCommand_SLB2Extract()
        {
            var command = new Command("slb2extract", "Extracts a SLB2 or PUP file.");

            var inArg = new Option<string>("--in", description: "The input file."){IsRequired = true};
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output directory.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outputDirectory) => SLB2.ExtractEntry(inputFile, outputDirectory), inArg, outArg);
            return command;
        }

        private static Command CreateCommand_MakeSLB2()
        {
            var command = new Command("makeslb2", "Makes a new slb2 file from given directory.");

            var inArg = new Option<string>("--in", description: "The input directory.") { IsRequired = true };
            command.AddOption(inArg);

            var inVers = new Option<string>("--version", description: "The version (1 - normal file, 2 - update).") { IsRequired = false };
            inVers.SetDefaultValue(2);
            command.AddOption(inVers);

            var sigFile = new Option<string>("--signature", description: "The signature file.") { IsRequired = false };
            command.AddOption(sigFile);

            var outArg = new Option<string>("--out", description: "The output file.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputDirectory, string version, string signatureFile, string outputFile) => SLB2.MakeSLB2File(inputDirectory, int.Parse(version), signatureFile, outputFile), inArg, inVers, sigFile, outArg);
            return command;
        }

        private static Command CreateCommand_ENVDecrypt()
        {
            var command = new Command("envdecrypt", "Decrypts an Envelope file. Huge thanks to SocraticBliss, IDC, Flatz and Zecoxao for implementation, algorithm and idea.");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var encKey = new Option<string>("--key", description: "The encryption key.") { IsRequired = true };
            command.AddOption(encKey);

            var outArg = new Option<string>("--out", description: "The output file.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string encryptionKey, string outputFile) => ENV.DecryptENV(inputFile, encryptionKey, outputFile), inArg, encKey, outArg);
            return command;
        }

        private static Command CreateCommand_ENVEncrypt()
        {
            var command = new Command("envencrypt", "Creates a new Envelope file.");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var contentId = new Option<string>("--content", description: "The content Id.");
            contentId.SetDefaultValue(1);
            command.AddOption(contentId);

            var ivArg = new Option<string>("--iv", description: "The initialization vector.") { IsRequired = true };
            command.AddOption(ivArg);

            var encKey = new Option<string>("--key", description: "The encryption key.") { IsRequired = true };
            command.AddOption(encKey);

            var publicSignFile = new Option<string>("--signature", description: "The public signature file.") { IsRequired = true };
            command.AddOption(publicSignFile);

            var outArg = new Option<string>("--out", description: "The output file.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string contentId, string ivKey, string encryptionKey, string publicSignatureKey, string outputFile) => ENV.EncryptENV(inputFile, int.Parse(contentId), ivKey, encryptionKey, publicSignatureKey, outputFile), inArg, contentId, ivArg, encKey, publicSignFile, outArg);
            return command;
        }

        private static Command CreateCommand_EulaSplit()
        {
            var command = new Command("eulasplit", "Splits the multilanguage eula in files.");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output folder.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outputFolder) => Eula.ReadEulaFile(inputFile, outputFolder), inArg, outArg);
            return command;
        }

        private static Command CreateCommand_SFOInform()
        {
            var command = new Command("sfoinfo", "Prints information in the console about param.sfo. [UNFINISHED]");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            command.SetHandler((string inputFile) => SFO.ViewPSOInformation(inputFile), inArg);
            return command;
        }

        private static Command CreateCommand_RCOExtract()
        {
            var command = new Command("rcoextract", "Extracts a RCO (Resources Container Object File) file. [UNFINISHED]");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output folder.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outpuDirectory) => RCO.ExtractResourceFile(inputFile, outpuDirectory), inArg, outArg);
            return command;
        }

        private static Command CreateCommand_CERTSplitter()
        {
            var command = new Command("certsplit", "Splits the certificate file in files.");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output folder.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outpuDirectory) => Eula.SplitCertificates(inputFile, outpuDirectory), inArg, outArg);
            return command;
        }

        private static Command CreateCommand_VAG2WAV()
        {
            var command = new Command("vag2wav", "Converts VAG to WAV. [UNFINISHED]");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output file.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outputFile) => VAG.Vag2Wav(inputFile, outputFile), inArg, outArg);
            return command;
        }

        private static Command CreateCommand_WAV2VAG()
        {
            var command = new Command("wav2vag", "Converts WAV to VAG. [UNFINISHED]");

            var inArg = new Option<string>("--in", description: "The input file.") { IsRequired = true };
            command.AddOption(inArg);

            var outArg = new Option<string>("--out", description: "The output file.") { IsRequired = false };
            command.AddOption(outArg);

            command.SetHandler((string inputFile, string outputFile) => VAG.Wav2Vag(inputFile, outputFile), inArg, outArg);
            return command;
        }

        private static Argument CreateArgumehnt_WriteLog()
        {
            var argument = new Argument<string>("--log", "Logs the console output in a file")
            {
                IsHidden = true
            };

            return argument;
        }
    }
}