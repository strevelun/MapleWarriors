using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class PacketReader 
{
	private ArraySegment<byte> m_buffer = null;
	private int m_getPos = Define.PacketSize;
	
	public int Size 
	{ 
		get 
		{
			if (m_buffer == null) return 0;

			return BitConverter.ToUInt16(m_buffer.Array, m_buffer.Offset); 
		} 
	}

	public void SetBuffer(RingBuffer _buffer)
	{
		m_buffer = _buffer.ReadAddr;
		m_getPos = Define.PacketSize + m_buffer.Offset;
	}

	public PacketType.eServer GetPacketType()
	{
		return (PacketType.eServer)GetUShort();
	}

	public bool GetBool()
	{
		int pos = m_getPos;
		m_getPos += sizeof(bool);
		return BitConverter.ToBoolean(m_buffer.Array, pos);
	}


	public byte GetByte()
	{
		int pos = m_getPos;
		m_getPos += sizeof(byte);
		return m_buffer.Array[pos];
	}

	public char GetChar()
	{
		int pos = m_getPos;
		m_getPos += sizeof(char);
		return BitConverter.ToChar(m_buffer.Array, pos);
	}

	public short GetShort()
	{
		int pos = m_getPos;
		m_getPos += sizeof(short);
		return BitConverter.ToInt16(m_buffer.Array, pos);
	}

	public ushort GetUShort()
	{
		int pos = m_getPos;
		m_getPos += sizeof(ushort);
		return BitConverter.ToUInt16(m_buffer.Array, pos);
	}

	public string GetString()
	{
		int pos = m_getPos;
		int i = pos;
		for (; i < m_buffer.Offset + m_buffer.Count - 1; i += 2)
		{
			if (m_buffer.Array[i] == 0 && m_buffer.Array[i + 1] == 0)
			{
				m_getPos = i;
				break;
			}
		}

		string result = Encoding.Unicode.GetString(m_buffer.Array, pos, m_getPos - pos);
		m_getPos += sizeof(char);
		return result;
	}
}
