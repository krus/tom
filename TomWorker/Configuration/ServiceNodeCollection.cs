using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TomWorker.Configuration
{
	public sealed class ServiceNodeCollection
	{
		private Dictionary<string, ServiceNode> services = new Dictionary<string,ServiceNode>();

		public void Add(ServiceNode node)
		{
			services.Add(node.Name, node);
		}

		public ServiceNode Find(string name)
		{
			ServiceNode node = null;
			services.TryGetValue(name, out node);
			return node;
		}
	}
}
