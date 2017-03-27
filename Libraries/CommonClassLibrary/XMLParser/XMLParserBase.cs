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
// Base class for XML parser
///////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace CommonClassLibrary.XMLParser
{
	/// <summary>
	/// Base class for XML parsing
	/// </summary>
	public class XMLParserBase
	{
		#region · Data members ·

		// error report
		private string m_error_message = "";
		private int m_error_line = 0;
		private int m_error_col = 0;

		protected object m_root_class;

		#endregion

		#region · Constructor&Destructor ·
		public XMLParserBase()
		{
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets error message if XML parsing is failed
		/// </summary>
		public string ErrorMessage
		{
			get { return m_error_message; }
		}

		/// <summary>
		/// Returns line number where parse error occured
		/// </summary>
		public int ErrorLine
		{
			get { return m_error_line; }
		}

		/// <summary>
		/// Returns column numnber where parse error occured
		/// </summary>
		public int ErrorColumn
		{
			get { return m_error_col; }
		}

		#endregion

		#region · Members to override ·

		/// <summary>
		/// Clears all data content of the class
		/// </summary>
		public virtual void Clear()
		{
		}

		/// <summary>
		/// Parses Packet description
		/// </summary>
		/// <param name="in_element"></param>
		protected virtual void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent_class)
		{
		}

		protected virtual void ParseBase(XPathNavigator in_element, TextReader in_xml_stream)
		{
		}
		#endregion

		#region · Parser functions ·

		/// <summary>
		/// Parses a GZIP comressed XML file
		/// </summary>
		/// <param name="in_start_path"></param>
		/// <param name="in_xml_file_name"></param>
		/// <returns></returns>
		public bool ParserXMLFileGZIP(string in_start_path, string in_xml_file_name)
		{
			bool retval = false;

			using (FileStream compressed_stream = File.Open(in_xml_file_name, FileMode.Open))
			{
				using (GZipStream decompressed_stream = new GZipStream(compressed_stream, CompressionMode.Decompress))
				{
					using (StreamReader reader = new StreamReader(decompressed_stream))
					{
						retval = ParseXMLStream(in_start_path, reader);
					}
				}
			}

			return retval;
		}

		/// <summary>
		/// Parses XML file from resource
		/// </summary>
		/// <param name="in_start_path"></param>
		/// <param name="in_resource_file_name"></param>
		/// <returns></returns>
		public bool ParseXMLFileFromResource(string in_start_path, string in_resource_file_name)
		{
			bool retval = false;


			Assembly assembly = Assembly.GetEntryAssembly();
			Stream stream = assembly.GetManifestResourceStream(in_resource_file_name);

			using (StreamReader stream_reader = new StreamReader(stream))
			{
				retval = ParseXMLStream(in_start_path, stream_reader);
			}

			return retval;
		}

		/// <summary>
		/// Parses XML as a file
		/// </summary>
		/// <param name="in_xml"></param>
		/// <returns></returns>
		public bool ParseXMLFile(string in_start_path, string in_xml_file_name)
		{
			bool retval = false;

			using (TextReader reader = new StreamReader(in_xml_file_name))
			{
				retval = ParseXMLStream(in_start_path, reader);
			}
			
			return retval;
		}

		/// <summary>
		/// Parses XML as a stream
		/// </summary>
		/// <param name="in_xml"></param>
		/// <returns></returns>
		public bool ParseXMLStream(string in_start_path, TextReader in_xml_stream)
		{
			bool retval = true;

			try
			{
				// initialize
				Clear();

				// parse XML file
				XPathDocument document = new XPathDocument(in_xml_stream);
				XPathNavigator navigator = document.CreateNavigator();

#if false
				XPathExpression xpath_expression = navigator.Compile("/sn:Settings");
				XmlNamespaceManager namespace_manager = new XmlNamespaceManager(new NameTable());
				namespace_manager.AddNamespace("xmlns", "http://cygnusuav.hu/ConfigurationXML.xsd");
				xpath_expression.SetContext(namespace_manager);
#endif
				XPathNodeIterator nodes;

				// parse root element
				nodes = navigator.Select("/*");
				ParseBase(nodes.Current, in_xml_stream);

				// parse elements
				nodes = navigator.Select(in_start_path);
				while (nodes.MoveNext())
					ParseElement(nodes.Current, in_xml_stream, m_root_class);
			}
			catch (XmlException exception)
			{
				m_error_message = exception.Message;

				m_error_line = exception.LineNumber;
				m_error_col = exception.LinePosition - 1;

				retval = false;
			}
			catch (XMLParserException exception)
			{
				m_error_message = exception.ErrorMessage;
				m_error_line = exception.ErrorLine;
				m_error_col = exception.ErrorColumn;

				retval = false;
			}

			return retval;
		}

		/// <summary>
		/// Pases XML as a string
		/// </summary>
		/// <param name="in_xml_string"></param>
		/// <returns></returns>
		public bool ParseXMLString(string in_start_path, string in_xml_string)
		{
			bool retval = true;

			// parse XML file
			using (StringReader stream = new StringReader(in_xml_string))
			{
				retval = ParseXMLStream(in_start_path,stream);
			}

			return retval;
		}

		/// <summary>
		/// Parses child element of the given node
		/// </summary>
		/// <param name="in_parent">Element with child nodes to parse</param>
		/// <param name="in_xml_stream">XML reader stream</param>
		public void ParseXMLChildNodes(XPathNavigator in_parent, TextReader in_xml_stream, object in_parent_class)
		{
			if (in_parent.MoveToFirstChild())
			{
				do
				{
					ParseElement(in_parent, in_xml_stream, in_parent_class);
				} while (in_parent.MoveToNext());
			}
		}

		/// <summary>
		/// Creates XML parsing exception
		/// </summary>
		/// <param name="in_message">Error message</param>
		/// <param name="in_element">XPathNavigator for error position</param>
		/// <returns></returns>
		public static XmlException CreateXMLParseException(string in_message, XPathNavigator in_element)
		{
			IXmlLineInfo info = (IXmlLineInfo)in_element;
			XmlException exception = new XmlException(in_message, null, info.LineNumber, info.LinePosition);

			return exception;
		}
#endregion
	}
}
