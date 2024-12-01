using SLB2Extractor.IO;
using SLB2Extractor.structs;

namespace SLB2Extractor
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;
        private static readonly int headerSize = 512;
        private static ByteArray byteArray = new ByteArray(headerSize);
        private static Header headerInfo = new Header();

        private static string ToHex(object value)
        {
            return String.Format("0x{0:X}", value);
        }

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

            string[] orders = new string[] { "B", "KB", "MB", "GB"};
            double size = bytes;
            int orderIndex = 0;

            while (size >= 1024 && orderIndex < orders.Length - 1)
            {
                size /= 1024;
                orderIndex++;
            }

            return string.Format("{0:0.##} {1}", size, orders[orderIndex]);
        }

        private static void ExtractSubFile(string inputFile, string fileName2, uint offset, uint length)
        {
            fileName2 = fileName2.Replace("\0", string.Empty);
            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                byte[] buffer = new byte[length];
                int bytesRead = fs.Read(buffer, 0, (int)length);
                if (bytesRead < length)
                {
                    Array.Resize(ref buffer, bytesRead);
                }

                File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), fileName2), buffer);
            }
        }

        private static void extractInfo(String fileName)
        {
            byteArray.WriteBytes(readFileContent(fileName, 0, headerSize));

            headerInfo.magic = byteArray.ReadString(4); // SLB2
            Console.WriteLine($"[+] Magic: {headerInfo.magic}");
            headerInfo.version = byteArray.ReadUInt64(); // 0x0001 or 0x0002
            Console.WriteLine($"[+] Version: {ToHex(headerInfo.version)}");
            headerInfo.fileCount = byteArray.ReadUInt32();
            Console.WriteLine($"[+] Found instances: {ToHex(headerInfo.fileCount)}");
            headerInfo.blockCount = byteArray.ReadUInt32();
            Console.WriteLine($"[+] BlockCount: {ToHex(headerInfo.blockCount)}");
            headerInfo.reserved = byteArray.ReadBytes(12);
            headerInfo.fileEntry = new Entry[4];
            for(int i = 0; i < headerInfo.fileCount; i++)
            {
                headerInfo.fileEntry[i].offset = byteArray.ReadUInt32();
                headerInfo.fileEntry[i].contentSize = byteArray.ReadUInt32();
                headerInfo.fileEntry[i].reserved = byteArray.ReadUInt64();
                headerInfo.fileEntry[i].fileName = byteArray.ReadString(32);
            }
            byteArray.ReadBytes(16 * 18); // empty

            for(int i = 0; i < headerInfo.fileCount; i++)
            {
                ExtractSubFile(fileName, headerInfo.fileEntry[i].fileName, headerInfo.fileEntry[i].offset * 512, headerInfo.fileEntry[i].contentSize);
                Console.WriteLine($"[Sector {i + 1}] File -> {headerInfo.fileEntry[i].fileName} | Size: {getFormatFromBytes(headerInfo.fileEntry[i].contentSize)}");
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Console.WriteLine("Extracts the contents of a SLB2 file.");
                Console.WriteLine($"Usage: {appName} [PS4UPDATE.PUP]");
                return;
            }
            string fileName = args[0];
            extractInfo(fileName);
        }
    }
}