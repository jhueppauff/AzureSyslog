using System;
using System.Net;
using System.Security.Cryptography;

namespace Syslog.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Syslog-Client... [Destination];[Severity];[Message]");
            while (true)
            {
                string messageline = Console.ReadLine();
                string[] msg = messageline.Split(";");
                Syslog syslog = new Syslog(msg[0]);
                syslog.Send((Syslog.Severity)Enum.Parse(typeof(Syslog.Severity), msg[1]), msg[2]);
                syslog.Dispose();
            }
        }
    }
}
