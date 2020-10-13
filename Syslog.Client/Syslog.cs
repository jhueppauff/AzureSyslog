using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Syslog.Client
{
    public class Syslog : IDisposable
    {
        private const int Facility = 1;  // user-level messages    private const int FacilityFactor = 8;
        private readonly UdpClient udpClient;

        public Syslog(string hostname = "localhost", int port = 514) { this.udpClient = new UdpClient(hostname, port); }
        public void Send(Severity severity, string message, [CallerMemberName] string caller = null)
        {
            byte[] data = Encoding.UTF8.GetBytes(String.Format("<{0}>{1} {2} {3}", ((int)severity), DateTime.Now.ToString("MMM dd HH:mm:ss"), Dns.GetHostName(), ((!String.IsNullOrWhiteSpace(caller)) ? (caller + " ") : (String.Empty)) + message));
            this.udpClient.Send(data, data.Length);
        }
        public void Dispose() => this.udpClient?.Dispose();

        public enum Severity
        {
            Emergency,     // [0] system is unusable
            Alert,         // [1] action must be taken immediately
            Critical,      // [2] critical conditions
            Error,         // [3] error conditions
            Warning,       // [4] warning conditions
            Notice,        // [5] normal but significant condition
            Informational, // [6] informational messages        
            Debug,         // [7] debug-level messages    
        }  // https://tools.ietf.org/html/rfc3164}
    }
}
