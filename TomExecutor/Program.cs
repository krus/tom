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
			string serviceDirectory = args[0];
			int workerServicePort = Convert.ToInt32(args[1]);
			int executorServicePort = Convert.ToInt32(args[2]);
			string executorServiceIP = args[3];
			string executorServiceName = args[4];
			string mqUri = args[5];

			Server = new Executor(serviceDirectory, workerServicePort, executorServiceName, executorServiceIP, executorServicePort, mqUri);
			Server.Start();
		}

		
	}
}
