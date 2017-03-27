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
using CommonClassLibrary.XMLParser;
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
			UInt8Value = 1,
			Int8Value = 2,
			UInt16Value = 3,
			Int16Value = 4,
			//UInt32Value = 5,
			Int32Value = 6,

			UInt8FixedValue = 7,
			Int8FixedValue = 8,
			UInt16FixedValue = 9,
			Int16FixedValue = 10,

			EnumValue = 11,

			FloatValue = 12,

			StringValue = 13
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
		private int m_int_min;
		private int m_int_max;
		private int m_multiplier;
		private string m_enumdef_ref;
		private float m_float_min;
		private float m_float_max;
		private int m_fractional_digits;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_type">Type of the value</param>
		public ParserDeviceSettingValue(ValueType in_type)
		{
			m_value_type = in_type;

			switch (in_type)
			{
				case ValueType.UInt8Value:
					m_int_min = Byte.MinValue;
					m_int_max = Byte.MaxValue;
					break;

				case ValueType.Int8Value:
					m_int_min = SByte.MinValue;
					m_int_max = SByte.MaxValue;
					break;

				case ValueType.UInt16Value:
					m_int_min = UInt16.MinValue;
					m_int_max = UInt16.MaxValue;
					break;

				case ValueType.Int16Value:
					m_int_min = Int16.MinValue;
					m_int_max = Int16.MaxValue;
					break;

				case ValueType.Int32Value:
					m_int_min = int.MinValue;
					m_int_max = int.MaxValue;
					break;

				case ValueType.UInt8FixedValue:
					m_float_min = byte.MinValue;
					m_float_max = byte.MaxValue;
					break;

				case ValueType.Int8FixedValue:
					m_float_min = SByte.MinValue;
					m_float_max = SByte.MaxValue;
					break;

				case ValueType.UInt16FixedValue:
					m_float_min = UInt16.MinValue;
					m_float_max = UInt16.MaxValue;
					break;

				case ValueType.Int16FixedValue:
					m_float_min = Int16.MinValue;
					m_float_max = Int16.MaxValue;
					break;

				case ValueType.EnumValue:
					break;

				case ValueType.FloatValue:
					break;

				case ValueType.StringValue:
					break;

				default:
					throw new InvalidDataException();
			}

			m_multiplier = 1;
			m_fractional_digits = 0;
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
		/// Gets/sets value as the stored type
		/// </summary>
		public object Value
		{
			get
			{
				switch (m_value_type)
				{
					case ValueType.UInt8FixedValue:
						return (float)((byte)m_value / m_multiplier);

					case ValueType.Int8FixedValue:
						return (float)((SByte)m_value / m_multiplier);

					case ValueType.UInt16FixedValue:
						return (float)((UInt16)m_value / m_multiplier);

					case ValueType.Int16FixedValue:
						return (float)((Int16)m_value / m_multiplier);

					default:
						return m_value;
				}
			}

			set
			{
				switch(m_value_type)
				{
					case ValueType.UInt8FixedValue:
						m_value = (byte)Math.Round((float)value * m_multiplier);
						break;

					case ValueType.Int8FixedValue:
						m_value = (sbyte)Math.Round((float)value * m_multiplier);
						break;

					case ValueType.UInt16FixedValue:
						m_value = (UInt16)Math.Round((float)value * m_multiplier);
						break;

					case ValueType.Int16FixedValue:
						m_value = (Int16)Math.Round((float)value * m_multiplier);
						break;

					default:
						// set value
						m_value = value;
						break;
				}

				// call callback
				if (m_parent_group != null && m_parent_group.Root != null && m_parent_group.Root.Parent != null)
					m_parent_group.Root.Parent.OnValueChanged(this);
			}
		}

		/// <summary>
		/// Gets/sets value as integer (if possible, otherwise it returns zero and sets nothing)
		/// </summary>
		public int IntValue
		{
			get
			{
				switch(m_value_type)
				{
					case ValueType.UInt8Value:
						return (byte)m_value;

					case ValueType.Int8Value:
						return (SByte)m_value;

					case ValueType.UInt16Value:
						return (UInt16)m_value;

					case ValueType.Int16Value:
						return (Int16)m_value;

					case ValueType.Int32Value:
						return (Int32)m_value;
				}

				return 0;
			}
			set
			{
				switch(m_value_type)
				{
					case ValueType.UInt8Value:
						m_value = (byte)value;
						break;

					case ValueType.Int8Value:
						m_value = (sbyte)value;
						break;

					case ValueType.UInt16Value:
						m_value = (UInt16)value;
						break;

					case ValueType.Int16Value:
						m_value = (Int16)value;
						break;

					case ValueType.Int32Value:
						m_value = (Int32)value;
						break;

					default:
						throw new InvalidDataException();
				}

				// call callback
				if (m_parent_group != null && m_parent_group.Root != null && m_parent_group.Root.Parent != null)
					m_parent_group.Root.Parent.OnValueChanged(this);
			}
		}


		/// <summary>
		/// Gets decimal places for floating numbers (used for user interface only)
		/// </summary>
		public int FractionalDigits
		{
			get { return m_fractional_digits; }
		}

		/// <summary>
		/// Gets list of the enum values
		/// </summary>
		public List<ParserDeviceSettingsEnumValue> EnumValues
		{
			get { return m_parent_group.Root.EnumDefs[m_parent_group.Root.GetEnumDefIndex(m_enumdef_ref)].Values; }
		}

		/// <summary>
		/// Multiplier for fixed value
		/// </summary>
		public int Multiplier
		{
			get { return m_multiplier; }
			set { m_multiplier = value; }
		}

		/// <summary>
		/// Minimum value for int data type
		/// </summary>
		public int IntMin
		{
			get { return m_int_min; }
			set
			{ m_int_min = value; }
		}

		/// <summary>
		/// Maximum value for int data type
		/// </summary>
		public int IntMax
		{
			get { return m_int_max; }
			set { m_int_max = value; }
		}

		/// <summary>
		/// Minimum value for float data type
		/// </summary>
		public float FloatMin
		{
			get { return m_float_min; }
			set	{ m_float_min = value; }
		}

		/// <summary>
		/// Maximum value for float data type
		/// </summary>
		public float FloatMax
		{
			get { return m_float_max; }
			set { m_float_max = value; }
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

				case ValueType.Int32Value:
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

				case ValueType.Int8Value:
					m_binary_length = sizeof(SByte);
					try
					{
						m_value = (SByte)in_element.ValueAsInt;
					}
					catch
					{
						// throw an exception if value is invalid
						XMLParserException exception = new XMLParserException(in_element);
						exception.SetInvalidTypeError(m_name);
						throw exception;
					}
					break;

				case ValueType.UInt8Value:
					{
						int value;
						m_binary_length = sizeof(SByte);
						try
						{
							value = in_element.ValueAsInt;
						}
						catch
						{
							// throw an exception if value is invalid
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidTypeError(m_name);
							throw exception;
						}

						GetAndCheckMinMaxInt(in_element, value);

						m_value = (byte)value;
					}
					break;

				case ValueType.UInt16Value:
					{
						int value;
						m_binary_length = sizeof(UInt16);
						try
						{
							value = in_element.ValueAsInt;
						}
						catch
						{
							// throw an exception if value is invalid
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidTypeError(m_name);
							throw exception;
						}

						GetAndCheckMinMaxInt(in_element, value);

						m_value = (UInt16)value;
					}
					break;

					// Fixed values
				case ValueType.UInt8FixedValue:
				case ValueType.Int8FixedValue:
				case ValueType.UInt16FixedValue:
				case ValueType.Int16FixedValue:
					{
						float float_value;
						object value = 0;

						m_binary_length = sizeof(Int16);

						// get multiplier
						m_multiplier = XMLAttributeParser.ConvertAttributeToInt(in_element, "Multiplier", XMLAttributeParser.atObligatory);

						if(m_multiplier < 1)
						{
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidAttributeValue("Multiplier");
							throw exception;
						}
						m_float_min /= m_multiplier;
						m_float_max /= m_multiplier; 

						// get fractional digits number
						m_fractional_digits = XMLAttributeParser.ConvertAttributeToInt(in_element, "FractionalDigits", XMLAttributeParser.atOptional);
						if (m_fractional_digits < 0)
						{
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidAttributeValue("FractionalDigits");
							throw exception;
						}

						// store value
						try
						{
							float_value = (float)in_element.ValueAsDouble;

							switch (m_value_type)
							{
								case ValueType.UInt8FixedValue:
									value = (byte)Math.Round(float_value * m_multiplier);
									break;

								case ValueType.Int8FixedValue:
									value = (SByte)Math.Round(float_value * m_multiplier);
									break;

								case ValueType.UInt16FixedValue:
									value = (UInt16)Math.Round(float_value * m_multiplier);
									break;

								case ValueType.Int16FixedValue:
									value = (Int16)Math.Round(float_value * m_multiplier);
									break;
							}
						}
						catch
						{
							// throw an exception if value is invalid
							XMLParserException exception = new XMLParserException(in_element);
							exception.SetInvalidTypeError(m_name);
							throw exception;
						}

						GetAndCheckMinMaxFloat(in_element, float_value);

						m_value = value;
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
					throw new InvalidDataException();
			}
		}

		/// <summary>
		/// Gets min, max values 
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_value"></param>
		private void GetAndCheckMinMaxInt(XPathNavigator in_element, int in_value)
		{
			m_int_min = XMLAttributeParser.ConvertAttributeToInt(in_element, "Min", XMLAttributeParser.atOptional, m_int_min);
			m_int_max = XMLAttributeParser.ConvertAttributeToInt(in_element, "Max", XMLAttributeParser.atOptional, m_int_max);

			if (in_value < m_int_min && in_value > m_int_max)
			{
				XMLParserException exception = new XMLParserException(in_element);
				exception.SetOutOfRangeError();
				throw exception;
			}
		}

		/// <summary>
		/// Gets min, max values 
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_value"></param>
		private void GetAndCheckMinMaxFloat(XPathNavigator in_element, float in_value)
		{
			m_float_min = (float)XMLAttributeParser.ConvertAttributeToDouble(in_element, "Min", XMLAttributeParser.atOptional, m_float_min);
			m_float_max = (float)XMLAttributeParser.ConvertAttributeToDouble(in_element, "Max", XMLAttributeParser.atOptional, m_float_max);

			if (in_value < m_float_min && in_value > m_float_max)
			{
				XMLParserException exception = new XMLParserException(in_element);
				exception.SetOutOfRangeError();
				throw exception;
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
				case ValueType.UInt8Value:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)m_value;

						return retval;
					}

				case ValueType.Int8Value:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)((sbyte)m_value);

						return retval;
					}

				case ValueType.UInt16Value:
					return BitConverter.GetBytes((UInt16)m_value);

				case ValueType.Int16Value:
					return BitConverter.GetBytes((Int16)m_value);

				case ValueType.Int32Value:
					return BitConverter.GetBytes((int)m_value);

				case ValueType.UInt8FixedValue:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)m_value;

						return retval;
					}

				case ValueType.Int8FixedValue:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)((sbyte)m_value);

						return retval;
					}

				case ValueType.UInt16FixedValue:
					return BitConverter.GetBytes((UInt16)m_value);

				case ValueType.Int16FixedValue:
					return BitConverter.GetBytes((Int16)m_value);

				case ValueType.EnumValue:
					{
						byte[] retval = new byte[1];

						retval[0] = (byte)m_value;

						return retval;
					}

				case ValueType.FloatValue:
					return BitConverter.GetBytes((float)m_value);

				case ValueType.StringValue:
					{
						byte[] retval = new byte[m_binary_length];
						byte[] str = Encoding.ASCII.GetBytes(((string)m_value));

						Array.Copy(str, retval, str.Length);
						return retval;
					}

				default:
					throw new InvalidDataException();
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
				case ValueType.UInt8Value:
					m_value = in_binary_value_file[m_binary_value_offset];
					break;

				case ValueType.Int8Value:
					m_value = (SByte)in_binary_value_file[m_binary_value_offset];
					break;

				case ValueType.UInt16Value:
					m_value = BitConverter.ToUInt16(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.Int16Value:
					m_value = BitConverter.ToInt16(in_binary_value_file, m_binary_value_offset);
					break;

/*				case ValueType.UInt32Value:
					m_value = BitConverter.ToUInt32(in_binary_value_file, m_binary_value_offset);
					break;*/

				case ValueType.Int32Value:
					m_value = BitConverter.ToInt32(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.UInt8FixedValue:
					m_value = in_binary_value_file[m_binary_value_offset];
					break;

				case ValueType.Int8FixedValue:
					m_value = (SByte)in_binary_value_file[m_binary_value_offset];
					break;

				case ValueType.UInt16FixedValue:
					m_value = BitConverter.ToUInt16(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.Int16FixedValue:
					m_value = BitConverter.ToInt16(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.EnumValue:
					m_value = in_binary_value_file[m_binary_value_offset];
					break;

				case ValueType.FloatValue:
					m_value = BitConverter.ToSingle(in_binary_value_file, m_binary_value_offset);
					break;

				case ValueType.StringValue:
					{
						int count = Array.IndexOf<byte>(in_binary_value_file, 0, m_binary_value_offset, m_binary_length) - m_binary_value_offset;
						if (count < 0)
							count = m_binary_length;

						m_value = Encoding.ASCII.GetString(in_binary_value_file, m_binary_value_offset, count);
					}
					break;

				default:
					throw new InvalidDataException();
			}
		}

		/// <summary>
		/// Creates value class based on the XML content
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_xml_stream"></param>
		/// <param name="in_parent"></param>
		/// <returns></returns>
		public static ParserDeviceSettingValue ValueFactory(XPathNavigator in_element, TextReader in_xml_stream, ParserDeviceSettingsGroup in_parent)
		{
			ParserDeviceSettingValue value = null;

			// create value class
			switch (in_element.Name)
			{
				// UInt8 type
				case "UInt8":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.UInt8Value);
					break;

				// Int8 type
				case "Int8":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.Int8Value);
					break;

				// UInt16 type
				case "UInt16":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.UInt16Value);
					break;

				// UInt16 type
				case "Int16":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.Int16Value);
					break;

				case "Int32":
				case "Int":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.Int32Value);
					break;

				// Fixed UInt8 type
				case "FixedUInt8":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.UInt8FixedValue);
					break;

				// Fixed UInt8 type
				case "Int8Fixed":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.Int8FixedValue);
					break;


				// Fixed UInt16 type
				case "UInt16Fixed":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.UInt16FixedValue);
					break;

				// Fixed Int16 type
				case "Int16Fixed":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.Int16FixedValue);
					break;

				// enum type
				case "Enum":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.EnumValue);
					break;

				// float type
				case "Float":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.FloatValue);
					break;

				// string type
				case "String":
					value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.StringValue);
					break;

				default:
					throw XMLParserBase.CreateXMLParseException(string.Format(ParserDeviceSettingsStringConstants.ErrorInvalidElementType, in_element.Name), in_element);
			}

			// parse and store value class
			if (value != null)
			{
				value.ParseXML(in_element);

				in_parent.AddValue(in_element, value);
			}

			return value;
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
		public void GenerateFiles(ParserConfig in_parser_config)
		{
			string declaration;

			// generate header declaration
			if (in_parser_config.UseOffsets)
				declaration = "#define cfgVAL_" + m_parent_group.ID.ToUpper() + "_" + m_id.ToUpper() + " " + m_binary_value_offset.ToString();
			else
				declaration = "#define cfgVAL_" + m_parent_group.ID.ToUpper() + "_" + m_id.ToUpper() + " " + m_value_index.ToString();

			in_parser_config.HeaderFile.AppendLine(declaration);

			// generate value info file
			BinaryValueInfo value_info = new BinaryValueInfo((UInt16)m_binary_value_offset, (byte)m_binary_length, m_value_type);
			byte[] value_info_binary = RawBinarySerialization.SerializeObject(value_info);
			in_parser_config.ValueInfoFile.Write(value_info_binary, 0, value_info_binary.Length);

			// generate default data
			byte[] default_value = GetBinaryData();
			in_parser_config.DefaultValueFile.Write(default_value, 0, default_value.Length);
		}

		#endregion

	}
}

