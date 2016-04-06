using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClassLibrary.DeviceCommunication
{
	public class SLIP
	{
		#region · Constants ·

		// SLIP constants
		private const byte SLIP_END = 0xc0;
		private const byte SLIP_ESC = 0xdb;
		private const byte SLIP_ESC_END = 0xdc;
		private const byte SLIP_ESC_ESC = 0xdd;

		private enum DecoderStatus
		{
			NoPacket,
			Packet,
			Escaped
		};

		[Flags]
		public enum EncoderFlags
		{
			StorePacketStart = 1,
			StorePacketEnd = 2
		}						  


		#endregion

		#region · Data members ·
		private DecoderStatus m_decoder_status = DecoderStatus.NoPacket;
		#endregion

		#region · Public members ·


		/// <summary>
		/// Enclodes block of bytes into SLIP enoded data
		/// </summary>
		/// <param name="in_source_buffer"></param>
		/// <param name="in_source_pos"></param>
		/// <param name="in_source_length"></param>
		/// <param name="in_destination_buffer"></param>
		/// <param name="in_destination_pos"></param>
		public static void EncodeBlock(byte[] in_source_buffer, ref int in_source_pos, int in_source_length, byte[] in_destination_buffer, ref int in_destination_pos, EncoderFlags in_flags)
		{
			byte data;
			int destination_length = in_destination_buffer.Length;

			// store packet start
			if ((in_flags & EncoderFlags.StorePacketStart) == EncoderFlags.StorePacketStart && in_destination_pos < destination_length)
			{
				in_destination_buffer[in_destination_pos++] = SLIP_END;
			}

			// store packet data
			while (in_source_pos < in_source_length && in_destination_pos < destination_length)
			{
				// get data
				data = in_source_buffer[in_source_pos++];
				// check to see if is a special character
				switch (data)
				{
					case SLIP_END:
						in_destination_buffer[in_destination_pos++] = SLIP_ESC; // escape special character
						in_destination_buffer[in_destination_pos++] = SLIP_ESC_END;
						break;

					case SLIP_ESC:
						in_destination_buffer[in_destination_pos++] = SLIP_ESC; // escape special character
						in_destination_buffer[in_destination_pos++] = SLIP_ESC_ESC;
						break;

					// send raw character
					default:
						in_destination_buffer[in_destination_pos++] = data;
						break;
				}
			}

			// store packet end
			if ((in_flags & EncoderFlags.StorePacketEnd) == EncoderFlags.StorePacketEnd && in_destination_pos < destination_length)
			{
				in_destination_buffer[in_destination_pos++] = SLIP_END;
			}
		}

		/// <summary>
		/// Decodes SLIP ecoded blick
		/// </summary>
		/// <param name="in_source_buffer"></param>
		/// <param name="in_source_pos"></param>
		/// <param name="in_source_length"></param>
		/// <param name="in_destination_buffer"></param>
		/// <param name="in_destination_pos"></param>
		/// <returns></returns>
		public bool DecodeBlock(byte[] in_source_buffer, ref int in_source_pos,int in_source_length, byte[] in_destination_buffer, ref int in_destination_pos)
		{
			byte data;
			int destination_length = in_destination_buffer.Length;

			while (in_source_pos < in_source_length && in_destination_pos < destination_length)
			{
				// get data
				data = in_source_buffer[in_source_pos++];

				switch (m_decoder_status)
				{
					case DecoderStatus.NoPacket:
						if (data == SLIP_END)
						{
							m_decoder_status = DecoderStatus.Packet;
						}
						break;

					case DecoderStatus.Packet:
						if (data == SLIP_ESC)
						{
							// escaped data
							m_decoder_status = DecoderStatus.Escaped;
						}
						else
						{
							if (data == SLIP_END)
							{
								// packet end found
								m_decoder_status = DecoderStatus.NoPacket;

								return true;
							}
							else
							{
								// store if not escaped
								in_destination_buffer[in_destination_pos++] = data;
							}
						}
						break;

					case DecoderStatus.Escaped:
						// process escaped data
						switch (data)
						{
							case SLIP_ESC_END:
								data = SLIP_END;
								m_decoder_status = DecoderStatus.Packet;
								break;

							case SLIP_ESC_ESC:
								data = SLIP_ESC;
								m_decoder_status = DecoderStatus.Packet;
								break;
						}

						// store data
						in_destination_buffer[in_destination_pos++] = data;
						break;
				}
			}

			return false;
		}

		/// <summary>
		/// resets decoder
		/// </summary>
		public void ResetDecoder()
		{
			m_decoder_status = DecoderStatus.NoPacket;
		}

		#endregion
	}
}

