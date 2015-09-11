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
	public class RPCClient
	{
		private int appId;
		private IConnection connection;
		private IModel channel;
		private string replyQueueName;
		private QueueingBasicConsumer consumer;
		private BinaryFormatter binaryFormatter = new BinaryFormatter();

		public RPCClient(int appId, string mqUri)
		{
			this.appId = appId;
			var factory = new ConnectionFactory();
			factory.Uri = mqUri;
			connection = factory.CreateConnection();
			channel = connection.CreateModel();
			replyQueueName = channel.QueueDeclare().QueueName;
			consumer = new QueueingBasicConsumer(channel);
			channel.BasicConsume(queue: replyQueueName,
								 noAck: true,
								 consumer: consumer);
		}

		public object Invoke(Request request)
		{
			var corrId = Guid.NewGuid().ToString();
			var props = channel.CreateBasicProperties();
			props.ReplyTo = replyQueueName;
			props.CorrelationId = corrId;

			byte[] messageBytes;

			using (MemoryStream ms = new MemoryStream())
			{
				binaryFormatter.Serialize(ms, request);
				messageBytes = ms.ToArray();
			}

			channel.BasicPublish(exchange: "",
								 routingKey: "tom.app." + this.appId,
								 basicProperties: props,
								 body: messageBytes);

			while (true)
			{
				var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
				if (ea.BasicProperties.CorrelationId == corrId)
				{
					using (MemoryStream ms = new MemoryStream(ea.Body))
					{
						return binaryFormatter.Deserialize(ms);
					}
				}
			}
		}

		public void Close()
		{
			connection.Close();
		}
	}
}
