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
	bool m_errFlag;

	public int Size
	{
		get
		{
			if (m_buffer == null) return 0;

			int size = 0;
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
		//m_buffer = _buffer.ReadAddr;
		//m_getPos = (Define.PacketSize + m_buffer.Offset) % Define.BufferMax;
		m_buffer = _buffer.BufferSegment;
		m_getPos = (_pos + Define.PacketSize) % Define.BufferMax;
		m_startOffset = _pos;
	}

	public PacketType.eServer GetPacketType()
	{
		return (PacketType.eServer)GetUShort();
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

	public string GetString()
	{
		m_errFlag = true;
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
				//result += ((char)m_buffer.Array[i]).ToString();
				result += Encoding.Unicode.GetString(m_buffer.Array, i, sizeof(char));
			}
			i = (sizeof(char) + i) % Define.BufferMax;
		}

		m_getPos = (i + sizeof(char)) % Define.BufferMax;
		return result;
	}
}
