using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO_Client.Common
{
    public class Logger
    {
        public static void Debug(string data)
        {
            string Timestamp = DateTime.Now.ToString("h:mm:ss");
            string LogStr = String.Format("{0} [DEBUG] {1}", Timestamp, data);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(LogStr);
            Console.ForegroundColor = ConsoleColor.Gray; //Default?
        }
    }
}
