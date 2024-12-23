using PS4UpdateTools.sys;
using System;
using System.Collections;

namespace PS4UpdateTools.plugins
{
    internal class Extractor
    {
        public static bool ExtractEntry(string inputFile, string outputDirectory)
        {
            outputDirectory ??= Directory.GetCurrentDirectory();
            if(!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                string magic = new(reader.ReadChars(4)); // magic
                Console.WriteLine($"[+] Magic: {magic}");

                ulong version = reader.ReadUInt64(); // version
                Console.WriteLine($"[+] Version: {Utils.ToHex(version)}");

                uint fileCount = reader.ReadUInt32(); // total files
                Console.WriteLine($"[+] FileCount: {Utils.ToHex(fileCount)}");

                uint blockCount = reader.ReadUInt32(); // total blocks
                Console.WriteLine($"[+] BlockCount: {Utils.ToHex(blockCount)}");

                reader.ReadBytes(12); // reserved

                var fileEntries = new Entry[fileCount];
                for (int i = 0; i < fileCount; i++)
                {
                    var entry = new Entry
                    {
                        offset = reader.ReadUInt32(),
                        contentSize = reader.ReadUInt32(),
                        reserved = reader.ReadUInt64(),
                        fileName = new string(reader.ReadChars(32)).Trim('\0')
                    };
                    fileEntries[i] = entry;
                }

                reader.ReadBytes(16 * 18); // reserved

                for (int i = 0; i < fileCount; i++)
                {
                    fs.Seek(fileEntries[i].offset * 512, SeekOrigin.Begin);

                    byte[] buffer = new byte[fileEntries[i].contentSize];
                    int bytesRead = fs.Read(buffer, 0, (int)fileEntries[i].contentSize);
                    if (bytesRead < fileEntries[i].contentSize)
                    {
                        Array.Resize(ref buffer, bytesRead);
                    }

                    File.WriteAllBytes(Path.Combine(outputDirectory, fileEntries[i].fileName), buffer);
                    Console.WriteLine($"[{i + 1}] File -> {fileEntries[i].fileName} | Sz -> {Utils.GetFormatFromBytes(fileEntries[i].contentSize)}");
                }
            }
            return true;
        }

        public static bool MakeSLB2File(string inputDirectory, int version, string signatureFile, string outputFile)
        {
            if (outputFile == null)
                outputFile = "out.slb2";

            uint currentOffset = (uint)(signatureFile != null ? 2 : 1);
            string[] files = Directory.GetFiles(inputDirectory);
            uint fileCount = (uint)files.Length;
            uint blockCount = 1; // header
            List<string> fileNames = new List<string>();
            List<uint> fileSizes = new List<uint>();

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                uint size = (uint)fileInfo.Length;
                uint padding = (512 - (size % 512)) % 512;

                blockCount += ((size + padding) / 512);
                fileSizes.Add(size);
                fileNames.Add(Path.GetFileName(file));
            }

            if(signatureFile != null)
            {
                blockCount++;
            }

            using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write("SLB2".ToCharArray());  // Magic
                writer.Write(version);  // Version
                writer.Write(0);  // Flags
                writer.Write(fileCount);  // Total files
                writer.Write(blockCount);  // Block count
                writer.Write(new byte[12]);  // Reserved

                for (int i = 0; i < fileCount; i++)
                {
                    uint currentFileSize = fileSizes[i];
                    writer.Write(currentOffset);  // Offset
                    writer.Write(currentFileSize);  // Size
                    writer.Write(new byte[8]);  // Reserved
                    writer.Write(fileNames[i].ToCharArray());  // File name
                    writer.Write(new byte[32 - fileNames[i].Length]);

                    uint padding = (512 - (currentFileSize % 512)) % 512;
                    currentOffset += ((currentFileSize + padding) / 512);
                }

                writer.Write(new byte[512 - writer.BaseStream.Position % 512]);

                if (signatureFile != null)
                {
                    byte[] signatureData = File.ReadAllBytes(signatureFile);
                    writer.Write(signatureData, 0, Math.Min(signatureData.Length, 512));
                }

                foreach (var fileName in fileNames)
                {
                    string fullPath = Path.Combine(inputDirectory, fileName);
                    using (FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(writer.BaseStream);

                        int paddingSize = (int)((512 - (fileSizes[fileNames.IndexOf(fileName)] % 512)) % 512);
                        if (paddingSize > 0)
                        {
                            writer.Write(new byte[paddingSize]);
                        }
                    }
                }
            }
            return true;
        }

        private class Entry
        {
            public uint offset;
            public uint contentSize;
            public ulong reserved;
            public required string fileName;
        }
    }
}