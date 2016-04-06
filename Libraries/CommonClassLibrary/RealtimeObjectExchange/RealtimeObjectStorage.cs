using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class RealtimeObjectStorage
	{
		private List<ParserRealtimeObject> m_objects = new List<ParserRealtimeObject>();


		public void ObjectCreationBegin()
		{

		}

		public void ObjectCreationEnd()
		{

		}

		public ParserRealtimeObject CreateObject(string in_name)
		{
			ParserRealtimeObject realtime_object = new ParserRealtimeObject(in_name);

			m_objects.Add(realtime_object);

			return realtime_object;
		}
	}
}
