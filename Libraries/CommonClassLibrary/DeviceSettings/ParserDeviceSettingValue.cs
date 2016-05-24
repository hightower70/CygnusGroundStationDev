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
using CommonClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingValue : IParserDeviceSettingsBase
	{
		#region · Types ·

		/// <summary>
		/// Type of the value
		/// </summary>
		public enum ValueType : byte
		{
			IntValue = 1,
			EnumValue = 2,
			FloatValue = 3,
			StringValue = 4
		}

		/// <summary>
		/// Binary value info
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
		public class BinaryValueInfo
		{
			private UInt16 m_offset;
			private byte m_size;
			private ValueType m_type;

			public BinaryValueInfo(UInt16 in_offset, byte in_size, ValueType in_type)
			{
				m_offset = in_offset;
				m_size = in_size;
				m_type = in_type;
			}
		}

		#endregion

		#region · Data members ·

		private ParserDeviceSettingsGroup m_parent_group;

		private string m_id;
		private string m_name;
		private ValueType m_value_type;
		private int m_value_index;
		private int m_binary_length;
		private int m_binary_value_offset;
		private object m_value;
		private string m_units;
		private string m_description;
		private int m_min;
		private int m_max;

		private string m_enumdef_ref;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_type">Type of the value</param>
		public ParserDeviceSettingValue(ValueType in_type)
		{
			m_value_type = in_type;

			m_min = int.MinValue;
			m_max = int.MaxValue;
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets reference name of the enum value
		/// </summary>
		public string ID
		{
			get { return m_id; }
		}

		/// <summary>
		/// Gets name of the settings
		/// </summary>
		public string Name
		{
			get { return m_name; }
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
			set
			{
				// set value
				m_value = value;

				// call callback
				if (m_parent_group != null && m_parent_group.Root != null && m_parent_group.Root.Parent != null)
					m_parent_group.Root.Parent.OnValueChanged(this);
			}
		}

		/// <summary>
		/// Gets list of the enum values
		/// </summary>
		public List<ParserDeviceSettingsEnumValue> EnumValues
		{
			get { return m_parent_group.Root.EnumDefs[m_parent_group.Root.GetEnumDefIndex(m_enumdef_ref)].Values; }
		}

		/// <summary>
		/// Minimum value for int data type
		/// </summary>
		public int Min
		{
			get { return m_min; }
			set
			{ m_min = value; }
		}

		/// <summary>
		/// Maximum value for int data type
		/// </summary>
		public int Max
		{
			get { return m_max; }
			set { m_max = value; }
		}

		/// <summary>
		/// Gets the length of the binary data in bytes
		/// </summary>
		/// <returns></returns>
		public int BinaryLength
		{
			get { return m_binary_length; }
		}

		/// <summary>
		/// Gets the offset of the first binary byte of this value
		/// </summary>
		public int BinaryOffset
		{
			get { return m_binary_value_offset; }
		}

		/// <summary>
		/// Gets value index (unique index of this settings value)
		/// </summary>
		public int ValueIndex
		{
			get { return m_value_index; }
		}

		#endregion

		#region · Parser functions ·

		/// <summary>
		/// Parses value description 
		/// </summary>
		/// <param name="in_element">Element to parse</param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get id
			m_id = XMLAttributeParser.ConvertAttributeToString(in_element, "ID", XMLAttributeParser.atObligatory);

			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			// get unit
			m_units = XMLAttributeParser.ConvertAttributeToString(in_element, "Units", XMLAttributeParser.atOptional);

			// get description
			m_description = XMLAttributeParser.ConvertAttributeToString(in_element, "Description", XMLAttributeParser.atOptional);

			// get value
			switch (m_value_type)
			{
				case ValueType.StringValue:
					// if string type is specified length must be existing
					m_binary_length = XMLAttributeParser.ConvertAttributeToInt(in_element, "Length", XMLAttributeParser.atObligatory) + 1; // +1 because of the terminator zero
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

				case ValueType.FloatValue:
					m_binary_length = sizeof(float);
					try
					{
						m_value = (float)in_element.ValueAsDouble;
					}
					catch
					{
						// throw an exception if value is invalid
						XMLParserException exception = new XMLParserException(in_element);
						exception.SetInvalidTypeError(m_name);
						throw exception;
					}
					break;

				case ValueType.EnumValue:
					{
						m_binary_length = sizeof(byte);

						m_enumdef_ref = XMLAttributeParser.ConvertAttributeToString(in_element, "Enum", XMLAttributeParser.atObligatory);

						try
						{
							m_value = (byte)in_element.ValueAsInt;
						}
						catch
						{
							// throw an exception if value is invalid
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidTypeError(m_name);
							throw exception;
						}
					}
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Sets index of this value
		/// </summary>
		/// <param name="in_index"></param>
		internal void SetValueIndex(int in_index)
		{
			m_value_index = in_index;
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
						byte[] retval = new byte[m_binary_length];
						byte[] str = Encoding.ASCII.GetBytes(((string)m_value));

						Array.Copy(str, retval, str.Length);
						return retval;
					}

				case ValueType.IntValue:
					return BitConverter.GetBytes((int)m_value);

				case ValueType.FloatValue:
					return BitConverter.GetBytes((float)m_value);

				case ValueType.EnumValue:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)m_value;

						return retval;
					}

				default:
					return null;
			}
		}

		/// <summary>
		/// Sets value from the raw binary bytes
		/// </summary>
		/// <param name="in_binary_data"></param>
		public void SetBinaryData(byte[] in_binary_data, int in_offset)
		{

		}

		/// <summary>
		/// Sets parent group of this value
		/// </summary>
		/// <param name="in_parent_group"></param>
		internal void SetParentGroup(ParserDeviceSettingsGroup in_parent_group)
		{
			m_parent_group = in_parent_group;
		}

		/// <summary>
		/// Updates current settings values from raw binary file
		/// </summary>
		/// <param name="in_binary_value_file"></param>
		public void UpdateValuesFromBinaryFile(byte[] in_binary_value_file)
		{
			switch (m_value_type)
			{
				case ValueType.StringValue:
					{
						int count = Array.IndexOf<byte>(in_binary_value_file, 0, m_binary_value_offset, m_binary_length) - m_binary_value_offset;
						if (count < 0)
							count = m_binary_length;

						m_value = Encoding.ASCII.GetString(in_binary_value_file, m_binary_value_offset, count);
					}
					break;

				case ValueType.IntValue:
					m_value = BitConverter.ToInt32(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.FloatValue:
					m_value = BitConverter.ToSingle(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.EnumValue:
					m_value = in_binary_value_file[m_binary_value_offset];
					break;
			}
		}

		#endregion

		#region · IParserDeviceSettingsType interface ·

		/// <summary>
		/// Gets type of this class
		/// </summary>
		/// <returns></returns>
		public ParserDeviceSettings.ClassType GetClassType()
		{
			return ParserDeviceSettings.ClassType.Value;
		}

		/// <summary>
		/// Generates value offset
		/// </summary>
		/// <param name="inout_current_offset"></param>
		public void GenerateOffsets(ref int inout_current_offset)
		{
			m_binary_value_offset = inout_current_offset;
			inout_current_offset += m_binary_length;
		}

		/// <summary>
		/// Generates declaration and data files
		/// </summary>
		/// <param name="in_header_file"></param>
		/// <param name="in_default_value_file"></param>
		/// <param name="in_value_info_file"></param>
		public void GenerateFiles(StringBuilder in_header_file, MemoryStream in_value_info_file, MemoryStream in_default_value_file)
		{
			// generate header declaration
			string declaration = "#define cfgVAL_" + m_parent_group.ID.ToUpper() + "_" + m_id.ToUpper() + " " + m_value_index.ToString();

			in_header_file.AppendLine(declaration);

			// generate value info file
			BinaryValueInfo value_info = new BinaryValueInfo((UInt16)m_binary_value_offset, (byte)m_binary_length, m_value_type);
			byte[] value_info_binary = RawBinarySerialization.SerializeObject(value_info);
			in_value_info_file.Write(value_info_binary, 0, value_info_binary.Length);

			// generate default data
			byte[] default_value = GetBinaryData();
			in_default_value_file.Write(default_value, 0, default_value.Length);
		}

		#endregion

	}
}

