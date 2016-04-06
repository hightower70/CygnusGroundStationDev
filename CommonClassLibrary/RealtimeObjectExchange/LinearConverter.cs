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
// Realtime Object Exchange - Linear value converter class
///////////////////////////////////////////////////////////////////////////////
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class LinearConverter : IValueConverter
	{
		#region · Data members ·
		private string m_name;
		private double m_multiplier = 1;
		#endregion

		#region · Properties ·

		/// <summary>
		/// Name of the linear converter
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Converts value
		/// </summary>
		/// <param name="in_value"></param>
		/// <returns></returns>
		public double Convert(double in_value)
		{
			return in_value * m_multiplier;
		}
		#endregion

		#region · Parser function ·

		/// <summary>
		/// Parses XML file
		/// </summary>
		/// <param name="in_element"></param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			m_multiplier = XMLAttributeParser.ConvertAttributeToDouble(in_element, "Multiplier", XMLAttributeParser.atOptional, 1);
		}
		#endregion
	}
}

