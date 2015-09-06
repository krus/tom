using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TomComm;
using System.Collections;
using TomWorker.Configuration;

namespace TomWorker
{
	public class Executor
	{
		static log4net.ILog LOG = log4net.LogManager.GetLogger("Executor");
		static Dictionary<string, Executor> executors;
		static object s_SyncRoot;

		static Executor()
		{
			executors = new Dictionary<string, Executor>();
			s_SyncRoot = (executors as ICollection).SyncRoot;
		}

		public static Executor Run(string serviceName)
		{
			Executor executor = null;
			lock(s_SyncRoot)
			{
				if(executors.TryGetValue(serviceName, out executor))
				{
					return executor;
				}

				ServiceNode element = null;
				if (element == null)
				{
					throw new  Exception("未找到服务配置");
				}

				executor = new Executor();
				executor.serviceName = serviceName;
				executor.serviceDirectory = element.Path;
				executor.port = NetPort.AcquriedPort();
				executor.Start();
				executors.Add(serviceName, executor);
				return executor;
			}
		}


		private int tasks;
		private int port;
		private Process process;
		private string serviceDirectory;
		private string serviceName;

		public void PushRequest(EventArgs request)
		{
			ExecutorServiceClient client = new ExecutorServiceClient();
			client.PushRequest(request);
		}

		private void Start()
		{
			if (process == null)
			{
				process = new Process();
				process.StartInfo = new ProcessStartInfo(Context.Current.ExecutorPath, string.Format("{0} {1} {2} {3}",this.serviceName,this.serviceDirectory, Context.Current.WorkerServicePort, port));
				process.Start();
			}
		}

		private void Shutdown()
		{
			if (process != null)
			{
				process.Close();
			}

		}
	}
}