#if false
using System;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.Generic;

namespace FlowNaviMonitor
{
	public class FlowNaviThread
	{
		#region Constants


		// SLIP constants
		private const byte SLIP_END = 0xc0;
		private const byte SLIP_ESC = 0xdb;
		private const byte SLIP_ESC_END = 0xdc;
		private const byte SLIP_ESC_ESC = 0xdd;

		#endregion

		#region Types

		public enum CommunicationMode
		{
			Normal,
			FrameCapture
		}

		public enum IlluminationMode
		{
			Off,
			On,
			Auto
		};

		enum SlipDecoderStatus
		{
			WaitingForPacketStart,
			LengthReceiving,
			PacketTypeReceiving,
			DataReceiving
		};


		public class ComboboxEntry
		{
			public string DisplayName;
			public string Description;
			public string SerialNumber;
			public uint Location;

			public override string ToString()
			{
				return DisplayName;
			}
		}

		//PortType enum
		[Flags]
		public enum PortType : int
		{
				write = 0x1,
				read = 0x2,
				redirected = 0x4,
				net_attached = 0x8
		}


		//struct for PORT_INFO_2
		[StructLayout(LayoutKind.Sequential)]
		public struct PORT_INFO_2
		{
				public string pPortName;
				public string pMonitorName;
				public string pDescription;
				public PortType fPortType;
				internal int Reserved;
		}

