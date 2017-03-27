using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CygnusControls
{
	public class WaitCursor : IDisposable
	{
		#region · Data members ·
		private Cursor m_previous_cursor;
		#endregion

		#region · Constructor ·
		public WaitCursor()
		{
			m_previous_cursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.Wait;
		}
		#endregion

		#region · IDisposable Members ·

		public void Dispose()
		{
			Mouse.OverrideCursor = m_previous_cursor;
		}

		#endregion
	}
}
