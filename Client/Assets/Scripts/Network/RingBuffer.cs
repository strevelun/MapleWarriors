using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingBuffer 
{
	object m_lock = new object();

    private byte[] m_buffer = new byte[Define.BufferMax];
    private byte[] m_tempBuffer = new byte[Define.BufferMax];
	private int m_readPos = 0;
	private int m_writePos = 0;
	private int m_tempPos = 0;
	private bool m_bIsTempUsed = false;
	private int m_writtenBytes = 0;

	//public byte[] ReadAddr { get { return m_bIsTempUsed ? m_tempBuffer : m_buffer.Skip(m_readPos).ToArray(); } }
	public byte[] WriteAddr { get { return m_buffer.Skip(m_writePos).ToArray(); } }
	public ArraySegment<byte> ReadAddr { 
		get 
		{
			if (m_bIsTempUsed) return new ArraySegment<byte>(m_tempBuffer, 0, m_tempPos);
			
			return new ArraySegment<byte>(m_buffer, m_readPos, ReadableSize);
		}}
	public int WritableSize
	{
		get
		{
			if (IsFull()) return 0;
			return (m_readPos <= m_writePos) ? Define.BufferMax - m_writePos : m_readPos - m_writePos;
		}
	}
	public int ReadableSize
	{
		get
		{
			//if (IsFull()) return Define.BufferMax;
			if (m_bIsTempUsed) return m_tempPos;

			return (m_readPos <= m_writePos) ? m_writePos - m_readPos : Define.BufferMax - m_readPos;
		}
	}

	public bool IsFull() => m_writtenBytes >= Define.BufferMax;

	public bool SetWriteSegment(out ArraySegment<byte> _seg)
	{
		_seg = null;
		int writableSize = WritableSize;
		if (writableSize == 0) return false;

		if (m_bIsTempUsed)
		{
			int size = (m_tempPos < sizeof(ushort)) ? Define.PacketHeaderSize : BitConverter.ToUInt16(m_tempBuffer, 0);
			_seg = new ArraySegment<byte>(m_tempBuffer, m_tempPos, size - m_tempPos);
		}
		else
		{
			_seg = new ArraySegment<byte>(m_buffer, m_writePos, writableSize);
		}
		return true;
	}

	public void MoveReadPos(int _readBytes)
	{
		lock (m_lock)
		{
			//Debug.Log("READ중!!!!! : " + m_writtenBytes);
			m_writtenBytes -= _readBytes;
			m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
			if (m_bIsTempUsed)
			{
				m_bIsTempUsed = false;
				m_tempPos = 0;
			}
		}
	}

	public void MoveWritePos(int _recvBytes)
	{
		lock (m_lock)
		{
			m_writtenBytes += _recvBytes;
			m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
			if (m_bIsTempUsed) m_tempPos += _recvBytes;
			//Debug.Log("writePos : " + m_writePos);
		}
	}

	public void HandleVerge()
	{
		if (m_bIsTempUsed) return;

		lock (m_lock)
		{
			int readableSize = ReadableSize;
			ushort cpySize = 0;
			if (readableSize >= sizeof(ushort))
				cpySize = BitConverter.ToUInt16(m_buffer, m_readPos);
			//Debug.Log("패킷사이즈 : " + cpySize + "현재 readPos : " + m_readPos);

			// 만약 r w MAX 이렇게 되어있을땐?
			if (readableSize == 1 || m_readPos + cpySize > Define.BufferMax)
			{
				m_tempPos = Define.BufferMax - m_readPos;
				Buffer.BlockCopy(m_buffer, m_readPos, m_tempBuffer, 0, m_tempPos);
				m_bIsTempUsed = true;
				//Debug.Log(cpySize + ", " + m_readPos + ", " + m_writePos + " : TEMPUSED");
			}
		}
	}
}
