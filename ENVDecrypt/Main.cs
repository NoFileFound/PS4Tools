using System.Security.Cryptography;

/**
 * PS4 ENV File Decryptor by SocraticBliss(R)
 * 
 * Special Thanks to IDC for finding the Buffers/IV/Flag and implementation suggestions
 * Huge thanks to Flatz for the proper decryption technique
 * 
 * ... Oh and I guess Zecoxao as well
*/

namespace ENVDecrypt
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;
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
            return x & ~(alignment -1);
        }

        private static byte[] xor(byte[] key, byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        private static byte[] AesDecryptCbcCts(byte[] key, byte[] iv, byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                int blockSize = aes.BlockSize / 8; // Block size in bytes
                long sizeAligned = AlignDown(data.Length, blockSize);
                byte[] result;

                // Decrypt aligned blocks
                using (var decryptor = aes.CreateDecryptor())
                {
                    result = decryptor.TransformFinalBlock(data, 0, (int)sizeAligned);
                }

                int sizeLeft = data.Length - (int)sizeAligned;

                if (sizeLeft > 0)
                {
                    if (sizeLeft >= blockSize) throw new Exception("Invalid CTS size.");

                    aes.Mode = CipherMode.ECB;

                    byte[] tmp;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        if (sizeAligned > blockSize)
                        {
                            tmp = encryptor.TransformFinalBlock(data, (int)sizeAligned - blockSize, blockSize);
                        }
                        else
                        {
                            tmp = encryptor.TransformFinalBlock(iv, 0, blockSize);
                        }
                    }

                    byte[] tail = new byte[sizeLeft];
                    Array.Copy(data, sizeAligned, tail, 0, sizeLeft);
                    result = result.Concat(xor(tmp.Take(sizeLeft).ToArray(), tail)).ToArray();
                }

                return result;
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Decrypts an ENV file.");
                Console.WriteLine($"Usage: {appName} [.ENV]");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = inputFilePath + ".dec";

            byte[] fileData = File.ReadAllBytes(inputFilePath);

            int id = BitConverter.ToInt32(fileData, 0x8);
            if (!KEYS.TryGetValue(id, out var keyString))
            {
                Console.WriteLine("Error: Invalid File!");
                return;
            }

            byte[] key = Convert.FromHexString(keyString.Replace(" ", ""));
            long size = BitConverter.ToInt64(fileData, 0x10);
            byte[] iv = fileData.Skip(0x20).Take(0x10).ToArray();
            byte[] message = fileData.Skip(0x150).Take((int)size).ToArray();

            byte[] decryptedData = AesDecryptCbcCts(key, iv, message);

            File.WriteAllBytes(outputFilePath, decryptedData);
            Console.WriteLine("Success!");
        }
    }
}