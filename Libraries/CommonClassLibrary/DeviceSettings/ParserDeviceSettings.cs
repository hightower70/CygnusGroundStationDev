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
// Device settings parser class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettings : XMLParserBase
	{
		#region · Types ·

		public delegate void DeviceSettingsValueChangedCallback(ParserDeviceSettingValue in_value_info);

		/// <summary>
		/// Device settings parser class types
		/// </summary>
		public enum ClassType
		{
			Root,
			Group,
			Value,
			EnumDefs
		};

		public class GeneratedFileNames
		{
			public string ConfigFileName;
			public string HeaderFileName;
			public string XmlDataFileName;
			public string DefaultDataFileName;
			public string ValueInfoFileName;

			public GeneratedFileNames()
			{
				ConfigFileName = null;
				HeaderFileName = null;
				XmlDataFileName = null;
				DefaultDataFileName = null;
				ValueInfoFileName = null;
			}
	}

		#endregion

		#region · Data members ·
		private int m_value_index;
		private DeviceSettingsValueChangedCallback m_value_changed_callback = null;
		#endregion

		#region · Properties ·
		/// <summary>
		/// Gets settings root class
		/// </summary>
		public ParserDeviceSettingsRoot DeviceSettingsRoot
		{
			get { return (ParserDeviceSettingsRoot)m_root_class; }
		}
		#endregion

		#region · Parser functions ·

		/// <summary>
		/// Clears all data content of the class
		/// </summary>
		public override void Clear()
		{
			m_root_class = new ParserDeviceSettingsRoot(this);
			m_value_index = 0;
		}

		/// <summary>
		/// Parses Packet description
		/// </summary>
		/// <param name="in_element"></param>
		protected override void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			ParserDeviceSettingValue value = null;

			switch (in_element.Name)
			{
				case "Group":
					{
						ParserDeviceSettingsRoot parent = (ParserDeviceSettingsRoot)in_parent;
						ParserDeviceSettingsGroup group = new ParserDeviceSettingsGroup();

						group.ParseXML(in_element);

						parent.AddGroup(in_element, group);

						ParseXMLChildNodes(in_element, in_xml_stream, group);
					}
					break;

				case "String":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.StringValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "Int":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.IntValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "EnumDefs":
					{
						ParserDeviceSettingsRoot parent = (ParserDeviceSettingsRoot)in_parent;
						ParserDeviceSettingsEnumDefs enum_defs = new ParserDeviceSettingsEnumDefs();

						enum_defs.ParseXML(in_element);

						parent.AddEnumDefs(in_element, enum_defs);

						ParseXMLChildNodes(in_element, in_xml_stream, enum_defs);
					}
					break;

				case "Title":
					{
						ParserDeviceSettingsEnumDefs parent = (ParserDeviceSettingsEnumDefs)in_parent;
						ParserDeviceSettingsEnumValue title = new ParserDeviceSettingsEnumValue();

						title.ParseXML(in_element);

						parent.AddValue(in_element, title);
					}
					break;

				case "Float":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.FloatValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "Enum":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.EnumValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				default:
					throw CreateXMLParseException(string.Format(ParserDeviceSettingsStringConstants.ErrorInvalidElementType, in_element.Name), in_element);
			}

			if(value != null)
			{
				value.SetValueIndex(m_value_index++);
			}
		}

		/// <summary>
		/// Updates current settings values from raw binary file
		/// </summary>
		/// <param name="in_binary_value_file"></param>
		public void UpdateValuesFromBinaryFile(byte[] in_binary_value_file)
		{
			((ParserDeviceSettingsRoot)m_root_class).UpdateValuesFromBinaryFile(in_binary_value_file);
		}

		#endregion

		#region · Value changed notification ·

		/// <summary>
		/// Sets callback function for value change notification
		/// </summary>
		/// <param name="in_value_changed_callback"></param>
		public void SetValueChangedCallback(DeviceSettingsValueChangedCallback in_value_changed_callback)
		{
			m_value_changed_callback = in_value_changed_callback;
		}

		/// <summary>
		/// Callback function
		/// </summary>
		/// <param name="in_value"></param>
		internal void OnValueChanged(ParserDeviceSettingValue in_value)
		{
			if (m_value_changed_callback != null)
				m_value_changed_callback(in_value);
		}
		#endregion

		#region · File creation functions ·

		/// <summary>
		/// Generates offset of values in the binary offset storage (and updates binary value storage size)
		/// </summary>
		public void GenerateBinaryValueOffset()
		{
			int current_value_offset;

			current_value_offset = 0;

			((ParserDeviceSettingsRoot)m_root_class).GenerateOffsets(ref current_value_offset);
		}


		/// <summary>
		/// Creates all C files needs for the embedded software
		/// </summary>
		/// <param name="in_path">Full path of the original XML file</param>
		public void CreateFiles(GeneratedFileNames in_file_names)
		{
			long xml_inline_length;
			string define_name;
			byte[] binary_buffer;
			StringBuilder header_file_builder;

			// prepare header file
			header_file_builder = new StringBuilder();

			header_file_builder.AppendLine("///////////////////////////////////////////////////////////////////////////////");
			header_file_builder.AppendLine("// This header file was generated by the SettingsParser");
			header_file_builder.AppendLine("// at " + DateTime.Now.ToString());
			define_name = "__" + Path.GetFileNameWithoutExtension(in_file_names.HeaderFileName.ToUpper()) + "_h";
			header_file_builder.AppendLine("#ifndef " + define_name);
			header_file_builder.AppendLine("#define " + define_name);
			header_file_builder.AppendLine();

			// create GZIP-ed xml file
			using (MemoryStream xml_compressed_stream = new MemoryStream())
			{
				using (FileStream xml_file_stream = File.OpenRead(in_file_names.ConfigFileName))
				{
					using (GZipStream compression_stream = new GZipStream(xml_compressed_stream, CompressionMode.Compress))
					{
						xml_file_stream.CopyTo(compression_stream);
					}
				}

				binary_buffer = xml_compressed_stream.ToArray();
				xml_inline_length = binary_buffer.Length;
			}

			WriteBinaryArrayIntoCArray(in_file_names.XmlDataFileName, binary_buffer);

			// traversing the tree and creating header declarations
			MemoryStream value_info_binary_data = new MemoryStream();
			MemoryStream default_value_binary_data = new MemoryStream();
			((ParserDeviceSettingsRoot)m_root_class).GenerateFiles(header_file_builder, value_info_binary_data, default_value_binary_data);

			// save binary length
			header_file_builder.AppendLine();
			header_file_builder.AppendLine("// Configuration constants");
			header_file_builder.AppendLine("#define cfg_XML_DATA_FILE_LENGTH " + xml_inline_length.ToString());
			header_file_builder.AppendLine("#define cfg_VALUE_INFO_DATA_FILE_LENGTH " + value_info_binary_data.Length.ToString());
			header_file_builder.AppendLine("#define cfg_VALUE_DATA_FILE_LENGTH " + default_value_binary_data.Length.ToString());
			header_file_builder.AppendLine("#define cfg_VALUE_COUNT " + m_value_index.ToString());

			// save header file
			header_file_builder.AppendLine();
			header_file_builder.AppendLine("#endif");

			using (StreamWriter header_file = new StreamWriter(in_file_names.HeaderFileName))
			{
				header_file.Write(header_file_builder);
			}

			// save value info data
			WriteBinaryArrayIntoCArray(in_file_names.ValueInfoFileName, value_info_binary_data.ToArray());

			// save default values data
			WriteBinaryArrayIntoCArray(in_file_names.DefaultDataFileName, default_value_binary_data.ToArray());
		}

		/// <summary>
		/// Creates a C array from the given byte array
		/// </summary>
		/// <param name="in_file_name"></param>
		/// <param name="in_array"></param>
		private void WriteBinaryArrayIntoCArray(string in_file_name, byte[] in_array)
		{
			using (StreamWriter c_array_file = File.CreateText(in_file_name))
			{
				string line_buffer = "";
				int pos;

				for (pos = 0; pos < in_array.Length; pos++)
				{
					line_buffer += "0x" + in_array[pos].ToString("X2");

					if (((pos + 1) % 16) == 0 || pos == in_array.Length - 1)
					{
						if (pos < in_array.Length - 1)
							line_buffer += ",";

						c_array_file.WriteLine(line_buffer);

						line_buffer = "";
					}
					else
					{
						line_buffer += ", ";
					}
				}
			}
		}

#endregion
	}
}
