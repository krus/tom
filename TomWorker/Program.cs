using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace TomWorker
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		static void Main()
		{
			MainConsole.Start();


			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new WinService() 
			};
			ServiceBase.Run(ServicesToRun);
		}
	}


	class MainConsole
	{
		public static void Start()
		{
			log4net.Config.DOMConfigurator.Configure();

			int appId;
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomAppId"], out appId);

			Worker worker = new Worker(appId);
			System.Console.Read();
		}
	}
}