		//Win32 API
		[DllImport("winspool.drv", EntryPoint = "EnumPortsA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int EnumPorts(string pName, int Level, IntPtr lpbPorts, int cbBuf, ref int pcbNeeded, ref int pcReturned);

		#endregion

		#region Members

		// Main thread sets this event to stop worker thread:
		bool m_stop_thread;

		// Worker thread sets this event when it is stopped:
		ManualResetEvent m_event_stopped;

		// Reference to main form used to make syncronous user interface calls:
		MainForm m_form;

		// Serial port for FLowNavi communication
		SerialPort m_flownavi_port;

		CommunicationMode m_mode = CommunicationMode.Normal;

		#endregion

		#region Properties
		public CommunicationMode Mode
		{
			get { return m_mode; }
		}
		#endregion

		#region Functions

		public FlowNaviThread(ManualResetEvent eventStopped, MainForm form)
		{
			m_stop_thread = false;
			m_event_stopped = eventStopped;
			m_form = form;
    }

		public void FillDeviceCombo( ComboBox combo )
		{
			// clear device list
			combo.Items.Clear();

			foreach (SerialPortEnumerator.DeviceInfo device_info in SerialPortEnumerator.GetAllCOMPorts())
			{
				ComboboxEntry entry = new ComboboxEntry();

				string port_number = "";
				string port = device_info.name;

				for (int i = 0; i < port.Length; i++)
				{
					if (char.IsNumber(port[i]))
					{
						port_number += port[i];
					}
				}
				uint.TryParse(port_number, out entry.Location);

				entry.DisplayName = device_info.name + " (" + device_info.description + ")";
				entry.Description = "";
				entry.SerialNumber = "";

				combo.Items.Add(entry);
			}


			if (combo.Items.Count > 0)
				combo.SelectedIndex = 0;
    }

    public bool Open(ComboboxEntry entry)
    {
      bool success = false;

			try
      {
        m_flownavi_port = new SerialPort("COM" + entry.Location.ToString(), 115200, Parity.None, 8, StopBits.One);
        m_flownavi_port.ReadTimeout = 50;
        m_flownavi_port.WriteTimeout = 50;
				m_flownavi_port.ReadBufferSize = 4096;
        m_flownavi_port.Open();

        success = true;
      }
      catch
      {
        success = false;
      }

      // display error message
      if (!success)
        MessageBox.Show("Can't open device.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

      return success;
    }


		public void SetMode(CommunicationMode in_mode)
		{
			byte[] command_buffer = new byte[1];

			command_buffer[0] = 0;

			switch(in_mode)
			{
				case CommunicationMode.Normal:
					command_buffer[0] = 0x96;
					break;

				case CommunicationMode.FrameCapture:
					command_buffer[0] = 0xd2;
					break;
			}

			if (command_buffer[0] != 0x00 && m_flownavi_port != null)
			{
				m_mode = in_mode;
				m_flownavi_port.Write(command_buffer, 0, command_buffer.Length);
			}
		}

		public void SetIlluminationMode(IlluminationMode in_mode)
		{
			byte[] command_buffer = new byte[1];

			command_buffer[0] = 0;

			switch(in_mode)
			{
				case IlluminationMode.Off:
					command_buffer[0] = 0x1e;
					break;

				case IlluminationMode.On:
					command_buffer[0] = 0x2d;
					break;
			}

			if (command_buffer[0] != 0x00 && m_flownavi_port != null)
			{
				m_flownavi_port.Write(command_buffer, 0, command_buffer.Length);
			}


		}



		// Read Function runs in worker thread 
    public void Run()
    {
      byte[] read_buffer = new byte[READ_BUFFER_LENGTH];
      byte[] packet_buffer = new byte[PACKET_BUFFER_LENGTH];

			int packet_expected_length;
			int read_length;
			int read_pos;
			int packet_pos;
			byte packet_type;
			bool escape;
			byte data;
			bool data_valid;
			byte checksum;
			SlipDecoderStatus status;
			FlowNaviPacketBuilder packet_builder = new FlowNaviPacketBuilder();
			FlowNaviPacketBuilder.PacketBase packet_to_send;

      // init
			packet_type = 0;
			packet_to_send = null;
			packet_pos = 0;
			packet_expected_length = 0;
			escape = false;
			checksum = 0;
			status = SlipDecoderStatus.WaitingForPacketStart;
			m_stop_thread = false;

      // main loop
			while (!m_stop_thread)
			{
				try
				{
					read_length = m_flownavi_port.Read(read_buffer, 0, READ_BUFFER_LENGTH);

					for (read_pos = 0; read_pos < read_length; read_pos++)
					{
						data = read_buffer[read_pos];

						// packet start
						if (status == SlipDecoderStatus.WaitingForPacketStart)
						{
							if (data == SLIP_END)
							{
								packet_pos = 0;
								packet_expected_length = 0;
								escape = false;
								checksum = 0;
								status = SlipDecoderStatus.LengthReceiving;
							}
						}
						else
						{
							data_valid = false;

							// slip decode data
							if (escape)
							{
								// process escaped data
								switch (data)
								{
									case SLIP_ESC_END:
										data = SLIP_END;
										data_valid = true;
										break;

									case SLIP_ESC_ESC:
										data = SLIP_ESC;
										data_valid = true;
										break;
								}

								escape = false;
							}
							else
							{
								switch (data)
								{
									// escape receives
									case SLIP_ESC:
										escape = true;
										break;

									// packet start
									case SLIP_END:
										packet_pos = 0;
										packet_expected_length = 0;
										escape = false;
										checksum = 0;
										status = SlipDecoderStatus.LengthReceiving;
										break;

									// store packet data
									default:
										data_valid = true;
										break;
								}
							}

							// process data byte if valid data received
							if (data_valid)
							{
								// store data
								if (packet_pos < PACKET_BUFFER_LENGTH)
									packet_buffer[packet_pos++] = data;

								switch (status)
								{
									// length byte (decode multibyte length as well)
									case SlipDecoderStatus.LengthReceiving:
										packet_expected_length = (packet_expected_length << 7) | (data & 0x7f);

										if (packet_expected_length > PACKET_BUFFER_LENGTH)
											status = SlipDecoderStatus.WaitingForPacketStart;
										else
										{
											if ((data & 0x80) == 0)
											{
												packet_pos = 0;
												status = SlipDecoderStatus.PacketTypeReceiving;
											}
										}
										break;

									// get packet type
									case SlipDecoderStatus.PacketTypeReceiving:
										packet_type = data;
										status = SlipDecoderStatus.DataReceiving;
										break;

									// packet data
									case SlipDecoderStatus.DataReceiving:
										// check for packet end
										if (packet_expected_length > 0)
										{
											// if packet was fully and correctly received
											if (packet_pos > packet_expected_length)
											{
												if (checksum == data)
												{
													// create packet
													packet_to_send = packet_builder.UpdatePacket(packet_buffer);

													// update display
													if(packet_to_send != null)
														m_form.Invoke(m_form.m_delegate_update_display, packet_to_send);
												}

												// prepare for new packet
												packet_pos = 0;
												packet_expected_length = 0;
												status = SlipDecoderStatus.WaitingForPacketStart;
											}
										}
										else
										{
											status = SlipDecoderStatus.WaitingForPacketStart;
										}
										break;
								}

								// update checksum
								checksum += data;
							}
						}
					}
				}
				catch
				{
				}
			}

			// inform main thread that this thread stopped
      m_event_stopped.Set();

      // Close our device
      m_flownavi_port.Close();

      return;
    }

		/// <summary>
		/// Flag thread to stop
		/// </summary>
		public void Stop()
		{
			m_stop_thread = true;
		}

		#endregion
	}

}





#endif