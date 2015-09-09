using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using TomComm;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RabbitMQ.Client.Events;
using System.Threading;

namespace TomRPCClient
{
	public class RPCClient : IDisposable
	{
		const string RoutingKey = "rpc.queue";
		private string mqUri;

		private readonly ConnectionFactory factory;
		private readonly IConnection conn;
		private readonly IModel ch;
		private readonly string replyQueueName;
		private readonly QueueingBasicConsumer consumer;
		private readonly BinaryFormatter binaryFormatter;
		
		public RPCClient(string mqUri)
		{
			this.mqUri = mqUri;

			binaryFormatter = new BinaryFormatter();

			factory = new ConnectionFactory();
			factory.Uri = this.mqUri;

			conn = factory.CreateConnection();
			ch = conn.CreateModel();

			replyQueueName = ch.QueueDeclare().QueueName;

			ch.QueueDeclare("tom.host.1", false, false, false, null);

			consumer = new QueueingBasicConsumer(ch);
			ch.BasicConsume(replyQueueName, true, consumer);

		}

		public IAsyncResult BeginRequest(Request request)
		{
			request.ReplyTo = replyQueueName;
			request.CorrelationId = Guid.NewGuid().ToString();

			byte[] body;
			using (MemoryStream mem = new MemoryStream())
			{
				binaryFormatter.Serialize(mem, request);
				body = mem.ToArray();
			}

			ch.BasicPublish(string.Empty, "tom.host.1", null, body);

			return new RPCAsyncResult(request.CorrelationId, null);
		}

		public object EndRequest(IAsyncResult asyncResult)
		{
			DateTime callTime = DateTime.Now;
			RPCAsyncResult result = asyncResult as RPCAsyncResult;
			while (true)
			{
				var e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
				if (e.BasicProperties.CorrelationId == result.CorrelationId)
				{
					using (MemoryStream mem = new MemoryStream(e.Body))
					{
						return binaryFormatter.Deserialize(mem);
					}
				}

				if ((DateTime.Now - callTime).TotalSeconds > 10)
				{
					throw new Exception("请求超时!");
				}
			}
		}

		public object Request(Request request)
		{
			IAsyncResult asyncResult = BeginRequest(request);
			return EndRequest(asyncResult);
		}

		public void Close()
		{
			conn.Close();
		}

		#region IDisposable 成员

		public void Dispose()
		{
			this.Close();
		}

		#endregion


		internal class RPCAsyncResult : IAsyncResult
		{
			private object thisLock;
			private object state;
			private bool completed;
			public string CorrelationId { get; private set; }

			public RPCAsyncResult(string correlationId, object state)
			{
				this.thisLock = new object();
				this.state = state;
				this.CorrelationId = correlationId;
			}

			#region IAsyncResult 成员

			public object AsyncState
			{
				get { return this.state; }
			}

			public System.Threading.WaitHandle AsyncWaitHandle
			{
				get 
				{
					return null;
				}
			}

			public bool CompletedSynchronously
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsCompleted
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}
	}
}
