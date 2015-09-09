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

			int hostId;
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomHostId"], out hostId);

			Worker worker = new Worker(hostId);
			worker.Start();

			System.Console.Read();
		}
	}
}
