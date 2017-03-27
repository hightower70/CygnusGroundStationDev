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
// Exceptions used by parser
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Xml;
using System.Xml.XPath;

namespace CommonClassLibrary.XMLParser
{
	public class XMLParserException : Exception
	{
		#region · Data mebers ·
		public string ErrorMessage;
    public int ErrorLine;
    public int ErrorColumn;
		#endregion

		#region · Public members ·

		public XMLParserException(XPathNavigator in_navigator)
    {
      IXmlLineInfo line_info = (IXmlLineInfo)in_navigator;

      ErrorLine = line_info.LineNumber;
      ErrorColumn = line_info.LinePosition;
    }

    public XMLParserException(IXmlLineInfo in_line_info)
    {
      ErrorLine = in_line_info.LineNumber;
      ErrorColumn = in_line_info.LinePosition;
    }

    public XMLParserException(int in_line_number, int in_column_number)
    {
      ErrorLine = in_line_number;
      ErrorColumn = in_column_number;
    }

    public XMLParserException(string in_error_message, int in_line_number, int in_column_number)
    {
      ErrorMessage = in_error_message;
      ErrorLine = in_line_number;
      ErrorColumn = in_column_number;
    }

    public void SetAttributeNotFoundError(string in_attrbibute_name)
    {
      ErrorMessage = "Attribute '" + in_attrbibute_name + "' must be defined";
    }

	  public void SetInvalidAttributeValue(string in_attribute_name)
    {
      ErrorMessage = "Invalid attribute value for '" + in_attribute_name + "'";
    }

    public void SetNameAlreadyDefinedError(string in_name)
    {
      ErrorMessage = "Name already exists. (" + in_name + ")";
    }

    public void SetInvalidElementError(string in_name)
    {
      ErrorMessage = "Unknown element '" + in_name + "'";
    }

    public void SetFileNotFoundError(string in_name)
    {
      ErrorMessage = "File not found: " + in_name;
		}

		public void SetStringIsTooLongError(string in_name, int in_max_length)
		{
			ErrorMessage = "String '" + in_name + "' is longer than maximum (" + in_max_length.ToString() + " character)";
		}

		public void SetInvalidTypeError(string in_name)
		{
			ErrorMessage = "Element type is invalid for '" + in_name + "'";
		}

		public void SetOutOfRangeError()
		{
			ErrorMessage = "Value is out of range";
		}
		#endregion
	}
}
