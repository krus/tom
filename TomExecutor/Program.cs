using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amib.Threading;
using System.ServiceModel;
using TomExecutorServiceContract;
using TomComm;

namespace TomExecutor
{
	class Program
	{
		public static bool IsStop { set; get; }
		static SmartThreadPool s_Smart;
        
		static void Main(string[] args)
		{
			string serviceName = args[0];
			string serviceDirectory = args[1];
			int workerServicePort = Convert.ToInt32(args[2]);
			int executorServicePort = Convert.ToInt32(args[3]);
			string executorServiceIP = args[4];
			string executorServiceName = args[5];

			OpenExecuterService(executorServiceIP, executorServicePort, executorServiceName);

			s_Smart = new SmartThreadPool(new STPStartInfo()
			{
				MinWorkerThreads = 3,
				MaxWorkerThreads = 10
			});

			while(RequestQueue.Count > 0)
			{
				Request  req = RequestQueue.Pop();


				if (RequestQueue.Count == 0 && IsStop)
				{
					break;
				}
			}
		}

		static void OpenExecuterService(string ip, int port, string serviceName)
		{
			NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
			binding.Security.Mode = SecurityMode.None;

			using (ServiceHost host = new ServiceHost(typeof(ExecutorServiceImpl)))
			{
				host.AddServiceEndpoint(typeof(IExecutorService), binding,
					new Uri(string.Format("net.tcp://{0}:{1}/{2}", ip, port, serviceName)));

				if (host.State != CommunicationState.Opening)
					host.Open();
			}
		}
	}
}
