using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceNode
	{
		public int AppId { get; set; }
		public string Name { get; set; }
		public string BinDirectory { get; set; }
		public int Workers { get; set; }
	}
}
