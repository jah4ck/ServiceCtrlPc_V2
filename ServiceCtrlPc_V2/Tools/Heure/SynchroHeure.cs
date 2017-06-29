using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServiceCtrlPc_V2.Tools.Heure
{
    public class SynchroHeure
    {
        public static DateTime GetNetworkTime()
        {
            try
            {
                const string ntpServer = "time.windows.com";
                var ntpData = new byte[48];
                ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

                var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.ReceiveTimeout = 30000;
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);

                socket.Close();

                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);


                networkDateTime = networkDateTime.ToLocalTime();
                return networkDateTime;
            }
            catch (Exception _Exception)
            {
                try
                {
                    Scheduler.Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                }
                catch (Exception)
                {
                }
                return DateTime.Now;
            }
            
        }
    }
}
