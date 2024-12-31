using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4UpdateTools.sys
{
    internal class Logger
    {
        private static bool _WriteToFile = false;

        public static void LogMsg(string message)
        {
            if (_WriteToFile)
            {
                File.AppendAllText("logger.log", message + Environment.NewLine);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        public static void setWriteToFile(bool writeToFile)
        {
            _WriteToFile = writeToFile;
        }
    }
}