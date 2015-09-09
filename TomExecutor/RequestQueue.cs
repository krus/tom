using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomComm;
using System.Collections;

namespace TomExecutor
{
	internal class RequestQueue
	{
		static Queue<Request> s_Queue;
		static object s_SyncRoot;

		static RequestQueue()
		{
			s_Queue = new Queue<Request>();
			s_SyncRoot = (s_Queue as ICollection).SyncRoot;
		}

		public static void Push(Request request)
		{
			lock (s_SyncRoot)
			{
				s_Queue.Enqueue(request);
			}
		}

		public static Request Pop()
		{
			lock (s_SyncRoot)
			{
				return s_Queue.Dequeue();
			}
		}

		public static int Count
		{
			get
			{
				lock (s_SyncRoot)
				{
					return s_Queue.Count;
				}
			}
		}
	}
}
