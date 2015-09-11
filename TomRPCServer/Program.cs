using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TomRPCServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var factory = new ConnectionFactory();
			

			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.QueueDeclare(queue: "rpc_queue",
									 durable: false,
									 exclusive: false,
									 autoDelete: false,
									 arguments: null);
				channel.BasicQos(0, 1, false);
				var consumer = new QueueingBasicConsumer(channel);
				channel.BasicConsume(queue: "rpc_queue",
									 noAck: false,
									 consumer: consumer);
				Console.WriteLine(" [x] Awaiting RPC requests");

				while (true)
				{
					string response = null;
					var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

					var body = ea.Body;
					var props = ea.BasicProperties;
					var replyProps = channel.CreateBasicProperties();
					replyProps.CorrelationId = props.CorrelationId;

					try
					{
						var message = Encoding.UTF8.GetString(body);
						response = "test.test";
					}
					catch (Exception e)
					{
						Console.WriteLine(" [.] " + e.Message);
						response = "";
					}
					finally
					{
						var responseBytes = Encoding.UTF8.GetBytes(response);
						channel.BasicPublish(exchange: "",
											 routingKey: props.ReplyTo,
											 basicProperties: replyProps,
											 body: responseBytes);
						channel.BasicAck(deliveryTag: ea.DeliveryTag,
										 multiple: false);
					}
				}
			}
		}
	}
}
