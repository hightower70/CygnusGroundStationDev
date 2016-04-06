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
// File cache for system (flash) files
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Helpers;
using CygnusGroundStation;
using System;
using System.IO;

namespace CommonClassLibrary.DeviceCommunication
{
	public class DeviceFileCache
	{
		#region · Types ·
		public class CachedFile : IDisposable
		{
			private string m_full_file_path;
			private FileStream m_file_stream;
			private BinaryWriter m_write_file;

			// default constructor
			public CachedFile()
			{
				m_file_stream = null;
				m_write_file = null;
			}

			// Dispose() calls Dispose(true)
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			// destructor
			~CachedFile()
			{
				// Finalizer calls Dispose(false)
				Dispose(false);
			}

			// The bulk of the clean-up code is implemented in Dispose(bool)
			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (m_write_file != null)
					{
						m_write_file.Close();
						m_write_file.Dispose();
						m_write_file = null;
					}

					if (m_file_stream != null)
					{
						m_file_stream.Close();
						m_file_stream.Dispose();
						m_file_stream = null;
					}
				}
			}

			public string FullFilePath
			{
				get { return m_full_file_path; }
			}

			public void Create(string in_full_path)
			{
				m_full_file_path = in_full_path;
				m_file_stream = new FileStream(in_full_path, FileMode.Create);
				m_write_file = new BinaryWriter(m_file_stream);
			}

			public void Close()
			{
				m_write_file.Close();
				m_file_stream.Close();
			}

			public void Write(byte[] in_data, int in_offset, int in_length)
			{
				m_write_file.Write(in_data, in_offset, in_length);
			}
		}
		#endregion

		/// <summary>
		/// Gets file cache system path
		/// </summary>
		/// <returns></returns>
		static public string GetFileCachePath()
		{
			string cache_path;

			cache_path = Path.Combine(SystemPaths.GetApplicationDataPath(), "Cache");

			if (!Directory.Exists(cache_path))
			{
				Directory.CreateDirectory(cache_path);
			}

			return cache_path;
		}

		/// <summary>
		/// Clears file cache
		/// </summary>
		static public void ClearCache()
		{
			string path;

			path = GetFileCachePath();

			try
			{
				Directory.Delete(path, true);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Checks if file exists in the cache
		/// </summary>
		/// <param name="in_file_name"></param>
		/// <param name="in_file_length"></param>
		/// <param name="in_checksum"></param>
		static public bool IsFileExists(string in_file_name, UInt32 in_file_length, MD5Hash in_hash)
		{
			string cache_path = GetFileCachePath();
			string file_path = Path.Combine(cache_path, in_file_name);

			// check if file is exists in the cache
			if(File.Exists(file_path))
			{
				// check file length
				FileInfo file_info = new FileInfo(file_path);
				if (file_info.Length != in_file_length)
					return false;

				// check MD5 checksum
				MD5Hash hash = new MD5Hash();
				hash.ComputeFileHash(file_path);
				if (hash.IsEqual(in_hash))
					return true;
			}

			return false;
		}

		static public CachedFile CreateFile(string in_file_name, UInt32 in_unique_device_id)
		{
			string full_path = Path.Combine(GetFileCachePath(), in_file_name);
			CachedFile cached_file = new CachedFile();

			cached_file.Create(full_path);

			return cached_file;
		}
	}
}
