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
		static Queue<EventArgs> s_Queue;
		static object s_SyncRoot;

		static RequestQueue()
		{
			s_Queue = new Queue<EventArgs>();
			s_SyncRoot = (s_Queue as ICollection).SyncRoot;
		}

		public static void Push(EventArgs request)
		{
			lock (s_SyncRoot)
			{
				s_Queue.Enqueue(request);
			}
		}

		public static EventArgs Pop()
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
