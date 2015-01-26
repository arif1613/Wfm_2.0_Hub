using System.Threading;
using NLog;
using NLog.Targets;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace CommonInfrastructureLibrary.Logging
{
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout 
    {
        public SyslogTarget()
        {
            SyslogServer = "127.0.0.1";
            Port = 514;
            Sender = Assembly.GetCallingAssembly().GetName().Name;
            Facility = Syslog.SyslogFacility.Local1;
            Protocol = ProtocolType.Udp;
            Machine = Dns.GetHostName();
        }

        public string Machine { get; set; }
        public string SyslogServer { get; set; }
        public int Port { get; set; }
        public string Sender { get; set; }
        public Syslog.SyslogFacility Facility { get; set; }
        public String CustomPrefix { get; set; }
        public ProtocolType Protocol { get; set; }
        public bool Ssl { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var formattedLines = Layout.Render(logEvent)
                                       .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var fline in formattedLines)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                byte[] msg = BuildSyslogMessage(Facility, GetSyslogSeverity(logEvent.Level), DateTime.Now, Sender, fline);
                SendMessage(SyslogServer, Port, msg, Protocol, Ssl);
            }
        }

        private static void SendMessage(string logServer, int port, byte[] msg, ProtocolType protocol,
                                        bool useSsl = false)
        {
            var logServerIp = Dns.GetHostAddresses(logServer).FirstOrDefault();

            if (logServerIp == null)
            {
                return;
            }

            var ipAddress = logServerIp.ToString();

            switch (protocol)
            {
                case ProtocolType.Udp:
                    var udp = new UdpClient(ipAddress, port);
                    udp.Send(msg, msg.Length);
                    udp.Close();
                    break;
                case ProtocolType.Tcp:
                    var tcp = new TcpClient(ipAddress, port);
                    Stream stream = tcp.GetStream();
                    if (useSsl)
                    {
                        var sslStream = new SslStream(tcp.GetStream());
                        sslStream.AuthenticateAsClient(logServer);
                        stream = sslStream;
                    }
                    else
                    {
                        stream = tcp.GetStream();
                    }

                    stream.Write(msg, 0, msg.Length);

                    stream.Close();
                    tcp.Close();
                    break;
                default:
                    throw new NLogConfigurationException(string.Format("Protocol '{0}' is not supported.", protocol));
            }
        }

        private static Syslog.SyslogSeverity GetSyslogSeverity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Fatal)
            {
                return Syslog.SyslogSeverity.Emergency;
            }

            if (logLevel >= LogLevel.Error)
            {
                return Syslog.SyslogSeverity.Error;
            }

            if (logLevel >= LogLevel.Warn)
            {
                return Syslog.SyslogSeverity.Warning;
            }

            if (logLevel >= LogLevel.Info)
            {
                return Syslog.SyslogSeverity.Informational;
            }

            if (logLevel >= LogLevel.Debug)
            {
                return Syslog.SyslogSeverity.Debug;
            }

            if (logLevel >= LogLevel.Trace)
            {
                return Syslog.SyslogSeverity.Notice;
            }

            return Syslog.SyslogSeverity.Notice;
        }

        private byte[] BuildSyslogMessage(Syslog.SyslogFacility facility, Syslog.SyslogSeverity priority, DateTime time,
                                          string sender,
                                          string body)
        {
            var machine = Machine + " ";
            var calculatedPriority = (int) facility*8 + (int) priority;
            var pri = "<" + calculatedPriority.ToString(CultureInfo.InvariantCulture) + ">";

            var timeToString = time.ToString("MMM dd HH:mm:ss ");
            sender = sender + ": ";

            if (!String.IsNullOrEmpty(CustomPrefix))
            {
                body = String.Format("[{0}] {1}", CustomPrefix, body);
            }

            string[] strParams = {pri, timeToString, machine, sender, body, Environment.NewLine};

            return Encoding.ASCII.GetBytes(string.Concat(strParams));
        }
    }
}