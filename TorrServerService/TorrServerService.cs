using NetFwTypeLib;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace TorrServerService
{
    public partial class TorrServerService : ServiceBase
    {
        private Process torrProcess = new Process
        {
            StartInfo =
                    {
                        FileName = "TorrServer.exe",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        ErrorDialog = false
                    }
        };
        public TorrServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var firewallRule = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            var isNotRulleAdd = true;
            foreach (INetFwRule rule in firewallRule.Rules)
            {
                if (rule.Name == "TorrServerService" && rule.Description == "Правило для входящих соединений TorrServer")
                {
                    isNotRulleAdd=false;
                }                
            }
            if (isNotRulleAdd)
            {
                try
                {
                    if (Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) is INetFwPolicy2 firewallPolicy)
                    {
                        INetFwRule ServicefirewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")) as INetFwRule;
                        ServicefirewallRule.Grouping = "TorrServer";
                        ServicefirewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                        ServicefirewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                        ServicefirewallRule.Enabled = true;
                        ServicefirewallRule.InterfaceTypes = "All";
                        ServicefirewallRule.Name = "TorrServerService";                      
                        ServicefirewallRule.Description = "Правило для входящих соединений TorrServer";
                        ServicefirewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                        ServicefirewallRule.LocalPorts = "8090";
                        firewallPolicy.Rules.Add(ServicefirewallRule);
                    }
                }
                catch
                {
                }
            }
            if (torrProcess.Start())
            {
                Process.Start("TorrServerTray.exe");
            }
    
           
        }

        protected override void OnStop()
        {
            torrProcess.Kill();
        }      
    }
}
