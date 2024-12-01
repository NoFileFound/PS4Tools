using ENVEncrypt.IO;
using System.Security.Cryptography;

// PS4 ENV File Encryptor.

/**
 * PS4 ENV File Decryptor by SocraticBliss(R)
 * 
 * Special Thanks to IDC for finding the Buffers/IV/Flag and implementation suggestions
 * Huge thanks to Flatz for the proper decryption technique
 * 
 * ... Oh and I guess Zecoxao as well
*/

namespace ENVEncrypt
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;
        private static ByteArray byteArray = new ByteArray(336);
        private static int contentId;
        // Replace the 0's with the actual keys!
        private static readonly Dictionary<int, string> KEYS = new Dictionary<int, string>{
            { 0x1, "00000000000000000000000000000000" }, // beta_updatelist
            { 0x2, "00000000000000000000000000000000" }, // timezone
            { 0x3, "00000000000000000000000000000000" }, // system_log_config
            { 0x4, "00000000000000000000000000000000" }, // system_log_unknown
            { 0x5, "00000000000000000000000000000000" }, // bgdc_config
            { 0x6, "00000000000000000000000000000000" }, // wctl_config
            { 0x7, "00000000000000000000000000000000" }, // morpheus_updatelist
            { 0x8, "00000000000000000000000000000000" }, // netev_config
            { 0x9, "00000000000000000000000000000000" }, // gls_config
            { 0xA, "00000000000000000000000000000000" }, // hid_config
            { 0xC, "00000000000000000000000000000000" }, // hidusbpower
            { 0xD, "00000000000000000000000000000000" }, // patch_hmac_key
            { 0xE, "00000000000000000000000000000000" }, // bgft
            { 0x11, "00000000000000000000000000000000" }, // system_log_privacy
            { 0x12, "00000000000000000000000000000000" }, // webbrowser_xutil
            { 0x13, "00000000000000000000000000000000" }, // entitlementmgr_config
            { 0x15, "00000000000000000000000000000000" }, // jsnex_netflixdeckeys
            { 0x16, "00000000000000000000000000000000" }  // party_config
        };

        private static long AlignDown(long x, int alignment)
        {
            return x & ~(alignment - 1);
        }

        private static byte[] XorString(byte[] key, byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        private static byte[] AesEncryptCbcCts(byte[] key, byte[] iv, byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                int blockSize = aes.BlockSize / 8; 
                long sizeAligned = AlignDown(data.Length, blockSize);
                byte[] result;

                using (var encryptor = aes.CreateEncryptor())
                {
                    result = encryptor.TransformFinalBlock(data, 0, (int)sizeAligned);
                }

                int sizeLeft = data.Length - (int)sizeAligned;
                if (sizeLeft > 0)
                {
                    if (sizeLeft >= blockSize)
                        throw new Exception("Invalid CTS size."); 

                    byte[] tail = new byte[sizeLeft];
                    Array.Copy(data, sizeAligned, tail, 0, sizeLeft);

                    aes.Mode = CipherMode.ECB;
                    byte[] tmp;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        if (sizeAligned > 0)
                        {
                            tmp = encryptor.TransformFinalBlock(data, (int)sizeAligned - blockSize, blockSize);
                        }
                        else
                        {
                            tmp = encryptor.TransformFinalBlock(iv, 0, blockSize);
                        }
                    }

                    byte[] xorTail = XorString(tmp.Take(sizeLeft).ToArray(), tail);
                    result = result.Concat(xorTail).ToArray();
                }

                return result;
            }
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

        private static void makeHeader(String[] args)
        {
            contentId = int.Parse(args[1]);
            byte[] iv = readFileContent(args[2], 0, 16);

            byteArray.WriteInt32(0x5173CBCC);
            byteArray.WriteInt32(0);
            byteArray.WriteInt32(contentId);
            byteArray.WriteInt32(0);
            byteArray.WriteInt64(new FileInfo(args[0]).Length);
            byteArray.WriteInt64(0);
            byteArray.WriteBytes(iv); // iv
            using (FileStream fs = File.OpenRead(args[0]))
            using (SHA256 sha256 = SHA256.Create())
            {
                byteArray.WriteBytes(sha256.ComputeHash(fs)); // sha256
            }
            byteArray.WriteBytes(readFileContent(args[3], 0, 256)); // signature, public key

            using (FileStream fs = new FileStream("output.bin", FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(byteArray.ToArray(), 0, byteArray.Length());
            }

            byte[] fileData = File.ReadAllBytes(args[0]);
            if (!KEYS.TryGetValue(contentId, out string keyString))
            {
                Console.WriteLine("Error: Invalid Key ID!");
                return;
            }
            byte[] key = Convert.FromHexString(keyString.Replace(" ", ""));
            byte[] encryptedData = AesEncryptCbcCts(key, iv, fileData);

            using (FileStream fs = new FileStream("output.bin", FileMode.Append, FileAccess.Write))
            {
                fs.Write(encryptedData);
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length < 4)
            {
                Console.WriteLine("Encrypts an ENV file.");
                Console.WriteLine($"Usage: {appName} [.ENV] [ContentId] [IV] [Signature]");
                return;
            }
            makeHeader(args);
        }
    }
}