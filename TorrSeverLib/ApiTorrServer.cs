using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TorrServerLib
{
    public static class ApiTorrServer
    {
        public static string GetLocalIp()
        {
            var ip = "";
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var gate = ni.GetIPProperties().GatewayAddresses;
                    var defgate = FindDefaultGateway();
                    foreach (var item in gate)
                    {
                        if (defgate.Equals(item.Address))
                        {
                            foreach (UnicastIPAddressInformation y in ni.GetIPProperties().UnicastAddresses)
                            {
                                if (y.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    foreach (var itemip in localIPs)
                                    {
                                        if (itemip.Equals(y.Address))
                                        {
                                            ip = y.Address.ToString();
                                            Console.WriteLine(ip);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ip;
        }

        public static string[] GetIpsSting()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            var countIpv4 = 0;
            foreach (IPAddress item in localIPs)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    countIpv4++;
                }
            }
            string[] ips = new string[countIpv4];
            foreach (IPAddress item in localIPs)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    for (int i = 0; i < ips.Length; i++)
                    {
                        if (ips[i] is null)
                        { ips[i] = item.ToString();
                            break;
                        }
                    }
                }
            }
            return ips;
        }
        //https://stackoverflow.com/a/62123858
        private static IPAddress FindDefaultGateway(IPAddress netaddr = null)
        {
            // user can provide an ip address that exists on the network they want to connect to, 
            // or this routine will default to 1.1.1.1 (IP of a popular internet dns provider)
            if (netaddr is null)
                netaddr = IPAddress.Parse("1.1.1.1");

            PingReply reply;
            var ping = new Ping();
            var options = new PingOptions(1, true); // ttl=1, dont fragment=true
            try
            {
                // I arbitrarily used a 200ms timeout; tune as you see fit.
                reply = ping.Send(netaddr, 200, new byte[0], options);
            }
            catch (PingException)
            {
                System.Diagnostics.Debug.WriteLine("Gateway not available");
                return default;
            }
            if (reply.Status != IPStatus.TtlExpired)
            {
                System.Diagnostics.Debug.WriteLine("Gateway not available");
                return default;
            }
            return reply.Address;
        }
    }
}
