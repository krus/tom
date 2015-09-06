using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomComm
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TomServiceAttribute : Attribute
	{
		private string serviceName;
		public TomServiceAttribute(string serviceName)
		{
			this.serviceName = serviceName;
		}
	}
}
