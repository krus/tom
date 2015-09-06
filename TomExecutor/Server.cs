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
	public class Server
	{
		const string ExchangeName = "amq.direct";
		public bool IsStop { set; get; }
		private SmartThreadPool smartThreadPool;

		private string serviceName;
		private string serviceDirectory;
		private int workerServicePort;
		private string executorServiceName;
		private string executorServiceIP;
		private int executorServicePort;
		private AutoResetEvent sign;

		private ConnectionFactory factory;
		private IModel ch;
		private IConnection conn;
		private BinaryFormatter binaryFormatter;

		public Server(string serviceName, string serviceDirectory, int workerServicePort, string executorServiceName, string executorServiceIP, int executorServicePort)
		{
			this.serviceName = serviceName;
			this.serviceDirectory = serviceDirectory;
			this.workerServicePort = workerServicePort;
			this.executorServiceName = executorServiceName;
			this.executorServiceIP = executorServiceIP;
			this.executorServicePort = executorServicePort;

			this.binaryFormatter = new BinaryFormatter();
		}

		public void Start()
		{
			factory = new ConnectionFactory();
			factory.Uri = string.Empty;
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
					EventArgs req = RequestQueue.Pop();
					smartThreadPool.QueueWorkItem(WorkCallback, req);
				}
			}
		}

		private object WorkCallback(object state)
		{
			BasicDeliverEventArgs eventArgs = (BasicDeliverEventArgs)state;
			IBasicProperties props = eventArgs.BasicProperties;
			string serviceName = props.Headers["ServiceName"].ToString();

			byte[] bytes = null;
			
			Request request = null;
			using (MemoryStream mem = new MemoryStream())
			{
				request = (Request)binaryFormatter.Deserialize(mem);
			}

			Executor executor = Executor.TryGetExecutor(serviceName, serviceDirectory);
			object objRet = executor.Execute(request);
			if (objRet != null)
			{
				using (MemoryStream mem = new MemoryStream())
				{
					binaryFormatter.Serialize(mem, objRet);
					bytes = mem.ToArray();
				}
			}

			IBasicProperties responseProps = ch.CreateBasicProperties();
			responseProps.CorrelationId = props.CorrelationId;
			ch.BasicPublish(ExchangeName, props.ReplyTo, responseProps, bytes);
			ch.BasicAck(eventArgs.DeliveryTag, false);
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
