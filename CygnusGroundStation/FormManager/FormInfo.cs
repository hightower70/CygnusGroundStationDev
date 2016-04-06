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
// Form information class
///////////////////////////////////////////////////////////////////////////////

namespace CygnusGroundStation
{
	public class FormInfo
	{
		#region · Properties ·

		public string FormName { get; set; }
		public string FormPath { get; set; }

		#endregion

		#region · Member functions ·
					 
		public override bool Equals(object in_object)
		{
			// If this and obj do not refer to the same type, then they are not equal.
			if (in_object.GetType() != this.GetType())
				return false;

			return FormPath == ((FormInfo)in_object).FormPath;
		}

		public override int GetHashCode()
		{
			return FormPath.GetHashCode();
		}

		#endregion
	}
}