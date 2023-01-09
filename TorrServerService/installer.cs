using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace TorrServerService
{
	[RunInstaller(true)]
	public class ProjectInstaller : Installer
	{
		private readonly ServiceProcessInstaller serviceProcessInstaller;
		private readonly ServiceInstaller serviceInstaller;

		public ProjectInstaller()
		{
			serviceProcessInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();
			// Here you can set properties on serviceProcessInstaller or register event handlers
			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.ServicesDependedOn = new string[] { "RpcSs" };
            serviceInstaller.ServiceName = "TorrServerService";
			serviceInstaller.DisplayName = "TorrServer Service";
			serviceInstaller.Description = "Служба управления TorrServer";
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
		}
	}
}