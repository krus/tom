using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TomComm;
using System.Collections;
using TomWorker.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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

		public static Executor Run(Worker worker,ServiceNode node)
		{
			Executor executor = null;
			lock(s_SyncRoot)
			{
				string key = node.Name + "_" + node.BinDirectory;
				if (executors.TryGetValue(key, out executor))
				{
					return executor;
				}

				executor = new Executor(worker);
				executor.serviceName = node.Name;
				executor.serviceDirectory = node.BinDirectory;
				executor.port = NetPort.AcquriedPort();
				executor.executorServiceName = "Executor_" + executor.port;

				executor.Start();
				executors.Add(key, executor);
				return executor;
			}
		}


		private int port;
		private Process process;
		private string serviceDirectory;
		private string serviceName;
		private string executorServiceName;
		private Worker worker;

		public void PushRequest(Request request)
		{
			ExecutorServiceClient client = new ExecutorServiceClient(new NetTcpBinding(SecurityMode.None), new EndpointAddress(string.Format("net.tcp://{0}:{1}/{2}", Worker.WorkerServiceIP, port, this.executorServiceName)));
			client.PushRequest(request);
		}

		private Executor(Worker worker)
		{
			this.worker = worker;
		}

		private void Start()
		{
			if (process == null)
			{
				process = new Process();
				process.StartInfo = new ProcessStartInfo(Worker.ExecutorFileName, string.Format("{0} {1} {2} {3} {4} {5} {6}", this.serviceName, this.serviceDirectory, Worker.WorkerServicePort, port, Worker.WorkerServiceIP, this.executorServiceName, worker.MQUri));
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
