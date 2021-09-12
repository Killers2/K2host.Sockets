/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;

using Microsoft.VisualBasic;

using gl = K2host.Core.OHelpers;

namespace K2host.Sockets.Raw
{

	public class OStandardPacket : IDisposable
	{

		#region Properties

		public int DataPointer
		{
			get;
			set;
		}

		public byte[] Data
		{
			get;
			set;
		}

		#endregion

		#region Instance

		public OStandardPacket()
		{
			Data = Array.Empty<byte>();
		}

		public OStandardPacket(byte[] buffer)
		{
			Data = new byte[buffer.Length];
			buffer.CopyTo(Data, 0);
			FileSystem.Reset();
		}

		#endregion

		#region Public Voids

		public void Reset()
		{
			DataPointer = 0;
		}

		public void Truncate()
		{
			Array.Clear(Data, 0, Data.Length);
			Data = null;
			Data = Array.Empty<byte>();
		}

		public void Seek(int ToPointer)
		{
			if (ToPointer < 0 | ToPointer > Data.Length)
			{
				throw new Exception("Data Service: Invalid pointer index.");
			}
			else
			{
				DataPointer = ToPointer;
			}
		}

		public void WriteData(bool data)
		{
			byte[] tmp = new byte[1];
			if (data)
				tmp[0] = 1;
			else
				tmp[0] = 0;
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(byte data)
		{
			byte[] tmp = new byte[1];
			tmp[0] = data;
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(float data)
		{
			byte[] tmp = BitConverter.GetBytes(data);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(double data)
		{
			byte[] tmp = BitConverter.GetBytes(data);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(short data)
		{
			byte[] tmp = BitConverter.GetBytes(data);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(int data)
		{
			byte[] tmp = BitConverter.GetBytes(data);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(long data)
		{
			byte[] tmp = BitConverter.GetBytes(data);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public void WriteData(string data)
		{
			byte[] tmp = System.Text.Encoding.ASCII.GetBytes(data);
			byte[] b = new byte[1];
			b[0] = 0;
			tmp = gl.CombineByteArrays(tmp, b);
			Data = gl.CombineByteArrays(Data, tmp);
		}

		public bool ReadBoolean()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				byte value = Data[DataPointer];
				DataPointer += 1;

				return (value == 1);
			}
		}

		public byte ReadByte()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				byte value = Data[DataPointer];
				DataPointer += 1;

				return value;
			}
		}

		public float ReadSingle()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				float value = BitConverter.ToSingle(Data, DataPointer);
				byte[] b = BitConverter.GetBytes(value);
				DataPointer += b.Length;

				return value;
			}
		}

		public double ReadDouble()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				double value = BitConverter.ToDouble(Data, DataPointer);
				byte[] b = BitConverter.GetBytes(value);
				DataPointer += b.Length;

				return value;
			}
		}

		public short ReadShort()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				short value = BitConverter.ToInt16(Data, DataPointer);
				byte[] b = BitConverter.GetBytes(value);
				DataPointer += b.Length;

				return value;
			}
		}

		public int ReadInteger()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				int value = BitConverter.ToInt32(Data, DataPointer);
				byte[] b = BitConverter.GetBytes(value);
				DataPointer += b.Length;

				return value;
			}
		}

		public long ReadLong()
		{
			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				long value = BitConverter.ToInt64(Data, DataPointer);
				byte[] b = BitConverter.GetBytes(value);
				DataPointer += b.Length;

				return value;
			}
		}

		public string ReadString()
		{

			if (DataPointer >= Data.Length)
			{
				throw new Exception("Data Service: Input past end of data.");
			}
			else
			{
				int Length = 0;
				int EndPo = gl.SeekByte(Data, 0x0, DataPointer, ref Length);

				if (EndPo == -1)
					throw new Exception("Read error");

				string value = gl.NormalizeString(BitConverter.ToString(Data, DataPointer, Length));
				DataPointer += value.Length + 1;

				return value.Replace(Strings.Chr(0), ' ');

			}

		}

		public bool IsDisposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{

			if (!IsDisposed)
				if (disposing)
				{

					Array.Clear(Data, 0, Data.Length);
					Data = Array.Empty<byte>();
					Data = null;

				}

			IsDisposed = true;
		}

		#endregion

	}

}
