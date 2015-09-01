using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomComm
{
	[Serializable]
	public class Request
	{
		public string ServiceName { get; set; }
		public SortedList<string,object> Parameters { get; set; }
	}
}
