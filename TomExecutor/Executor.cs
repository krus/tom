using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TomExecutorServiceContract;
using Amib.Threading;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TomComm;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TomExecutor
{
	public class Executor : IDisposable
	{
		public bool IsStop { set; get; }
		private SmartThreadPool smartThreadPool;

		private string serviceDirectory;
		private int workerServicePort;
		private string executorServiceName;
		private string executorServiceIP;
		private int executorServicePort;
		private string mqUri;
		
		private AutoResetEvent sign;

		private ConnectionFactory factory;
		private IModel ch;
		private IConnection conn;
		private BinaryFormatter binaryFormatter;

		private ServiceHost host;

		public Executor(string serviceDirectory, int workerServicePort, string executorServiceName, string executorServiceIP, int executorServicePort, string mqUri)
		{
			this.serviceDirectory = serviceDirectory;
			this.workerServicePort = workerServicePort;
			this.executorServiceName = executorServiceName;
			this.executorServiceIP = executorServiceIP;
			this.executorServicePort = executorServicePort;
			this.mqUri = mqUri;

			this.binaryFormatter = new BinaryFormatter();
			this.sign = new AutoResetEvent(false);
		}

		public void Start()
		{
			factory = new ConnectionFactory();
			factory.Uri = this.mqUri;
			conn = factory.CreateConnection();
			ch = conn.CreateModel();

			OpenExecuterService(executorServiceIP, executorServicePort, executorServiceName);

			smartThreadPool = new SmartThreadPool(new STPStartInfo()
			{
				MinWorkerThreads = 3,
				MaxWorkerThreads = 10
			});

			while (!IsStop)
			{
				sign.WaitOne();

				if (RequestQueue.Count > 0)
				{
					Request req = RequestQueue.Pop();
					smartThreadPool.QueueWorkItem(WorkCallback, req);
				}
			}
		}

		private object WorkCallback(object state)
		{
			Request request = (Request)state;

			byte[] bytes = null;
			Task task = Task.TryGetTask(request.ServiceName, serviceDirectory);
			object objRet = task.Execute(request);
			if (objRet != null)
			{
				using (MemoryStream mem = new MemoryStream())
				{
					binaryFormatter.Serialize(mem, objRet);
					bytes = mem.ToArray();
				}
			}

			IBasicProperties responseProps = ch.CreateBasicProperties();
			responseProps.CorrelationId = request.CorrelationId;
			ch.BasicPublish(string.Empty, request.ReplyTo, responseProps, bytes);
			return null;
		}

		public void Set()
		{
			sign.Set();
		}

		public void Stop()
		{
			IsStop = true;
			sign.Set();
		}

		private void OpenExecuterService(string ip, int port, string serviceName)
		{
			NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
			binding.Security.Mode = SecurityMode.None;

			host = new ServiceHost(typeof(ExecutorServiceImpl));
			host.AddServiceEndpoint(typeof(IExecutorService), binding,
				new Uri(string.Format("net.tcp://{0}:{1}/{2}", ip, port, serviceName)));

			if (host.State != CommunicationState.Opening)
				host.Open();
		}

		#region IDisposable 成员

		public void Dispose()
		{
			conn.Close();
			ch.Dispose();
			host.Close();
		}

		#endregion
	}

	
}
