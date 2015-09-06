using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceNode
	{
		public string Path { get; set; }
		public int Workers { get; set; }
	}
}
