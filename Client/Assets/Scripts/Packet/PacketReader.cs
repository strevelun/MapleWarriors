using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class PacketReader
{
	private ArraySegment<byte> m_buffer;
	private int m_getPos = Define.PacketSize;
	private int m_startOffset;

	public int Size
	{
		get
		{
			if (m_buffer == null) return 0;

			int size;
			if (m_startOffset + Define.PacketSize > Define.BufferMax)
			{
				byte first = m_buffer.Array[m_startOffset];
				byte second = m_buffer.Array[0];
				size = (second << 8) | first;
			}
			else
				size = BitConverter.ToUInt16(m_buffer.Array, m_startOffset);
			return size;
		}
	}

	public void SetBuffer(RingBuffer _buffer, int _pos)
	{
		m_buffer = _buffer.BufferSegment;
		m_getPos = (_pos + Define.PacketSize) % Define.BufferMax;
		m_startOffset = _pos;
	}

	public void SetBuffer(byte[] _buffer, int _pos)
	{
		m_buffer = new ArraySegment<byte>(_buffer);
		m_getPos = _pos + Define.PacketSize;
		m_startOffset = _pos;
	}

	public PacketType.ServerPacketTypeEnum GetPacketType(bool _moveGetPos = true)
	{
		int prevGetPos = 0;
		if (!_moveGetPos) prevGetPos = m_getPos;
		PacketType.ServerPacketTypeEnum type = (PacketType.ServerPacketTypeEnum)GetUShort();
		if(!_moveGetPos) m_getPos = prevGetPos;
		return type;
	}

	public bool GetBool()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(bool) + m_getPos) % Define.BufferMax;
		return BitConverter.ToBoolean(m_buffer.Array, pos);
	}

	public byte GetByte()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(byte) + m_getPos) % Define.BufferMax;
		return m_buffer.Array[pos];
	}

	public char GetChar()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(char) + m_getPos) % Define.BufferMax;
		char result;
		if (pos + sizeof(char) > Define.BufferMax)
		{
			byte first = m_buffer.Array[pos];
			byte second = m_buffer.Array[0];
			result = (char)((second << 8) | first);
		}
		else
			result = BitConverter.ToChar(m_buffer.Array, pos);

		return result;
	}

	public short GetShort()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(short) + m_getPos) % Define.BufferMax;
		short result;
		if (pos + sizeof(short) > Define.BufferMax)
		{
			byte first = m_buffer.Array[pos];
			byte second = m_buffer.Array[0];
			result = (short)((second << 8) | first);
		}
		else
			result = BitConverter.ToInt16(m_buffer.Array, pos);
		return result;
	}

	public ushort GetUShort()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(ushort) + m_getPos) % Define.BufferMax;
		ushort result;
		if (pos + sizeof(ushort) > Define.BufferMax)
		{
			byte first = m_buffer.Array[pos];
			byte second = m_buffer.Array[0];
			result = (ushort)((second << 8) | first);
		}
		else
			result = BitConverter.ToUInt16(m_buffer.Array, pos);
		return result;
	}

	public int GetInt32()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(int) + m_getPos) % Define.BufferMax;
		int result = 0;
		if (pos + sizeof(int) > Define.BufferMax)
		{
			int i = pos;
			int bit = 0;
			byte temp;
			while (bit < sizeof(int))
			{
				temp = m_buffer.Array[i];
				result |= temp << (bit * 8);
				++bit;
				i = (i + 1) % Define.BufferMax;
			}
		}
		else
			result = BitConverter.ToInt32(m_buffer.Array, pos);
		return result;
	}

	public uint GetUInt32()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(uint) + m_getPos) % Define.BufferMax;
		uint result = 0;
		if (pos + sizeof(uint) > Define.BufferMax)
		{
			int i = pos;
			int bit = 0;
			byte temp;
			while (bit < sizeof(int))
			{
				temp = m_buffer.Array[i];
				result |= (uint)(temp << (bit * 8));
				++bit;
				i = (i + 1) % Define.BufferMax;
			}
		}
		else
			result = BitConverter.ToUInt32(m_buffer.Array, pos);
		return result;
	}

	public long GetInt64()
	{
		int pos = m_getPos;
		m_getPos = (sizeof(long) + m_getPos) % Define.BufferMax;
		long result = 0;
		if (pos + sizeof(long) > Define.BufferMax)
		{
			int i = pos;
			int bit = 0;
			byte temp;
			while (bit < sizeof(long))
			{
				temp = m_buffer.Array[i];
				result |= (long)temp << (bit * 8);
				++bit;
				i = (i + 1) % Define.BufferMax;
			}
		}
		else
			result = BitConverter.ToInt64(m_buffer.Array, pos);
		return result;
	}

	public string GetString()
	{		
		string result = "";

		int i = m_getPos;
		while (m_buffer.Array[i] != 0 || m_buffer.Array[(i + 1) % Define.BufferMax] != 0)
		{
			if (i + sizeof(char) > Define.BufferMax)
			{
				byte first = m_buffer.Array[m_getPos];
				byte second = m_buffer.Array[0];
				result += Encoding.Unicode.GetString(new byte[] { first, second });
			}
			else
			{
				result += Encoding.Unicode.GetString(m_buffer.Array, i, sizeof(char));
			}
			i = (sizeof(char) + i) % Define.BufferMax;
		}

		m_getPos = (i + sizeof(char)) % Define.BufferMax;
		return result;
	}
}
