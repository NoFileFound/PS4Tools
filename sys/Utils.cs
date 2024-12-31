using System.Collections;

namespace PS4UpdateTools.sys
{
    internal class Utils
    {
        public static string ToHex(object value)
        {
            return String.Format("0x{0:X}", value);
        }

        public static string GetFormatFromBytes(long bytes)
        {
            if (bytes == 0)
                return "0 B";

            string[] orders = ["B", "KB", "MB", "GB"];
            double size = bytes;
            int orderIndex = 0;

            while (size >= 1024 && orderIndex < orders.Length - 1)
            {
                size /= 1024;
                orderIndex++;
            }

            return string.Format("{0:0.##} {1}", size, orders[orderIndex]);
        }

        public static string GetHexArrayString(byte[] bytes)
        {
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }
    }
}