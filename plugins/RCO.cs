using PS4UpdateTools.sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PS4UpdateTools.plugins
{
    internal class RCO
    {
        public static void ExtractResourceFile(string inputFile, string outputDirectory)
        {
            outputDirectory ??= Directory.GetCurrentDirectory();
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                string magic = new(reader.ReadChars(4)); // magic
                Logger.LogMsg($"[+] Magic: {magic}");

                uint version = reader.ReadUInt32(); // version
                Logger.LogMsg($"[+] Version: {Utils.ToHex(version)}");

                uint offset = reader.ReadUInt32();
                Logger.LogMsg($"[+] Offset: {Utils.ToHex(offset)}");

                uint size = reader.ReadUInt32();
                Logger.LogMsg($"[+] Size: {Utils.ToHex(size)}");

                byte[] signature = reader.ReadBytes(2);
                Logger.LogMsg($"[+] File signature: {Utils.ToHex(signature)}");
                if (signature[0] == 0x78 && (signature[1] == 0x01 || signature[1] == 0x5E || signature[1] == 0x9C || signature[1] == 0xDA))
                {
                    /// zlib compression header
                    Logger.LogMsg("[-] Not a valid PS4 RCO file.");
                    return;
                }


            }

            throw new NotImplementedException();
        }
    }
}
