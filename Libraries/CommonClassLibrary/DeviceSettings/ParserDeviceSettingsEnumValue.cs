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
// Value storage/parser for enumerated type
///////////////////////////////////////////////////////////////////////////////
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingsEnumValue
	{
		#region · Data members ·
		private string m_id;
		private string m_value;
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
		/// Gets enum value
		/// </summary>
		public string Value
		{
			get { return m_value; }
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

			// get value
			m_value = in_element.Value;
		}

		#endregion
	}
}
