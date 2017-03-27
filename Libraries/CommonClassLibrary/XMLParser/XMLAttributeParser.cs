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
// Parser routines for XML elements attributes
///////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace CommonClassLibrary.XMLParser
{
  public class XMLAttributeParser
  {
    #region · Types ·
    // Attribute type
    public const int atObligatory = 0x01;
		public const int atOptional = 0x00;

		// conversion table entry for enum conversion
		public class EnumConversionTableEntry
		{
			public string Key;		// text value of the enum type attribute
			public object Value;	// attribute value in c#
		}


    #endregion

    #region · Parser functions ·

		static public string ConvertAttributeToString(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
		{
			string attribute = in_element.GetAttribute(in_attribute_name, "");

			if (string.IsNullOrEmpty(attribute) && (in_attribute_type & atObligatory) != 0)
			{
				IXmlLineInfo info = (IXmlLineInfo)in_element;
				XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
				exception.SetAttributeNotFoundError(in_attribute_name);
				throw exception;
			}
			else
			{
				return attribute;
			}
		}

		static public double ConvertAttributeToDouble(XPathNavigator in_element, string in_attribute_name, int in_attribute_type, double in_default_value)
		{
			string value = in_element.GetAttribute(in_attribute_name, "");

			// check if attribute exists
			if (!string.IsNullOrEmpty(value))
			{
				// convert string to double
				double double_buffer = 0;
				if (double.TryParse(value.Trim(), out double_buffer))
				{
					// value is valid
					return double_buffer;
				}
				else
				{
					// throw an exception if value is invalid
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetInvalidAttributeValue(in_attribute_name);
					throw exception;
				}
			}
			else
			{
				// if attribute is obligatory throw an exception
				if ((in_attribute_type & atObligatory) != 0)
				{
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetAttributeNotFoundError(in_attribute_name);
					throw exception;
				}
			}

			return in_default_value;
		}

		/// <summary>
		/// Converts attribute value to int and sets the value of the given property. Or if the attributte value is variable name, store in the dependency list.
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_attribute_name"></param>
		/// <param name="in_attribute_type"></param>
		static public int ConvertAttributeToInt(XPathNavigator in_element, string in_attribute_name, int in_attribute_type, int in_default_value)
		{
			string value = in_element.GetAttribute(in_attribute_name, "");

			// check if attribute exists
			if (!string.IsNullOrEmpty(value))
			{
				// convert string to int
				int int_buffer = 0;
				if (int.TryParse(value.Trim(), out int_buffer))
				{
					return int_buffer;
				}
				else
				{
					// throw an exception if value is invalid
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetInvalidAttributeValue(in_attribute_name);
					throw exception;
				}
			}
			else
			{
				// if attribute is obligatory throw an exception
				if ((in_attribute_type & atObligatory) != 0)
				{
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetAttributeNotFoundError(in_attribute_name);
					throw exception;
				}
			}

			return in_default_value;
		}

#if false
    /// <summary>
    /// Convert a value of the specified attribute to a string.
    /// </summary>
    /// <param name="in_element"></param>
    /// <param name="in_attribute_name"></param>
    /// <param name="in_attribute_type"></param>
    static public void ConvertAttributeToStringProperty(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
    {
      string value = in_element.GetAttribute(in_attribute_name, "");

      // check if attribute exists
      if (!string.IsNullOrEmpty(value))
      {
        Type class_type = this.GetType();
        PropertyInfo property_info = class_type.GetProperty(in_attribute_name);

        property_info.SetValue(this, value.Trim(), null);
      }
      else
      {
        // if attribute is obligatory throw an exception
        if ((in_attribute_type & atObligatory) != 0)
        {
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetAttributeNotFoundError(in_attribute_name);
          throw exception;
        }
      }
    }

    /// <summary>
    /// Converts attribute value to int and sets the value of the given property. Or if the attributte value is variable name, store in the dependency list.
    /// </summary>
    /// <param name="in_element"></param>
    /// <param name="in_attribute_name"></param>
    /// <param name="in_attribute_type"></param>
    static public void ConvertAttributeToIntProperty(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
    {
      string value = in_element.GetAttribute(in_attribute_name, "");

      // check if attribute exists
      if (!string.IsNullOrEmpty(value))
      {
        // get property information
        Type class_type = this.GetType();
        PropertyInfo property_info = class_type.GetProperty(in_attribute_name);

        // convert string to int
        int int_buffer = 0;
        if (int.TryParse(value.Trim(), out int_buffer))
        {
          property_info.SetValue(this, int_buffer, null);
        }
        else
        {
          // throw an exception if value is invalid
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetInvalidAttributeValue(in_attribute_name);
          throw exception;
        }
      }
      else
      {
        // if attribute is obligatory throw an exception
        if ((in_attribute_type & atObligatory) != 0)
        {
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetAttributeNotFoundError(in_attribute_name);
          throw exception;
        }
      }
    }

    /// <summary>
    /// Converts attribute value to double and sets the value of the given property. Or if the attributte value is variable name, store in the dependency list.
    /// </summary>
    /// <param name="in_element"></param>
    /// <param name="in_attribute_name"></param>
    /// <param name="inout_success"></param>
    internal void ConvertAttributeToDoubleProperty(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
    {
      string value = in_element.GetAttribute(in_attribute_name, "");

      // check if attribute exists
      if (!string.IsNullOrEmpty(value))
      {
        // get property information
        Type class_type = this.GetType();
        PropertyInfo property_info = class_type.GetProperty(in_attribute_name);

        // convert string to double
        double double_buffer = 0;
        if (double.TryParse(value.Trim(), out double_buffer))
        {
          property_info.SetValue(this, double_buffer, null);
        }
        else
        {
          // throw an exception if value is invalid
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetInvalidAttributeValue(in_attribute_name);
          throw exception;
        }
      }
      else
      {
        // if attribute is obligatory throw an exception
        if ((in_attribute_type & atObligatory) != 0)
        {
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetAttributeNotFoundError(in_attribute_name);
          throw exception;
        }
      }
    }

    /// <summary>
    /// Converts attribute to a bool peroperty
    /// </summary>
    /// <param name="in_element"></param>
    /// <param name="in_attribute_name"></param>
    /// <param name="inout_success"></param>
    internal void ConvertAttributeToBoolProperty(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
    {
      string value = in_element.GetAttribute(in_attribute_name, "");

      // check if attribute exists
      if (!string.IsNullOrEmpty(value))
      {
        // get property information
        Type class_type = this.GetType();
        PropertyInfo property_info = class_type.GetProperty(in_attribute_name);

        // convert string to bool
        bool bool_buffer = false;
        if (bool.TryParse(value.Trim(), out bool_buffer))
        {
          property_info.SetValue(this, bool_buffer, null);
        }
        else
        {
          // throw an exception if value is invalid
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetInvalidAttributeValue(in_attribute_name);
          throw exception;
        }
      }
      else
      {
        // if attribute is obligatory throw an exception
        if ((in_attribute_type & atObligatory) != 0)
        {
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetAttributeNotFoundError(in_attribute_name);
          throw exception;
        }
      }
    }

    /// <summary>
    /// Convert a value of the specified attribute to a string.
    /// </summary>
    /// <param name="in_element"></param>
    /// <param name="in_attribute_name"></param>
    /// <param name="in_attribute_type"></param>
    internal void ConvertAttributeToEnumProperty(XPathNavigator in_element, string in_attribute_name, int in_attribute_type = 0)
    {
      string value = in_element.GetAttribute(in_attribute_name, "");

      // check if attribute exists
      if (!string.IsNullOrEmpty(value))
      {
        Type class_type = this.GetType();
        PropertyInfo property_info = class_type.GetProperty(in_attribute_name);

        // convert string to enum
        try
        {
          var enum_buffer = Enum.Parse(property_info.PropertyType, value, true);

          property_info.SetValue(this, enum_buffer, null);
        }
        catch
        {
          // throw an exception if value is invalid
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetInvalidAttributeValue(in_attribute_name);
          throw exception;
        }
      }
      else
      {
        // if attribute is obligatory throw an exception
        if ((in_attribute_type & atObligatory) != 0)
        {
          IXmlLineInfo info = (IXmlLineInfo)in_element;
          ParserException exception = new ParserException(info.LineNumber, info.LinePosition);
          exception.SetAttributeNotFoundError(in_attribute_name);
          throw exception;
        }
      }
    }
#endif
		/// <summary>
		/// Coverts XML attribute value to integer using a default value 
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_attribute_name"></param>
		/// <param name="in_attribute_type"></param>
		/// <param name="in_default_value"></param>
		/// <returns></returns>
		static public int ConvertAttributeToIntDefault(XPathNavigator in_element, string in_attribute_name, int in_attribute_type, int in_default_value)
		{
			string attribute = in_element.GetAttribute(in_attribute_name, "");

			// check if attribute exists
			if (!string.IsNullOrEmpty(attribute))
			{
				// convert string to int
				int int_buffer = 0;
				if (int.TryParse(attribute.Trim(), out int_buffer))
				{
					return int_buffer;
				}
				else
				{
					// throw an exception if value is invalid
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetInvalidAttributeValue(in_attribute_name);
					throw exception;
				}
			}
			else
			{
				// if attribute is obligatory throw an exception
				if ((in_attribute_type & atObligatory) != 0)
				{
					IXmlLineInfo info = (IXmlLineInfo)in_element;
					XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
					exception.SetAttributeNotFoundError(in_attribute_name);
					throw exception;
				}
				else
				{
					return in_default_value;
				}
			}
		}

		/// <summary>
		/// Coverts XML attribute value to integer using a default value 
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_attribute_name"></param>
		/// <param name="in_attribute_type"></param>
		/// <param name="in_default_value"></param>
		/// <returns></returns>
		static public int ConvertAttributeToInt(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
		{
			string attribute = in_element.GetAttribute(in_attribute_name, "");

			// check if attribute exists
			if (string.IsNullOrEmpty(attribute) && (in_attribute_type & atObligatory) != 0)
			{
				IXmlLineInfo info = (IXmlLineInfo)in_element;
				XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
				exception.SetAttributeNotFoundError(in_attribute_name);
				throw exception;
			}
			else
			{
				// convert string to int
				int int_buffer = 0;
				int_buffer = int.Parse(attribute.Trim());

				return int_buffer;
			}
		}

		/// <summary>
		/// Class for defining enum attribute type conversion table
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class EnumConversionTable<T> : Dictionary<string, T>
		{
			/// <summary>
			/// Converts string (attribute value) to enum or any object
			/// </summary>
			/// <param name="in_element"></param>
			/// <param name="in_attribute_name"></param>
			/// <param name="in_attribute_type"></param>
			/// <returns></returns>
			public T ConvertAttributeToEnum(XPathNavigator in_element, string in_attribute_name, int in_attribute_type)
			{
				string attribute = in_element.GetAttribute(in_attribute_name, "");

				// check if attribute exists
				if (!string.IsNullOrEmpty(attribute))
				{
					// convert to enum
					try
					{
						return this[attribute];
					}
					catch
					{
						// throw an exception if attribute value is invalid
						IXmlLineInfo info = (IXmlLineInfo)in_element;
						XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
						exception.SetInvalidAttributeValue(in_attribute_name);
						throw exception;
					}
				}
				else
				{
					// if attribute is obligatory throw an exception
					if ((in_attribute_type & atObligatory) != 0)
					{
						IXmlLineInfo info = (IXmlLineInfo)in_element;
						XMLParserException exception = new XMLParserException(info.LineNumber, info.LinePosition);
						exception.SetAttributeNotFoundError(in_attribute_name);
						throw exception;
					}
					else
					{
						return default(T);
					}
				}
			}
		}

		#endregion
  }
}	
