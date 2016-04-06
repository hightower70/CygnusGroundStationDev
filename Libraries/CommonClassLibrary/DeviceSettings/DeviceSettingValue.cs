///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2015 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Class for storing settings value
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class DeviceSettingValue
	{
		#region · Types ·

   /// <summary>
   /// Type of the value
   /// </summary>
		public enum ValueType
		{
			StringValue,
			IntValue
		}
   
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_type">Type of the value</param>
		public DeviceSettingValue(ValueType in_type)
		{
			m_value_type = in_type;

			m_min = int.MinValue;
			m_max = int.MaxValue;
		}
		#endregion

		#region · Data members ·

		private string m_name;
		private string m_display_name;
		private ValueType m_value_type;
		private int m_binary_length;
		private object m_value;
		private string m_units;
		private string m_description;
		private int m_min;
		private int m_max;

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets name of the settings
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets display name of the setting value (user readable name of the settings)
		/// </summary>
		public string DisplayName
		{
			get { return m_display_name; }
		}

		/// <summary>
		/// Gets units of the setting value
		/// </summary>
		public string Units
		{
			get { return m_units; }
		}

		/// <summary>
		/// Gets longer description of the settings value
		/// </summary>
		public string Description
		{
			get { return m_description; }
		}

		/// <summary>
		/// Gets valu etype
		/// </summary>
		public ValueType Type
		{
			get { return m_value_type; }
		}

		/// <summary>
		/// Gets/sets value
		/// </summary>
		public object Value
		{
			get { return m_value; }
			set { m_value = value; }
		}

		/// <summary>
		/// Minimum value for int data type
		/// </summary>
		public int Min
		{
			get { return m_min; }
			set { m_min = value; }
		}

		/// <summary>
		/// Maximum value for int data type
		/// </summary>
		public int Max
		{
			get { return m_max; }
			set { m_max = value; }
		}

		#endregion

		#region · Parser functions ·

		/// <summary>
		/// Parses value description 
		/// </summary>
		/// <param name="in_element">Element to parse</param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);
			m_display_name = m_name;

			// get display name
			string display_name = XMLAttributeParser.ConvertAttributeToString(in_element, "DisplayName", 0);
			if (!string.IsNullOrEmpty(display_name))
				m_display_name = display_name;
			else
				m_display_name = m_name;

			// get unit
			m_units = XMLAttributeParser.ConvertAttributeToString(in_element, "Units", 0);

			// get description
			m_description = XMLAttributeParser.ConvertAttributeToString(in_element, "Description", 0);

			// get value
			switch (m_value_type)
			{
				case ValueType.StringValue:
					// if string type is specified length must be existing
					m_binary_length = XMLAttributeParser.ConvertAttributeToInt(in_element, "Length", XMLAttributeParser.atObligatory);
					m_value = in_element.Value;

					// check length
					if (!string.IsNullOrEmpty((string)m_value))
					{
						if (((string)m_value).Length > m_binary_length)
						{
							// throw an exception if string is too long
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetStringIsTooLongError(m_name, m_binary_length);
							throw exception;
						}
					}
					break;

				case ValueType.IntValue:
					m_binary_length = sizeof(Int32);
					try
					{
						m_value = (Int32)in_element.ValueAsInt;
					}
					catch
					{
						// throw an exception if value is invalid
						XMLParserException exception = new XMLParserException(in_element);
						exception.SetInvalidTypeError(m_name);
						throw exception;
					}
					break;

				default:
					break;
			}
		}


		/// <summary>
		/// Gets the length of the binary data in bytes
		/// </summary>
		/// <returns></returns>
		public int GetBinaryLength()
		{
			return m_binary_length;
		}

		/// <summary>
		/// Gets the binary data
		/// </summary>
		/// <returns></returns>
		public byte[] GetBinaryData()
		{
			switch (m_value_type)
			{
				case ValueType.StringValue:
				{
					Encoding encoding = Encoding.ASCII;

					return encoding.GetBytes(((string)m_value));
				}

				case ValueType.IntValue:
					return BitConverter.GetBytes((int)m_value);

				default:
					return null;
			}
		}

		#endregion
	}
}



#if false
			// parse value type
			//ConvertAttributeToEnumProperty(in_element, "PacketType");
  
			// get separators
			//m_data_separator = GetSeparator(in_element, "DataSeparator");
			//m_packet_separator = GetSeparator(in_element, "PacketSeparator");

			// check separators
			if (m_packet_type == PacketTypes.Text)
			{
				if (string.IsNullOrEmpty(m_data_separator) || string.IsNullOrEmpty(m_packet_separator))
					throw PacketParser.CreateXMLParseException(StringConstants.ErrorSeparatorsAreNotDefined, in_element);
			}

			// parse child elements
			XPathNavigator children = in_element;
			child_exists = children.MoveToFirstChild();
			while (child_exists)
			{
				// create child
				if (in_element.NodeType == XPathNodeType.Element)
				{
					PacketDataBase data;

					if (m_data_element_type_lookup.ContainsKey(in_element.Name))
					{
						data = (PacketDataBase)Activator.CreateInstance(m_data_element_type_lookup[in_element.Name], this, in_element.Name);
					}
					else
					{
						throw PacketParser.CreateXMLParseException(string.Format(StringConstants.ErrorInvalidDataType, in_element.Name), in_element);
					}

					data.ParseXML(in_element);

					m_data_elements.Add(data);
				}

				child_exists = in_element.MoveToNext();
			}

#endif
