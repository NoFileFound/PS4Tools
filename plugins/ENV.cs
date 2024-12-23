using System;
using System.Security.Cryptography;

namespace PS4UpdateTools.plugins
{
    internal class ENV
    {
        public static void DecryptENV(string encryptedFile, string encryptionKey, string outputFile)
        {
            if (outputFile == null) 
                outputFile = "decrypted.dat";

            byte[] fileData = File.ReadAllBytes(encryptedFile);
            byte[] key = Convert.FromHexString(encryptionKey.Replace(" ", ""));
            long size = BitConverter.ToInt64(fileData, 0x10);
            byte[] iv = fileData.Skip(0x20).Take(0x10).ToArray();
            byte[] message = fileData.Skip(0x150).Take((int)size).ToArray();
            byte[] decrypted;

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                int blockSize = aes.BlockSize / 8; // Block size in bytes
                long sizeAligned = AlignDown(message.Length, blockSize);

                // Decrypt aligned blocks
                using (var decryptor = aes.CreateDecryptor())
                {
                    decrypted = decryptor.TransformFinalBlock(message, 0, (int)sizeAligned);
                }

                int sizeLeft = message.Length - (int)sizeAligned;
                if (sizeLeft > 0)
                {
                    if (sizeLeft >= blockSize) throw new Exception("Invalid CTS size.");

                    aes.Mode = CipherMode.ECB;

                    byte[] tmp;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        if (sizeAligned > blockSize)
                        {
                            tmp = encryptor.TransformFinalBlock(message, (int)sizeAligned - blockSize, blockSize);
                        }
                        else
                        {
                            tmp = encryptor.TransformFinalBlock(iv, 0, blockSize);
                        }
                    }

                    byte[] tail = new byte[sizeLeft];
                    Array.Copy(message, sizeAligned, tail, 0, sizeLeft);
                    decrypted = decrypted.Concat(xor(tmp.Take(sizeLeft).ToArray(), tail)).ToArray();
                }
            }
            File.WriteAllBytes(outputFile, decrypted);
        }

        public static void EncryptENV(string inputFile, int contentId, string IVkey, string encryptionKey, string publicSignatureFile, string outputFile)
        {
            if (string.IsNullOrEmpty(outputFile))
                outputFile = "encrypted.dat";

            byte[] IV = Convert.FromHexString(IVkey.Replace(" ", ""));
            byte[] key = Convert.FromHexString(encryptionKey.Replace(" ", ""));
            byte[] sha256Hash;
            byte[] fileData = File.ReadAllBytes(inputFile);

            using (var sha256 = SHA256.Create())
            {
                using (var fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                {
                    sha256Hash = sha256.ComputeHash(fs);
                }
            }

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0x5173CBCC); // signature
                writer.Write(0);          // reserved
                writer.Write(contentId);  // content ID
                writer.Write(0);          // reserved
                writer.Write(new FileInfo(inputFile).Length); // file size
                writer.Write(0L);         // reserved
                writer.Write(IV);         // IV
                writer.Write(sha256Hash); // SHA256 hash
                writer.Write(File.ReadAllBytes(publicSignatureFile)); // Public signature

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.None;

                    int blockSize = aes.BlockSize / 8;
                    long sizeAligned = AlignDown(fileData.Length, blockSize);
                    byte[] encrypted;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        encrypted = encryptor.TransformFinalBlock(fileData, 0, (int)sizeAligned);
                    }

                    int sizeLeft = fileData.Length - (int)sizeAligned;
                    if (sizeLeft > 0)
                    {
                        if (sizeLeft >= blockSize) throw new Exception("Invalid CTS size.");

                        aes.Mode = CipherMode.ECB;

                        byte[] tmp;
                        using (var encryptor = aes.CreateEncryptor())
                        {
                            if (sizeAligned > blockSize)
                            {
                                tmp = encryptor.TransformFinalBlock(encrypted, (int)sizeAligned - blockSize, blockSize);
                            }
                            else
                            {
                                tmp = encryptor.TransformFinalBlock(IV, 0, blockSize);
                            }
                        }

                        byte[] tail = new byte[sizeLeft];
                        Array.Copy(fileData, sizeAligned, tail, 0, sizeLeft);
                        byte[] lastBlock = xor(tmp.Take(sizeLeft).ToArray(), tail);
                        encrypted = encrypted.Concat(lastBlock).ToArray();
                    }
                    writer.Write(encrypted);
                }
                File.WriteAllBytes(outputFile, stream.ToArray());
            }
        }

        private static long AlignDown(long x, int alignment)
        {
            return x & ~(alignment - 1);
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
    }
}