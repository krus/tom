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
		public static Executor Server { get; private set; }
		static void Main(string[] args)
		{
			//args = @"test.test E:\project\Tom\Test\testbin 1800 18001 192.168.20.133 Executor_18001 amqp://test:test@115.29.236.46:5672".Split(' ');
			string serviceName = args[0];
			string serviceDirectory = args[1];
			int workerServicePort = Convert.ToInt32(args[2]);
			int executorServicePort = Convert.ToInt32(args[3]);
			string executorServiceIP = args[4];
			string executorServiceName = args[5];
			string mqUri = args[6];

			Server = new Executor(serviceName, serviceDirectory, workerServicePort, executorServiceName, executorServiceIP, executorServicePort, mqUri);
			Server.Start();
		}

		
	}
}
