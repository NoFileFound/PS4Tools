using SLB2Packer.IO;

namespace SLB2Packer
{
    public class Program
    {
        private static uint version;
        private static uint fileCount;
        private static uint blockCount;
        private static string? signFile;
        private static bool useSignature;
        private static List<string> fileNames = new List<string>();
        private static List<uint> fileSizes = new List<uint>();
        private static string appName = AppDomain.CurrentDomain.FriendlyName;
        private static ByteArray byteArray = new ByteArray(512 * 2);

        private static byte[] readFileContent(String fileName, int startPos, int length)
        {
            byte[] buffer = new byte[length];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = fs.Read(buffer, startPos, length);
            }
            return buffer;
        }

        private static string getFormatFromBytes(long bytes)
        {
            if (bytes == 0)
                return "0 B";

            string[] orders = new string[] { "B", "KB", "MB", "GB" };
            double size = bytes;
            int orderIndex = 0;

            while (size >= 1024 && orderIndex < orders.Length - 1)
            {
                size /= 1024;
                orderIndex++;
            }

            return string.Format("{0:0.##} {1}", size, orders[orderIndex]);
        }

        private static void consoleRead(string[] args)
        {
            // slb2 version.
            Console.Write("Version: (1 - OLD, 2 - NEW, Default: 2) ");
            if (!uint.TryParse(Console.ReadLine(), out version) || version > 2) version = 2;

            // signature file (> 7version)
            Console.Write("Signature filename: (Default: empty) ");
            signFile = Console.ReadLine();

            useSignature = (signFile != null && signFile.Length > 0);

            // file count
            fileCount = (uint)args.Length; // 0x2 for release and 0x4 for devkit
            
            // block count
            blockCount = (uint)(useSignature ? 2 : 1);
            for (int i = 0; i < fileCount; i++)
            {
                FileInfo fileInfo = new FileInfo(args[i]);
                uint size = (uint)fileInfo.Length;
                uint padding = (512 - (size % 512)) % 512;

                blockCount += ((size + padding) / 512);
                fileSizes.Add(size);
                fileNames.Add(args[i]);
            }
        }

        private static void makeHeader()
        {
            uint currentOffset;
            byteArray.WriteString("SLB2");
            byteArray.WriteUInt32(version); // 4 bytes
            byteArray.WriteUInt32(0); // flags (4 bytes)
            byteArray.WriteUInt32(fileCount); // 4 bytes
            byteArray.WriteUInt32(blockCount);
            byteArray.WriteBytes(new byte[12]); // reserved
            currentOffset = (uint)(useSignature ? 2 : 1);

            for (int i = 0; i < fileCount; i++)
            {
                uint currentFileSize = fileSizes[i];
                byteArray.WriteUInt32(currentOffset);
                byteArray.WriteUInt32(currentFileSize);
                byteArray.WriteBytes(new byte[8]); // reserved
                byteArray.WriteString(fileNames[i]);
                byteArray.WriteBytes(new byte[32 - fileNames[i].Length]);

                uint padding = (512 - (currentFileSize % 512)) % 512;
                currentOffset += ((currentFileSize + padding) / 512);
            }
            byteArray.WriteBytes(new byte[512 - byteArray.Length()]);
            if(useSignature)
            {
                byteArray.WriteBytes(readFileContent(signFile, 0, 512));
            }
        }

        private static void saveHeader()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(currentDirectory, "output.bin");
            File.WriteAllBytes(fullPath, byteArray.ToArray());
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Creates a new SLB2 file.");
                Console.WriteLine($"Usage: {appName} [PS4UPDATE1.PUP] ... [PS4UPDATE4.PUP]");
                return;
            }
            consoleRead(args);
            makeHeader();
            saveHeader();

            for(int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"[Sector {i + 1}] File -> {args[i]} | Size: {getFormatFromBytes(fileSizes[i])}");
                byte[] buffer = new byte[4096];
                long totalBytesRead = 0;
                using (FileStream inputStream = new FileStream(args[i], FileMode.Open, FileAccess.Read))
                {
                    using (FileStream outputStream = new FileStream("output.bin", FileMode.Append, FileAccess.Write))
                    {
                        int bytesRead;
                        while ((bytesRead = inputStream.Read(buffer, 0, 4096)) > 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }

                        int paddingSize = (int)((512 - (totalBytesRead % 512)) % 512);
                        if (paddingSize > 0)
                        {
                            byte[] padding = new byte[paddingSize];
                            outputStream.Write(padding, 0, padding.Length);
                        }
                    }
                }
            }
        }
    }
}