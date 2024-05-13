using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPBuffer : MonoBehaviour
{
	object m_lock = new object();
	PacketReader m_reader = new PacketReader();
	byte[] m_buffer = new byte[Define.BufferMax];
	int m_readPos = 0;
	int m_writePos = 0;
	bool m_readable = false;

	public ArraySegment<byte> BufferSegment
	{
		get
		{
			return new ArraySegment<byte>(m_buffer, 0, Define.BufferMax);
		}
	}

	void Update()
    {
		//Debug.Log("Update");
		OnBufferReadable();
	}

	public void OnBufferReadable()
	{
		if (m_readable)
		{
			lock (m_lock)
			{
				while (m_writePos - m_readPos != 0)
				{
					m_reader.SetBuffer(m_buffer, m_readPos);
					PacketHandler.Handle(m_reader);
					MoveReadPos(m_reader.Size);
				}
				m_readable = false;
			}
		}
	}

	public void SetWriteSegment(out ArraySegment<byte> _seg)
	{
		if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;

		_seg = new ArraySegment<byte>(m_buffer, m_writePos, Define.BufferMax - m_writePos);
	}

	public void MoveReadPos(int _readBytes)
	{
		m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
		if (m_readPos > Define.BufferMax - Define.PacketBufferMax) m_readPos = 0;
		//Debug.Log($"MoveReadPos : {m_readPos}");
	}

	public void MoveWritePos(int _recvBytes)
	{
		lock (m_lock)
		{
			m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
			if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;
			//Debug.Log($"MoveWritePos : {m_writePos}");
			if(!m_readable) m_readable = true;
		}
	}
	/*
	public void Clear()
	{
		m_readable = false;
		m_writePos = 0;
		m_readPos = 0;
	}
	*/
}
