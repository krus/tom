using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TomComm
{
	[Serializable]
	public class Request
	{
		public string CorrelationId { get; set; }
		public string ReplyTo { get; set; }
		public string ServiceName { get; set; }
		public SortedList<string,object> Parameters { get; set; }
		public ulong DeliveryTag { get; set; }
	}
}
