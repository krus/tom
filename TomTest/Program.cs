using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Content;

namespace TomTest
{
	class Program
	{
		const string QueueName = "queue.test";
		const string ExchangeName = "exchange.test";
		const string RoutingKey = "routing.test";
		static void Main(string[] args)
		{
			SendMsg("routing.test");
		}

		static void SendMsg(string routingKey)
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.Uri = "amqp://test:test@115.29.236.46:5672";
			using (IConnection conn = factory.CreateConnection())
			{
				using (IModel ch = conn.CreateModel())
				{
					QueueingBasicConsumer consumer = new QueueingBasicConsumer(ch);
					QueueDeclareOk result = ch.QueueDeclare(QueueName, true, false, false, null);
					ch.QueueBind(QueueName, "amq.direct", routingKey);

					IMapMessageBuilder b = new MapMessageBuilder(ch);
					((IBasicProperties)b.GetContentHeader()).DeliveryMode = 2;
					b.Headers.Add("test", 1);

					ch.BasicPublish(ExchangeName, RoutingKey, (IBasicProperties)b.GetContentHeader(), new byte[]{0,1,0,1});
				}
			}
		}

		static void ReviceMsg(string routingKey)
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.Uri = "amqp://test:test@115.29.236.46:5672";
			using (IConnection conn = factory.CreateConnection())
			{
				using (IModel ch = conn.CreateModel())
				{
					QueueingBasicConsumer consumer = new QueueingBasicConsumer(ch);
					QueueDeclareOk result = ch.QueueDeclare(QueueName, true, false, false, null);
					ch.BasicConsume(QueueName, factory, 
					ch.QueueBind(QueueName, "amq.direct", routingKey);

					IMapMessageBuilder b = new MapMessageBuilder(ch);
					((IBasicProperties)b.GetContentHeader()).DeliveryMode = 2;
					b.Headers.Add("test", 1);

					ch.BasicPublish(ExchangeName, RoutingKey, (IBasicProperties)b.GetContentHeader(), new byte[] { 0, 1, 0, 1 });
				}
			}
		}
	}
}
