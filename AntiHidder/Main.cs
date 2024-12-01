namespace AntiHidder
{
    public class Program
    {
        private static string appName = AppDomain.CurrentDomain.FriendlyName;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Clear system and hidden flag of every file in given directory.");
                Console.WriteLine($"Usage: {appName} [DIR]");
                return;
            }
            string dir = args[0];

            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Not exist");
                return;
            }

            string[] files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                FileAttributes attributes = File.GetAttributes(file);

                if((attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||(attributes & FileAttributes.System) == FileAttributes.System)
                {
                    attributes &= ~FileAttributes.Hidden;
                    attributes &= ~FileAttributes.System;
                    File.SetAttributes(file, attributes);
                }
            }
        }
    }
}