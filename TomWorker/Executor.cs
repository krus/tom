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
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TomWorker
{
	public class Executor
	{
		static log4net.ILog LOG = log4net.LogManager.GetLogger("Executor");
		static NetPort s_ExecutorServiceNetPort = new NetPort(18000);

		private int appId;
		private int servicePort;
		private Process process;
		private string serviceDirectory;
		private string executorServiceName;
		private Worker worker;

		public ExecutorServiceClient ExecutorService { private set; get; }

		public void PushRequest(Request request)
		{
			this.ExecutorService.PushRequest(request);
		}

		public Executor(Worker worker, int appId, string binDirectory)
		{
			this.worker = worker;
			this.appId = appId;
			this.serviceDirectory = binDirectory;
			this.servicePort = s_ExecutorServiceNetPort.AcquriedPort();
			this.executorServiceName = "Executor_" + this.servicePort;
		}

		public void Start()
		{
			if (process == null)
			{
				process = new Process();
				process.EnableRaisingEvents = true;
				process.Exited += new EventHandler(process_Exited);
				process.StartInfo = new ProcessStartInfo(Worker.ExecutorFileName, string.Format("{0} {1} {2} {3} {4} {5} {6}", this.serviceDirectory, worker.WorkerServicePort, servicePort, Worker.WorkerServiceIP, this.executorServiceName, worker.MQUri,this.worker.WorkerServiceName));
				process.Start();

				this.WaitExecutorStart();

				this.ExecutorService = new ExecutorServiceClient(new NetTcpBinding(SecurityMode.None), new EndpointAddress(string.Format("net.tcp://{0}:{1}/{2}", Worker.WorkerServiceIP, servicePort, this.executorServiceName)));
			}
		}

		private void WaitExecutorStart()
		{
			TcpClient client = new TcpClient();
			
			int i = 0;
			while(i<10)
			{
				try
				{
					client.Connect(new IPEndPoint(IPAddress.Parse(Worker.WorkerServiceIP), this.servicePort));
					if (client.Connected)
					{
						break;
					}
				}
				catch
				{
					Thread.Sleep(300);
				}
			}
		}

		private void process_Exited(object sender, EventArgs e)
		{
			worker.OnExecutorExit(this);
		}

		private void Shutdown()
		{
			if (process != null)
			{
				process.Close();
			}
		}

		public void Kill()
		{
			if (process != null)
			{
				process.Kill();
			}
		}

		public bool Ping()
		{
			return true;
		}
	}
}
