using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace CygnusControls
{
	/// <summary>
	/// Item for string selector
	/// </summary>
	public class StringSelectorItem
	{
		public StringSelectorItem()
		{
			Value = null;
			String = null;
			IsDefault = false;
		}

		public object Value { get; set; }
		public string String { get; set; }
		public bool IsDefault { set; get; }
	}

	/// <summary>
	/// Converts object enumaration into string
	/// </summary>
	[TypeConverter(typeof(StringSelectorConverter))]
	[ValueConversion(typeof(object), typeof(string))]
	public class StringSelectorConverter : IValueConverter
	{
		#region · Data members ·
		private List<StringSelectorItem> m_items = new List<StringSelectorItem>();
		private Dictionary<object, int> m_item_lookup = null;
		private StringSelectorItem m_default_item = null;
		private string m_field_name = "";
		private Type m_value_type = null;
		#endregion

		#region · Properties ·

		/// <summary>
		/// Field name
		/// </summary>
		public string FieldName
		{
			get { return m_field_name; }
			set { m_field_name = value; }
		}

		/// <summary>
		/// Templates collection
		/// </summary>
		public List<StringSelectorItem> Items
		{
			get { return m_items; }
		}
		#endregion

		#region · Converter function ·
		public object Convert(object in_value, Type in_target_type, object in_parameter, System.Globalization.CultureInfo culture)
		{
			// create lookuptable if needed
			if (m_item_lookup == null)
				UpdateLookupTable();

			if (in_value == null)
				return null;

			string retval = "";

			object value = in_value;

			// convert value to selector type
			if ( m_value_type != null && value.GetType() != m_value_type)
			{
				value = System.Convert.ChangeType(value, m_value_type, CultureInfo.InvariantCulture);
			}

			// lookup template
			if (m_item_lookup.ContainsKey(value))
			{
				retval = m_items[m_item_lookup[value]].String;
			}
			else
			{
				if (m_default_item != null)
				{
					retval = m_default_item.String;
				}
			}

			return retval;
		}
		#endregion

		#region · Convert back (non implemented) ·
		public object ConvertBack(object in_value, Type in_targetType, object in_parameter, System.Globalization.CultureInfo in_culture)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region · Non-public members ·
		/// <summary>
		/// Updates lookuptable
		/// </summary>
		private void UpdateLookupTable()
		{
			m_item_lookup = new Dictionary<object, int>();

			if (m_items.Count == 0)
				return;

			// get type 
			if (m_value_type == null)
				m_value_type = m_items[0].Value.GetType();

			// update lookup
			for (int i = 0; i < m_items.Count; i++)
			{
				if (m_items[i].IsDefault)
				{
					m_default_item = m_items[i];
				}
				else
				{
					object key = System.Convert.ChangeType(m_items[i].Value, m_value_type, CultureInfo.InvariantCulture);

					m_item_lookup.Add(key, i);
				}
			}
		}
		#endregion
	}
}
