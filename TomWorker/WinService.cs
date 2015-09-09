using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using System.ServiceModel;
using TomWorkerServiceContract;
using TomMasterServiceContract.Entities;
using RabbitMQ.Client.Events;
using System.Runtime.Serialization.Formatters.Binary;
using TomComm;
using System.IO;

namespace TomWorker
{
	public partial class WinService : ServiceBase
	{
		private static log4net.ILog LOG = log4net.LogManager.GetLogger("WinService");

		private Worker worker;
		public WinService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			log4net.Config.DOMConfigurator.Configure();

			int hostId;
			int.TryParse(System.Configuration.ConfigurationManager.AppSettings["TomHostId"], out hostId);

			worker = new Worker(hostId);
			worker.Start();
		}

		protected override void OnStop()
		{
			worker.Stop();
		}
	}
}
