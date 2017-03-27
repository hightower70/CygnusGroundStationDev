///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Laszlo Arvai. All rights reserved.
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
// Occupancy Grid main thread
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.DeviceCommunication;
using System;
using System.Threading;

namespace OccupancyGrid
{
	class OccupancyGridThread
	{
		#region · Types ·
		/// <summary>
		/// Current state of the thread
		/// </summary>
		enum CommunicationState
		{
			Uninitialized,
			Initializing,
			Initialized
		};
		#endregion

		#region · Private data ·
		private bool m_is_disposed;

		// thread variables
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_thread_event;
		private Thread m_thread;

		// communication variable
		private CommunicationState m_state;
		private DateTime m_packet_send_timestamp;
		private int m_retry_counter;

		//private OccupancyGridSettings m_settings;
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public OccupancyGridThread()
		{
			// init members
			m_stop_requested = false;
			m_thread_stopped = new ManualResetEvent(false);
			m_thread_event = new AutoResetEvent(false);
			m_thread = null;
			m_state = CommunicationState.Uninitialized;
		}

		#endregion

		#region · Public members ·

		/// <summary>
		/// Sets up this interface module
		/// </summary>
		/*public void Configure(OccupancyGridSettings in_settings)
		{
			m_settings = in_settings;
		}*/

		/// <summary>
		/// Starts module operation
		/// </summary>
		public void Open()
		{
			// reset events
			m_stop_requested = false;
			m_thread_stopped.Reset();
			m_thread_event.Reset();

			// create worker thread instance
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.Name = "Occupancy Grid Thread";   // looks nice in Output window
			m_thread.Start();
		}

		/// <summary>
		/// Stops module operation
		/// </summary>
		public void Close()
		{
			m_stop_requested = true;

			if (m_thread != null && m_thread.IsAlive)
			{
				// wait when thread  will stop or finish
				while (m_thread.IsAlive)
				{
					// We cannot use here infinite wait because our thread
					// makes syncronous calls to main form, this will cause deadlock.
					// Instead of this we wait for event some appropriate time
					// (and by the way give time to worker thread) and
					// process events. These events may contain Invoke calls.
					if (WaitHandle.WaitAll((new ManualResetEvent[] { m_thread_stopped }), 100, true))
					{
						break;
					}

					Thread.Sleep(100);
				}
			}
		}

		#endregion

		#region · Thread function ·

		/// <summary>
		/// Thread function
		/// </summary>
		private void Run()
		{
			bool event_occured;

			while (!m_stop_requested)
			{
				// wait for event
				event_occured = m_thread_event.WaitOne(100);

				// exit loop if thread must be stopped
				if (m_stop_requested)
					break;

				switch(m_state)
				{
					case CommunicationState.Uninitialized:
						//CommunicationManager.Default.SendFileFinishRequest
						break;
				}
			}

			// thread is finished
			m_thread_stopped.Set();
		}

		#endregion

		#region · IDisposable Members ·

		/// <summary>
		/// Releases all resources
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases all resources
		/// </summary>
		/// <param name="disposing">Indicates wheter the managed resources should be released as well</param>
		private void Dispose(bool disposing)
		{
			if (m_is_disposed)
				return;

			m_is_disposed = true;

			Close();
		}

		#endregion
	}
}

