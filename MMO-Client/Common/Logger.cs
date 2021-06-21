using System;

namespace MMO_Client.Common.Logger
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
