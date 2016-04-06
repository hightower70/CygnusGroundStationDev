using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace CygnusControls
{
	public class RealtimieDataProvider : DynamicObject, INotifyPropertyChanged, IDisposable
	{
		#region · Data members ·
		Dictionary<string, int> m_members = new Dictionary<string, int>();
		private float[] m_member_values;
		private volatile bool m_disposed = false;
		#endregion

		#region · IDisposable functions ·

		/// <summary>
		/// Public implementation of Dispose pattern callable by consumers.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Protected implementation of Dispose pattern.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool in_disposing)
		{
			if (m_disposed)
				return;

			if (in_disposing)
			{

			}

			m_disposed = true;
		}
		#endregion

		#region · Constructor ·

		public RealtimieDataProvider()
		{

		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/sets properties
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public float this[string in_name]
		{
			get { return m_member_values[m_members[in_name]]; }
			set	{	m_member_values[m_members[in_name]] = value; }
		}

		/// <summary>
		/// Gets member value by index
		/// </summary>
		/// <param name="in_index">Index of the member</param>
		/// <returns></returns>
		public float this[int in_index]
		{
			get { return m_member_values[in_index]; }
			set { m_member_values[in_index] = value; }
		}

		#endregion

		#region · DynamicObject ·

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!m_members.ContainsKey(binder.Name))
			{
				result = null;
				return false;
			}

			result = m_member_values[m_members[binder.Name]];

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (m_members.ContainsKey(binder.Name))
			{
				m_member_values[m_members[binder.Name]] = (float)value;

				OnPropertyChanged(binder.Name);

				return true;
			}
			else
				return false;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return m_members.Keys;
		}

		public override bool TryDeleteMember(DeleteMemberBinder binder)
		{
			return false;
		}

		#endregion

		#region · INotifyPropertyChanged ·

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null && !string.IsNullOrEmpty(propertyName))
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion INotifyPropertyChanged

		#region · Public members ·

		#endregion

		public void UpdateMember(string in_name, object in_value)
		{
			//DynamicMemberInfo member = m_members[in_name];

			//UpdateMember(ref member, in_value);

			//m_members[in_name] = member;

			//OnPropertyChanged(in_name);
		}
	}
}


