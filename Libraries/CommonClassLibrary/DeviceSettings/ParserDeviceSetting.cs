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

		#endregion

		#region · Data members ·
		private StringBuilder m_header_file;
		private byte[] m_binary_buffer;
		private int m_binary_pos;
		#endregion

		#region · Properties ·
		/// <summary>
		/// Gets settngs root class
		/// </summary>
		public ParserDeviceSettingsRoot DeviceSettingsRoot
		{
			get { return (ParserDeviceSettingsRoot)m_root_class; }
		}
		#endregion

		#region · Overriden members ·

		/// <summary>
		/// Clears all data content of the class
		/// </summary>
		public override void Clear()
		{
			m_root_class = new ParserDeviceSettingsRoot();
		}

		/// <summary>
		/// Parses Packet description
		/// </summary>
		/// <param name="in_element"></param>
		protected override void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			switch (in_element.Name)
			{
				case "Group":
					{
						ParserDeviceSettingsRoot parent = (ParserDeviceSettingsRoot)in_parent;
						ParserDeviceSettingsGroup group = new ParserDeviceSettingsGroup();

						group.ParseXML(in_element);

						ParseXMLChildNodes(in_element, in_xml_stream, group);

						parent.AddGroup(in_element, group);
					}
					break;

				case "String":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						ParserDeviceSettingValue value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.StringValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "Int":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						ParserDeviceSettingValue value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.IntValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "EnumDefs":
					{
						ParserDeviceSettingsRoot parent = (ParserDeviceSettingsRoot)in_parent;
						ParserDeviceSettingsEnumDefs enum_defs = new ParserDeviceSettingsEnumDefs();

						enum_defs.ParseXML(in_element);

						ParseXMLChildNodes(in_element, in_xml_stream, enum_defs);

						parent.AddEnumDefs(in_element, enum_defs);
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
						ParserDeviceSettingValue value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.FloatValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				case "Enum":
					{
						ParserDeviceSettingsGroup parent = (ParserDeviceSettingsGroup)in_parent;
						ParserDeviceSettingValue value = new ParserDeviceSettingValue(ParserDeviceSettingValue.ValueType.EnumValue);

						value.ParseXML(in_element);

						parent.AddValue(in_element, value);
					}
					break;

				default:
					throw CreateXMLParseException(string.Format(ParserDeviceSettingsStringConstants.ErrorInvalidElementType, in_element.Name), in_element);
			}

		}
		#endregion

		#region · Public members ·

		/// <summary>
		/// Generates value offsets for all vallues
		/// </summary>
		public void UpdateValueOffset()
		{
			m_binary_pos = 0;
			GenerateValueOffset(m_root_class);
		}

		/// <summary>
		/// Creates all C files needs for the embedded software
		/// </summary>
		/// <param name="in_path">Full path of the original XML file</param>
		public void CreateCFiles(string in_path)
		{
			string xml_file_name = in_path;
			string xml_settings_xml_file_name;
			long xml_inline_length;
			string xml_file_name_without_extension;
			string xml_header_file_name;
			string define_name;
			string xml_settings_data_file_name;

			// generate file names
			xml_file_name_without_extension = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(xml_file_name)), Path.GetFileNameWithoutExtension(xml_file_name));
			xml_header_file_name = xml_file_name_without_extension + ".h";
			xml_settings_xml_file_name = xml_file_name_without_extension + "_xml.inl";
			xml_settings_data_file_name = xml_file_name_without_extension + "_data.inl";

			// prepare header file
			m_header_file = new StringBuilder();

			m_header_file.AppendLine("///////////////////////////////////////////////////////////////////////////////");
			m_header_file.AppendLine("// This header file was generated by the SettingsParser");
			m_header_file.AppendLine("// at " + DateTime.Now.ToString());
			define_name = "__" + Path.GetFileNameWithoutExtension(xml_header_file_name.ToUpper()) + "_H";
			m_header_file.AppendLine("#ifndef " + define_name);
			m_header_file.AppendLine("#define " + define_name);
			m_header_file.AppendLine();

			// create GZIP-ed xml file
			using (MemoryStream xml_compressed_stream = new MemoryStream())
			{
				using (FileStream xml_file_stream = File.OpenRead(xml_file_name))
				{
					using (GZipStream compression_stream = new GZipStream(xml_compressed_stream, CompressionMode.Compress))
					{
						xml_file_stream.CopyTo(compression_stream);
					}
				}

				m_binary_buffer = xml_compressed_stream.ToArray();
				xml_inline_length = m_binary_buffer.Length;
			}

			WriteBinaryArrayIntoCArray(xml_settings_xml_file_name, m_binary_buffer);

			// traversing the tree and creating header declarations
			m_binary_pos = 0;
			GenerateHeaderDeclarations(m_root_class, "");

			// save binary length
			m_header_file.AppendLine();
			m_header_file.AppendLine("#define cfgSETTINGS_XML_FILE_LENGTH " + xml_inline_length.ToString());
			m_header_file.AppendLine("#define cfgSETTINGS_BINARY_FILE_LENGTH " + m_binary_pos.ToString());

			// save header file
			m_header_file.AppendLine();
			m_header_file.AppendLine("#endif");

			using (StreamWriter header_file = new StreamWriter(xml_header_file_name))
			{
				header_file.Write(m_header_file);
			}

			// save binary settings data (default data)
			m_binary_buffer = new byte[m_binary_pos];
			m_binary_pos = 0;

			GenerateBinarydata(m_root_class);

			WriteBinaryArrayIntoCArray(xml_settings_data_file_name, m_binary_buffer);
		}

		#endregion

		#region · Private functions ·

		/// <summary>
		/// Generates binary settings data
		/// </summary>
		/// <param name="in_settings"></param>
		private void GenerateBinarydata(object in_settings)
		{
			ParserDeviceSettings.ClassType parser_class_type = ((IParserDeviceSettingsType)in_settings).GetClassType();

			switch (parser_class_type)
			{
				case ClassType.Root:
					{
						ParserDeviceSettingsRoot root = (ParserDeviceSettingsRoot)in_settings;

						// process all values
						foreach (ParserDeviceSettingsGroup group in root.Groups)
						{
							GenerateBinarydata(group);
						}
					}
					break;

				case ClassType.Group:
					{
						ParserDeviceSettingsGroup group = (ParserDeviceSettingsGroup)in_settings;

						// process all values
						foreach (ParserDeviceSettingValue value in group.Values)
						{
							GenerateBinarydata(value);
						}
					}
					break;

				case ClassType.Value:
					{
						ParserDeviceSettingValue value = (ParserDeviceSettingValue)in_settings;

						value.GetBinaryData().CopyTo(m_binary_buffer, m_binary_pos);

						m_binary_pos += value.BinaryLength;
					}
					break;
			}
		}

		/// <summary>
		/// Generates (updates) value offsets
		/// </summary>
		/// <param name="in_settings"></param>
		private void GenerateValueOffset(object in_settings)
		{
			ParserDeviceSettings.ClassType parser_class_type = ((IParserDeviceSettingsType)in_settings).GetClassType();

			switch (parser_class_type)
			{
				case ClassType.Root:
					{
						ParserDeviceSettingsRoot root = (ParserDeviceSettingsRoot)in_settings;

						// process all subgroups
						foreach (ParserDeviceSettingsGroup group in root.Groups)
						{
							GenerateValueOffset(group);
						}
					}
					break;

				case ClassType.Group:
					{
						ParserDeviceSettingsGroup group = (ParserDeviceSettingsGroup)in_settings;

						// process all values
						foreach (ParserDeviceSettingValue value in group.Values)
						{
							GenerateValueOffset(value);
						}
					}
					break;

				case ClassType.Value:
					{
						ParserDeviceSettingValue value = (ParserDeviceSettingValue)in_settings;

						value.SetBinaryOffset(m_binary_pos);

						m_binary_pos += value.BinaryLength;
					}
					break;
			}
		}

		/// <summary>
		/// Generates C header declarations
		/// </summary>
		/// <param name="in_settings"></param>
		/// <param name="in_path"></param>
		private void GenerateHeaderDeclarations(object in_settings, string in_path)
		{
			ParserDeviceSettings.ClassType parser_class_type = ((IParserDeviceSettingsType)in_settings).GetClassType();

			switch (parser_class_type)
			{
				case ClassType.Root:
					{
						ParserDeviceSettingsRoot root = (ParserDeviceSettingsRoot)in_settings;

						if (root.EnumDefs.Count > 0)
						{
							m_header_file.AppendLine("// Enum definitions");

							foreach (ParserDeviceSettingsEnumDefs enum_defs in root.EnumDefs)
							{
								GenerateHeaderDeclarations(enum_defs, enum_defs.ID);
							}
						}

						// process all subgroups
						foreach (ParserDeviceSettingsGroup group in root.Groups)
						{
							GenerateHeaderDeclarations(group, group.ID);
						}

					}
					break;

				case ClassType.Group:
					{
						ParserDeviceSettingsGroup group = (ParserDeviceSettingsGroup)in_settings;
						string path = in_path;

						// generate path
						if (!string.IsNullOrEmpty(path))
							path += "_";
						path += group.Name;

						// process all values
						foreach (ParserDeviceSettingValue value in group.Values)
						{
							GenerateHeaderDeclarations(value, in_path);
						}
					}
					break;


				case ClassType.Value:
					{
						ParserDeviceSettingValue value = (ParserDeviceSettingValue)in_settings;

						string declaration = "#define cfg" + in_path.ToUpper() + "_" + value.ID.ToUpper() + " " + m_binary_pos.ToString();

						m_header_file.AppendLine(declaration);

						m_binary_pos += value.BinaryLength;
					}
					break;

				case ClassType.EnumDefs:
					{
						ParserDeviceSettingsEnumDefs enum_defs = (ParserDeviceSettingsEnumDefs)in_settings;

						for (int i=0; i<enum_defs.Values.Count; i++)
						{
							string declaration = "#define cfgENUM_" + enum_defs.ID.ToUpper() + "_" + enum_defs.Values[i].ID.ToUpper() + " " + i.ToString();

							m_header_file.AppendLine(declaration);
						}

						m_header_file.AppendLine();
					}
					break;
			}			
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
