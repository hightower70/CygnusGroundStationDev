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
// File transfer functions (device <-> ground station)
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Helpers;
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CommonClassLibrary.DeviceCommunication
{
	public class FileTransferManager
	{
		#region · Constants ·

		private const byte InvalidSystemFileID = 0xff;
		private const int FileOperationTimeout = 200;
		private const int FileOperationMaxRetryCount = 3;

		#endregion

		#region · Types ·

		public delegate void ReceivedPacketProcessorCallback(byte in_interface_index, byte[] in_packet_buffer, byte in_packet_length);

		// callback function for file operation callback
		public delegate void FileOperationCallback(PacketFileOperationBase in_response_packet);

		// class storing information about pending file operations
		class PendingFileOperationInfo
		{
			public PacketFileOperationBase Packet;
			public byte[] Data;
			public DateTime SendTime;
			public byte RetryCount;
			public FileOperationCallback Callback;
		};

		// file transfer state
		private enum FileTransferState
		{
			Idle,

			ReadInfo,
			ReadData,

			WriteData
		};

		// result codes for file transfer
		public enum FileTransferResult
		{
			Success,

			NotFound,
			NoConnection,
			Error
		}

		/// <summary>
		/// result information storage for file transfer
		/// </summary>
		public class FileTransferResultInfo
		{
			public FileTransferResultInfo(FileTransferResult in_state)
			{
				State = in_state;
			}

			public FileTransferResultInfo(FileTransferResult in_state, byte in_file_id, string in_full_path)
			{
				State = in_state;
				FileID = in_file_id;
				FullPath = in_full_path;
			}

			public FileTransferResult State;
			public string FullPath;
			public byte FileID;
		}

		// callback for file transfer 
		public delegate void FileTransferCallback(FileTransferResultInfo in_result);


		#endregion

		#region · Data members ·

		// file operation
		private FileTransferState m_file_transfer_state;
		private string m_file_transfer_current_name;
		private byte m_file_transfer_current_id;
		private UInt32 m_file_transfer_current_position;
		private UInt32 m_file_transfer_current_size;
		private DeviceFileCache.CachedFile m_file_transfer_current_file;
		private RealtimeObject m_file_transfer_state_object;
		private RealtimeObjectMember m_file_transfer_name_member;
		private RealtimeObjectMember m_file_transfer_percentage_member;

		private FileTransferCallback m_file_transfer_callback;

		private Dictionary<string, PendingFileOperationInfo> m_pending_file_operations;

		public FileTransferManager()
		{
			m_pending_file_operations = new Dictionary<string, PendingFileOperationInfo>();
			m_file_transfer_state = FileTransferState.Idle;
			m_file_transfer_current_file = null;
		}

		#endregion

		#region · Public members ·

		/// <summary>
		/// Creates realtime object used for communication state
		/// </summary>
		public void CreateRealtimeObjects()
		{
			// create file transfer state object
			m_file_transfer_state_object = RealtimeObjectStorage.Default.ObjectCreate("FileTransferState");
			m_file_transfer_name_member = m_file_transfer_state_object.MemberCreate("FileName", RealtimeObjectMember.MemberType.String);
			m_file_transfer_percentage_member = m_file_transfer_state_object.MemberCreate("FilePercentage", RealtimeObjectMember.MemberType.Int);
			m_file_transfer_state_object.ObjectCreateEnd();
		}

		/// <summary>
		/// Processes file transfer response packets
		/// </summary>
		/// <param name="in_response_packet_type"></param>
		public void ProcessResponsePacket(PacketType in_response_packet_type)
		{
			switch(in_response_packet_type)
			{
				// process file information response packet
				case PacketType.FileInfoResponse:
					ProcessFileInfoResponse();
					break;

				// process file read response packet
				case PacketType.FileDataReadResponse:
					ProcessFileDataReadResponse();
					break;

				// process file write response packet
				case PacketType.FileDataWriteResponse:
					ProcessFileDataWriteResponse();
					break;

				// process file operation finished response
				case PacketType.FileOperationFinishedResponse:
					ProcessFileOperationFinishedResponse();
					break;
			}
		}
		#endregion

		#region · File operation ·

		/// <summary>
		/// Starts file operation
		/// </summary>
		/// <param name="in_packet">Packet determining the operation</param>
		/// <param name="in_callback">Callback for operation finished/timeout signaling</param>
		public void FileOperationStart(PacketFileOperationBase in_packet, FileOperationCallback in_callback)
		{
			string file_operation_id = in_packet.FileOperationID;

			lock (m_pending_file_operations)
			{
				// check if operation already pending on this file
				if (m_pending_file_operations.ContainsKey(file_operation_id))
					throw new Exception("File operation already pending on file ID:" + in_packet.ID);

				// create pending operation information
				PendingFileOperationInfo operation_info = new PendingFileOperationInfo();
				operation_info.Packet = in_packet;
				operation_info.Data = null;
				operation_info.RetryCount = 1;
				operation_info.SendTime = DateTime.Now;
				operation_info.Callback = in_callback;

				// store pending operation
				m_pending_file_operations.Add(file_operation_id, operation_info);
			}

			// send packet
			CommunicationManager.Default.SendPacket(in_packet);
			CommunicationManager.Default.PacketLogWrite("S", in_packet);
		}

		/// <summary>
		/// Starts file operation with separated packet header and data content
		/// </summary>
		/// <param name="in_packet">Packet determining the operation</param>
		/// <param name="in_callback">Callback for operation finished/timeout signaling</param>
		public void FileOperationStart(PacketFileOperationBase in_packet, byte[] in_data, FileOperationCallback in_callback)
		{
			string file_operation_id = in_packet.FileOperationID;

			lock (m_pending_file_operations)
			{
				// check if operation already pending on this file
				if (m_pending_file_operations.ContainsKey(file_operation_id))
					throw new Exception("File operation already pending on file ID:" + in_packet.ID);

				// create pending operation information
				PendingFileOperationInfo operation_info = new PendingFileOperationInfo();
				operation_info.Packet = in_packet;
				operation_info.Data = in_data;
				operation_info.RetryCount = 1;
				operation_info.SendTime = DateTime.Now;
				operation_info.Callback = in_callback;

				// store pending operation
				m_pending_file_operations.Add(file_operation_id, operation_info);
			}

			// send packet
			CommunicationManager.Default.SendPacket(in_packet, in_data);
			CommunicationManager.Default.PacketLogWrite("S", in_packet);
		}

		/// <summary>
		/// removes file operation from the list of pending file operations
		/// </summary>
		/// <param name="in_packet"></param>
		public void FileOperationEnd(PacketFileOperationBase in_packet)
		{
			string file_operation_id = in_packet.FileOperationID;

			lock (m_pending_file_operations)
			{
				m_pending_file_operations.Remove(file_operation_id);
			}
		}

		/// <summary>
		/// Starts file read (download from device) operation
		/// </summary>
		/// <param name="in_file_name"></param>
		public void FileDownloadStart(string in_file_name, FileTransferCallback in_callback)
		{
			// sanity check
			if (m_file_transfer_state != FileTransferState.Idle)
				throw new Exception("File transfer already pending.");

			// initialize file transfer info
			m_file_transfer_callback = in_callback;

			m_file_transfer_state = FileTransferState.ReadInfo;
			m_file_transfer_current_name = in_file_name;
			m_file_transfer_current_position = 0;
			m_file_transfer_current_size = 0;

			UpdateFileTransferState();

			// file info packet
			PacketFileInfoRequest request_packet = new PacketFileInfoRequest();
			request_packet.FileName = in_file_name;

			FileOperationStart(request_packet, null);
		}

		/// <summary>
		/// Starts file write (upload to the device) operation
		/// </summary>
		/// <param name="in_file_id"></param>
		/// <param name="in_file_pos"></param>
		/// <param name="in_data"></param>
		/// <param name="in_callback"></param>
		/// <returns></returns>
		public void FileUploadStart(byte in_file_id, UInt32 in_file_pos, byte[] in_data, FileTransferCallback in_callback)
		{
			// sanity check
			if (m_file_transfer_state != FileTransferState.Idle)
				throw new Exception("File transfer already pending.");

			// init file transfer
			m_file_transfer_callback = in_callback;
			m_file_transfer_state = FileTransferState.WriteData;
			m_file_transfer_current_position = in_file_pos;
			m_file_transfer_current_id = in_file_id;

			// create write packet
			PacketFileDataWriteRequestHeader request_packet;

			// create file write packet
			request_packet = new PacketFileDataWriteRequestHeader((byte)in_data.Length);
			request_packet.ID = m_file_transfer_current_id;
			request_packet.Pos = m_file_transfer_current_position;

			// starts file operation
			FileOperationStart(request_packet, in_data, null);
		}

		/// <summary>
		/// Processes file information response packet
		/// </summary>
		private void ProcessFileInfoResponse()
		{
			string file_operation_id;
			PendingFileOperationInfo file_operation_info;
			PacketFileInfoResponse packet = (PacketFileInfoResponse)CommunicationManager.Default.ReceiverQueue.Pop(typeof(PacketFileInfoResponse));
			CommunicationManager.Default.PacketLogWrite("R", packet);

			// check file operation ID
			file_operation_id = packet.FileOperationID;
			if (!m_pending_file_operations.ContainsKey(file_operation_id))
				return;

			// check pending file operation
			file_operation_info = m_pending_file_operations[file_operation_id];
			if (file_operation_info.Packet.PacketType != PacketType.FileInfoRequest)
				return;

			// remove file operation from the peding list
			FileOperationEnd(packet);

			// if no callback if defined, this is regular system file operation
			if (file_operation_info.Callback == null)
			{
				if (packet.ID == InvalidSystemFileID)
				{
					// if file not found -> error
					m_file_transfer_state = FileTransferState.Idle;

					CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.NotFound));
				}
				else
				{
					m_file_transfer_current_position = 0;
					m_file_transfer_current_size = packet.Length;
					m_file_transfer_current_id = packet.ID;

					string cached_file_name = m_file_transfer_current_name + "[" + CommunicationManager.Default.ConnectedDeviceUniqueID.ToString("x4") + "]";

					// file exists -> check file in the cache
					if (DeviceFileCache.IsFileExists(cached_file_name, packet.Length, packet.Hash))
					{
						// file exists in the cache -> call callback with success code
						string file_path;
						file_path = Path.Combine(DeviceFileCache.GetFileCachePath(), cached_file_name);

						m_file_transfer_current_position = m_file_transfer_current_size;
						m_file_transfer_state = FileTransferState.Idle;

						UpdateFileTransferState();

						CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Success, m_file_transfer_current_id, file_path));
					}
					else
					{
						// file doesn't exists in the cache create file in the cache and download it
						m_file_transfer_current_file = DeviceFileCache.CreateFile(cached_file_name, 0);

						// start download
						m_file_transfer_state = FileTransferState.ReadData;

						UpdateFileTransferState();

						SendFileDataReadRequest();
					}
				}
			}
			else
			{
				file_operation_info.Callback(packet);
			}
		}

		/// <summary>
		/// Reads file data block from the device
		/// </summary>
		private void SendFileDataReadRequest()
		{
			UInt32 request_data_length;
			UInt32 max_data_length;
			PacketFileDataReadRequest request_packet;

			// determine data length
			max_data_length = (uint)(PacketConstants.PacketMaxLength - PacketConstants.PacketCRCLength - Marshal.SizeOf(typeof(PacketFileDataReadResponseHeader)));
			request_data_length = m_file_transfer_current_size - m_file_transfer_current_position;

			if (request_data_length > max_data_length)
				request_data_length = max_data_length;

			// create packet
			request_packet = new PacketFileDataReadRequest();
			request_packet.ID = m_file_transfer_current_id;
			request_packet.Pos = m_file_transfer_current_position;
			request_packet.Length = (byte)request_data_length;

			FileOperationStart(request_packet, null);
		}

		/// <summary>
		/// Processes received file data block
		/// </summary>
		private void ProcessFileDataReadResponse()
		{
			byte[] packet;
			byte packet_length;
			byte interface_index;
			DateTime packet_timestamp;
			PacketFileDataReadResponseHeader packet_header;
			int data_offset;
			int data_length;
			string file_operation_id;
			PendingFileOperationInfo file_operation_info;

			if (CommunicationManager.Default.ReceiverQueue.PopBegin(out packet, out packet_length, out interface_index, out packet_timestamp))
			{
				// get packet header
				packet_header = (PacketFileDataReadResponseHeader)RawBinarySerialization.DeserializeObject(packet, typeof(PacketFileDataReadResponseHeader));
				CommunicationManager.Default.PacketLogWrite("R", packet_header);

				// check pending file operation
				file_operation_id = packet_header.FileOperationID;
				if (m_pending_file_operations.ContainsKey(file_operation_id))
				{
					file_operation_info = m_pending_file_operations[file_operation_id];

					// finish file operation
					FileOperationEnd(packet_header);

					if (file_operation_info.Packet.PacketType == PacketType.FileDataReadRequest)
					{
						// determine file data block position and length within the packet
						data_offset = Marshal.SizeOf(typeof(PacketFileDataReadResponseHeader));
						data_length = packet_header.PacketLength - PacketConstants.PacketCRCLength - Marshal.SizeOf(typeof(PacketFileDataReadResponseHeader));

						// if no callback if defined, this is regular system file operation
						if (file_operation_info.Callback == null)
						{
							m_file_transfer_current_file.Write(packet, data_offset, data_length);

							m_file_transfer_current_position += (uint)data_length;

							UpdateFileTransferState();

							if (m_file_transfer_current_position < m_file_transfer_current_size)
							{
								SendFileDataReadRequest();
							}
							else
							{
								m_file_transfer_current_file.Close();

								m_file_transfer_state = FileTransferState.Idle;

								CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Success, m_file_transfer_current_id, m_file_transfer_current_file.FullFilePath));
								m_file_transfer_current_file.Dispose();
								m_file_transfer_current_file = null;
							}
						}
						else
						{
							// prepare deserialized packet for further file data processing
							PacketFileDataReadResponse response = new PacketFileDataReadResponse(packet_header);

							response.SetData(packet, data_offset, data_length);

							file_operation_info.Callback(response);
						}
					}
				}

				// close packet processing
				CommunicationManager.Default.ReceiverQueue.PopEnd();
			}
		}

		/// <summary>
		/// Processes received file data write response
		/// </summary>
		private void ProcessFileDataWriteResponse()
		{
			string file_operation_id;
			PendingFileOperationInfo file_operation_info;
			PacketFileDataWriteResponse packet = (PacketFileDataWriteResponse)CommunicationManager.Default.ReceiverQueue.Pop(typeof(PacketFileDataWriteResponse));

			CommunicationManager.Default.PacketLogWrite("R", packet);

			// check file operation ID
			file_operation_id = packet.FileOperationID;
			if (!m_pending_file_operations.ContainsKey(file_operation_id))
				return;

			// check pending file operation
			file_operation_info = m_pending_file_operations[file_operation_id];
			if (file_operation_info.Packet.PacketType != PacketType.FileDataWriteRequest)
				return;

			// remove file operation from the peding list
			FileOperationEnd(packet);

			// if no callback if defined, this is regular system file operation
			if (file_operation_info.Callback == null)
			{
				// file transfer state: idle
				m_file_transfer_state = FileTransferState.Idle;

				if (packet.ID == InvalidSystemFileID)
				{
					// if file not found -> error
					CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.NotFound));
				}
				else
				{
					if (packet.Error == 0)
					{
						// call transfer callback
						CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Success, m_file_transfer_current_id, ""));
					}
					else
					{
						// call transfer callback
						CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Error));
					}
				}
			}
			else
			{
				file_operation_info.Callback(packet);
			}
		}

		/// <summary>
		/// Send file operation finish request
		/// </summary>
		/// <param name="in_file_id"></param>
		/// <param name="in_request_code"></param>
		public void SendFileFinishRequest(FileOperationFinishMode in_finish_mode, FileTransferCallback in_callback)
		{
			PacketFileOperationFinishedRequest request_packet;

			// create packet
			request_packet = new PacketFileOperationFinishedRequest();
			request_packet.ID = m_file_transfer_current_id;
			request_packet.FinishMode = in_finish_mode;

			m_file_transfer_callback = in_callback;

			FileOperationStart(request_packet, null);
		}

		/// <summary>
		/// Processes file operation finished response
		/// </summary>
		private void ProcessFileOperationFinishedResponse()
		{
			string file_operation_id;
			PendingFileOperationInfo file_operation_info;
			PacketFileOperationFinishedResponse packet = (PacketFileOperationFinishedResponse)CommunicationManager.Default.ReceiverQueue.Pop(typeof(PacketFileOperationFinishedResponse));
			CommunicationManager.Default.PacketLogWrite("R", packet);

			// check file operation ID
			file_operation_id = packet.FileOperationID;
			if (!m_pending_file_operations.ContainsKey(file_operation_id))
				return;

			// check pending file operation
			file_operation_info = m_pending_file_operations[file_operation_id];
			if (file_operation_info.Packet.PacketType != PacketType.FileOperationFinishedRequest)
				return;

			// remove file operation from the peding list
			FileOperationEnd(packet);

			// if no callback if defined, this is regular system file operation
			if (file_operation_info.Callback == null)
			{
				if (packet.Error == PacketFileOperationFinishedResponse.NoError)
				{
					CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Success));
				}
				else
				{
					CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.NotFound));
				}
			}
			else
			{
				file_operation_info.Callback(packet);
			}
		}

		/// <summary>
		/// Handles timeout condition
		/// </summary>
		public void FileOperationTimeOutHandler()
		{
			DateTime current_time = DateTime.Now;
			List<string> file_operations_timeout = new List<string>();

			lock (m_pending_file_operations)
			{
				// check all pending file operation
				foreach (KeyValuePair<string, PendingFileOperationInfo> pending_operation in m_pending_file_operations)
				{
					if ((current_time - pending_operation.Value.SendTime).TotalMilliseconds > FileOperationTimeout)
					{
						if (pending_operation.Value.RetryCount > FileOperationMaxRetryCount)
						{
							// operation failed
							file_operations_timeout.Add(pending_operation.Key);
						}
						else
						{
							// resend oroginal packet again
							pending_operation.Value.RetryCount++;
							pending_operation.Value.SendTime = current_time;

							if (pending_operation.Value.Data == null)
							{
								CommunicationManager.Default.SendPacket(pending_operation.Value.Packet);
								CommunicationManager.Default.PacketLogWrite("S+", pending_operation.Value.Packet);
							}
							else
							{
								CommunicationManager.Default.SendPacket(pending_operation.Value.Packet, pending_operation.Value.Data);
								CommunicationManager.Default.PacketLogWrite("S+", pending_operation.Value.Packet);
							}
						}
					}
				}
			}

			// handle failed operations
			foreach (string operation_id in file_operations_timeout)
			{
				PendingFileOperationInfo file_operation_info = m_pending_file_operations[operation_id];

				// remove operation from pending list
				lock (m_pending_file_operations)
				{
					m_pending_file_operations.Remove(operation_id);
				}

				// if no callback if defined, this is regular system file operation
				if (file_operation_info.Callback == null)
				{
					// file transfer state: idle
					m_file_transfer_state = FileTransferState.Idle;

					// call file transfer callback with error
					CallFileTransferFinishedCallback(new FileTransferResultInfo(FileTransferResult.Error));
				}
				else
				{
					file_operation_info.Callback(null);
				}
			}
		}

		/// <summary>
		/// Updates ROX object to signal current file transfer operation
		/// </summary>
		private void UpdateFileTransferState()
		{
			m_file_transfer_state_object.UpdateBegin();

			m_file_transfer_name_member.Write(m_file_transfer_current_name);
			if (m_file_transfer_current_size == 0)
				m_file_transfer_percentage_member.Write(0);
			else
				m_file_transfer_percentage_member.Write(m_file_transfer_current_position * 100 / m_file_transfer_current_size);

			m_file_transfer_state_object.UpdateEnd();
		}

		/// <summary>
		// Calls file transfer function callback when file transfer ended
		/// </summary>
		/// <param name="in_result"></param>
		/// <param name="in_file_path"></param>
		private void CallFileTransferFinishedCallback(FileTransferResultInfo in_result)
		{
			if (m_file_transfer_callback != null)
				m_file_transfer_callback(in_result);
		}

		#endregion
	}
}
