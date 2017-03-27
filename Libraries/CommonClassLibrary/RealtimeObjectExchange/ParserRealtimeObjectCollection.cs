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
// Collection storage for realtime objects
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.XMLParser;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObjectCollection
	{
		#region · Data members ·
		private string m_name;

		private List<ParserRealtimeObject> m_objects = new List<ParserRealtimeObject>();
		private Dictionary<string, int> m_object_name_lookup = new Dictionary<string, int>();

		private List<IValueConverter> m_value_converter = new List<IValueConverter>();
		private Dictionary<string, int> m_value_converter_name_lookup = new Dictionary<string, int>();

		private static ParserRealtimeObjectCollection m_default;

		#endregion

		#region · Singleton members ·

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static ParserRealtimeObjectCollection Default
		{
			get
			{
				if (m_default == null)
				{
					m_default = new ParserRealtimeObjectCollection();
					m_default.m_name = "Default";
				}

				return m_default;
			}
		}
		#endregion

		#region · Properties ·
		/// <summary>
		/// Name of the collection
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Get realtime objects
		/// </summary>
		public List<ParserRealtimeObject> Objects
		{
			get { return m_objects; }
		}

		#endregion

		#region · Parser functions ·

		/// <summary>
		/// Clears collection
		/// </summary>
		public void Clear()
		{
			m_objects.Clear();
			m_object_name_lookup.Clear();

			m_value_converter.Clear();
			m_value_converter_name_lookup.Clear();
		}

		/// <summary>
		/// Adds object to the collection
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_object"></param>
		public void AddObject(XPathNavigator in_element, ParserRealtimeObject in_object)
		{
			m_objects.Add(in_object);
			m_object_name_lookup.Add(in_object.Name, m_objects.Count - 1);
		}

		/// <summary>
		/// Adds converter to the collection
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_converter"></param>
		public void AddConverter(XPathNavigator in_element, IValueConverter in_converter)
		{
			m_value_converter.Add(in_converter);
			m_value_converter_name_lookup.Add(in_converter.Name, m_value_converter.Count - 1);
		}

		/// <summary>
		/// Parses base class
		/// </summary>
		/// <param name="in_element"></param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atOptional);
		}

		public void AppendCollection(ParserRealtimeObjectCollection in_collection)
		{

		}
		#endregion
	}
}
