using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class PacketReader 
{
	private byte[] m_buffer = null;
	private int m_getPos = Define.PacketSize;
	
	public int Size 
	{ 
		get 
		{
			if (m_buffer == null) return 0;

			return BitConverter.ToUInt16(m_buffer, 0); 
		} 
	}

	public void SetBuffer(RingBuffer _buffer)
	{
		m_buffer = _buffer.ReadAddr.ToArray();
		m_getPos = Define.PacketSize;
	}

	public bool IsBufferReadable(RingBuffer _buffer)
	{
		int readableSize = _buffer.ReadableSize;
		if (readableSize < Define.PacketSize)	return false;

		ushort packetSize = BitConverter.ToUInt16(_buffer.ReadAddr.ToArray(), 0);
		if (packetSize > readableSize)			return false;
		if (packetSize > Define.PacketBufferMax)
		{
			Debug.Log("패킷 사이즈 에러");
			return false;
		}

		return true;
	}

	public PacketType.eServer GetPacketType()
	{
		return (PacketType.eServer)GetUShort();
	}

	public byte GetByte()
	{
		int pos = m_getPos;
		m_getPos += sizeof(byte);
		return m_buffer[pos];
	}

	public char GetChar()
	{
		int pos = m_getPos;
		m_getPos += sizeof(char);
		return BitConverter.ToChar(m_buffer, pos);
	}

	public short GetShort()
	{
		int pos = m_getPos;
		m_getPos += sizeof(short);
		return BitConverter.ToInt16(m_buffer, pos);
	}

	public ushort GetUShort()
	{
		int pos = m_getPos;
		m_getPos += sizeof(ushort);
		return BitConverter.ToUInt16(m_buffer, pos);
	}

	public string GetString()
	{
		int pos = m_getPos;
		for (int i = pos; i < m_buffer.Length - 1; i += 2)
		{
			if (m_buffer[i] == 0 && m_buffer[i + 1] == 0)
			{
				m_getPos = i;
				break;
			}
		}

		string result = Encoding.Unicode.GetString(m_buffer, pos, m_getPos - pos);
		m_getPos += sizeof(char);
		return result;
	}
}
