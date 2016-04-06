using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPlaneInterface
{
	class PacketConversionTable
	{
		#region · Constants ·
		public const int XPlaneMaxBlockID = 131;
		public const int XPlaneDataBlockElementCount = 8;
		#endregion

		#region · Types ·
		public class ConversionTableEntry
		{
			public int ClassIndex;
			public int MemberIndex;

			public ConversionTableEntry()
			{
				ClassIndex = -1;
				MemberIndex = -1;
			}
		}
		#endregion

		#region · Data members ·
		private ConversionTableEntry[,] m_conversion_table;
		#endregion

		#region · Properties ·
		public ConversionTableEntry this[int in_block_id, int in_block_index]
		{
			get { return m_conversion_table[in_block_id, in_block_index]; }
		}

		#endregion
		/// <summary>
		/// Creates table converting XPlane UDP packets to object members
		/// </summary>
		/// <param name="in_realtime_objects"></param>
		public void CreateConversionTable(ParserRealtimeObjectCollection in_realtime_objects)
		{
			int block_id;
			int block_index;
			int class_index;
			int member_index;
			List<ParserRealtimeObject> objects = in_realtime_objects.Objects;

			// init conversion table
			m_conversion_table = new ConversionTableEntry[XPlaneMaxBlockID, XPlaneDataBlockElementCount];

			for (block_id = 0; block_id < XPlaneMaxBlockID; block_id++)
			{
				for (block_index = 0; block_index < XPlaneDataBlockElementCount; block_index++)
				{
					m_conversion_table[block_id, block_index] = new ConversionTableEntry();
				}
			}

			// process all objects
			for (class_index = 0; class_index < objects.Count; class_index++)
			{
				// process members
				for (member_index = 0; member_index < objects[class_index].Members.Count; member_index++)
				{
					string block_id_string = objects[class_index].Members[member_index].GetAttribute("BlockID");
					string block_index_string = objects[class_index].Members[member_index].GetAttribute("BlockIndex");

					if(!string.IsNullOrEmpty(block_id_string) && !string.IsNullOrEmpty(block_index_string))
					{
						block_id = int.Parse(block_id_string);
						block_index = int.Parse(block_index_string);

						if (block_id >= 0 && block_id < XPlaneMaxBlockID && block_index >= 0 && block_index < XPlaneDataBlockElementCount)
						{
							m_conversion_table[block_id, block_index].ClassIndex = class_index;
							m_conversion_table[block_id, block_index].MemberIndex = member_index;
						}
					}
				}
			}
		}
	}
}
