///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2014 Laszlo Arvai. All rights reserved.
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
// Bindable data source for RealtimeObjects
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Windows;

namespace CygnusControls
{
	public class RealtimeObjectProvider : DynamicObject, INotifyPropertyChanged, IRealtimeObjectRefresher, IRealtimeObjectUpdateNotifier, IRealtimeObjectProvider
	{
		#region · Data members ·
		private volatile bool m_object_updated;
		private string m_object_name;
		private int m_object_index;
		Dictionary<string, int> m_member_lookup;
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constuctor
		/// </summary>
		public RealtimeObjectProvider()
		{
			m_member_lookup = new Dictionary<string, int>();
			m_object_updated = false;
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Name of the realtime object to this class is assigned
		/// </summary>
		[Description("Name of the realtime object"), Category("Other"), DefaultValue(""), Browsable(true)]
		public string RealtimeObjectName
		{
			get { return m_object_name; }
			set
			{
				m_object_name = value;
				m_object_index = RealtimeObjectStorage.Default.ObjectGetIndex(m_object_name);
			}
		}

		/// <summary>
		/// Gets/sets properties
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public object this[string in_name]
		{
			get
			{
				if(m_member_lookup.ContainsKey(in_name))
				{
					// member already exists in the cache
					int member_index = m_member_lookup[in_name];

					return RealtimeObjectStorage.Default[m_object_index].MemberRead(member_index);
				}
				else
				{
					// store index in the cache
					int member_index = RealtimeObjectStorage.Default[m_object_index].MemberGetIndex(in_name);

					m_member_lookup.Add(in_name, member_index);

					return RealtimeObjectStorage.Default[m_object_index].MemberRead(member_index);
				}
			}
			set
			{
				// TODO m_member_values[m_members[in_name]] = value;
			}
		}

		/// <summary>
		/// Gets member value by index
		/// </summary>
		/// <param name="in_index">Index of the member</param>
		/// <returns></returns>
		public object this[int in_index]
		{
			get { return null; /* m_member_values[in_index];*/ }
			set { /*m_member_values[in_index] = value;*/ }
		}

		#endregion

		#region · DynamicObject ·

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (m_member_lookup.ContainsKey(binder.Name))
			{
				// member already exists in the cache
				int member_index = m_member_lookup[binder.Name];

				result = RealtimeObjectStorage.Default[m_object_index].MemberRead(member_index);
			}
			else
			{
				// store index in the cache
				int member_index = RealtimeObjectStorage.Default[m_object_index].MemberGetIndex(binder.Name);

				m_member_lookup.Add(binder.Name, member_index);

				result = RealtimeObjectStorage.Default[m_object_index].MemberRead(member_index);
			}

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			/*
			if (m_members.ContainsKey(binder.Name))
			{
				m_member_values[m_members[binder.Name]] = (float)value;

				OnPropertyChanged(binder.Name);

				return true;
			}
			else*/
				return false;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return RealtimeObjectStorage.Default[m_object_index].MemberGetList();
		}

		public override bool TryDeleteMember(DeleteMemberBinder binder)
		{
			return false;
		}

		#endregion

		#region · IRealtimeObjectRefresher ·
		public void RefreshObject()
		{
			if (!m_object_updated)
				return;

			m_object_updated = false;

			foreach(string member in m_member_lookup.Keys)
			{
				OnPropertyChanged(member);
			}
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

		#region · IRealtimeObjectUpdateNotifier ·

		public void ObjectUpdated()
		{
			m_object_updated = true;
		}

		#endregion

		#region · IRealtimeObjectProvider ·

		public void Register(FrameworkElement in_parent, string in_resource_key)
		{
			FormManager.Default.RealtimeObjectRefresherAdd(this);
			RealtimeObjectStorage.Default[m_object_index].UpdateNotifierAdd(this);
		}

		public void Deregister(FrameworkElement in_parent)
		{
			RealtimeObjectStorage.Default[m_object_index].UpdateNotifierRemove(this);
			FormManager.Default.RealtimeObjectRefresherRemove(this);
			
		}

		#endregion

		#region · Public members ·


		#endregion
	}
}


