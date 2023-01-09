using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using TorrserverTray.Properties;
using Application = System.Windows.Forms.Application;
using TorrServerLib;
using System.ServiceProcess;
using System.Security.Principal;

namespace TorrWinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MyCustomApplicationContext());
        }
    }


    public class MyCustomApplicationContext : ApplicationContext
    {

        string localIP;
        ServiceController torrserv = null;
        private NotifyIcon trayIcon;

        public MyCustomApplicationContext()
        {
            using (Process myProcess = new Process())
            {

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                    socket.Close();
                }
                if (localIP is null)
                {
                    localIP = ApiTorrServer.GetLocalIp();
                }
            }
            ServiceController[] scServices = ServiceController.GetServices();
            //Ставим счетчик на кол-во найденных служб, мы зарание знаем их название и предполагаемое кол-во

            foreach (ServiceController scTemp in scServices)
            {
                switch (scTemp.ServiceName)
                {
                    case "TorrServerService":
                        torrserv = scTemp;
                        break;
                    default:
                        break;
                }
            }


            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadLine();

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.ts_round,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Рекомендованный IP " + localIP + ":8090"),
                    new MenuItem("-"),
                    new MenuItem("Другие возможные IP "),
                    new MenuItem("-"),
                    new MenuItem("Запустить", Start),
                    new MenuItem("Остановить", Stop),
                    new MenuItem("Закрыть", Exit)
                }),
                Visible = true
            };
            if (torrserv != null)
            {
                if (torrserv.Status == ServiceControllerStatus.Running)
                {
                    trayIcon.Text = "Служба работает";
                    trayIcon.ContextMenu.MenuItems[4].Enabled = false;
                }
                else if (torrserv.Status == ServiceControllerStatus.Stopped)
                {
                    trayIcon.Text = "Служба остановлена";
                    trayIcon.ContextMenu.MenuItems[5].Enabled = false;
                }
            }
            var otherIps = ApiTorrServer.GetIpsSting();
            if (otherIps.Length > 0)
            {
                var otherIpsMenu = trayIcon.ContextMenu.MenuItems[2];
                foreach (var item in otherIps)
                {
                    if (item != localIP)
                    {
                        otherIpsMenu.MenuItems.Add(item + ":8090");
                    }
                }
                if (otherIpsMenu.MenuItems.Count == 0)
                {
                    trayIcon.ContextMenu.MenuItems[2].Visible = false;
                    trayIcon.ContextMenu.MenuItems[3].Visible = false;
                }
            }

            if (!IsUserAdministrator())
            {
                trayIcon.ContextMenu.MenuItems[3].Visible = false;
                trayIcon.ContextMenu.MenuItems[4].Visible = false;
                trayIcon.ContextMenu.MenuItems[5].Visible = false;
            }


        }

        private void Stop(object sender, EventArgs e)
        {
            //try
            //{
            //    var firewallRule = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            //    foreach (INetFwRule rule in firewallRule.Rules)
            //    {
            //        if (rule.Name == "torrserver.exe")
            //        {
            //            var ff = rule;
            //        }
            //    }
            //    if (Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) is INetFwPolicy2 firewallPolicy)
            //    {
            //        var d = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            //        INetFwRule ServicefirewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")) as INetFwRule;
            //        ServicefirewallRule.Grouping = "TorrServer";
            //        ServicefirewallRule.ApplicationName= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)+ "\\TorrServer.exe";
            //        ServicefirewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            //        ServicefirewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            //        ServicefirewallRule.Enabled = true;
            //        ServicefirewallRule.InterfaceTypes = "All";
            //        ServicefirewallRule.Name = "TorrServer";
            //        ServicefirewallRule.Description = "Правило для входящих соединений TorrServer";
            //        ServicefirewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
            //        ServicefirewallRule.LocalPorts = "";
            //        firewallPolicy.Rules.Add(ServicefirewallRule);
            //    }
            //}
            //catch
            //{
            //}
            try
            {
                torrserv.Stop();
                //ждем остановку службы
                torrserv.WaitForStatus(ServiceControllerStatus.Stopped);
                trayIcon.Text = "служба остановлена";
                trayIcon.ContextMenu.MenuItems[5].Enabled = false;
                trayIcon.ContextMenu.MenuItems[4].Enabled = true;
            }
            catch (Exception ex)
            {
                trayIcon.Text = ex.Message;
            }
        }

        private void Start(object sender, EventArgs e)
        {
            try
            {
                torrserv.Start();
                //Ждем остановку службы
                torrserv.WaitForStatus(ServiceControllerStatus.Running);
                trayIcon.Text = "Служба работает";
                trayIcon.ContextMenu.MenuItems[4].Enabled = false;
                trayIcon.ContextMenu.MenuItems[5].Enabled = true;
            }
            catch (Exception ex)
            {
                trayIcon.Text = ex.Message;
            }
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }


        public bool IsUserAdministrator()
        {
            bool isAdmin = false;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);//Is Admin
            }
            //Is not admin
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }

            return isAdmin;
        }
    }
}
