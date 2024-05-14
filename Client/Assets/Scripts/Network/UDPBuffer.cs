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
	int m_writtenBytes = 0;

	public ArraySegment<byte> BufferSegment
	{
		get
		{
			return new ArraySegment<byte>(m_buffer, 0, Define.BufferMax);
		}
	}

	private void Start()
	{
		DontDestroyOnLoad(this);
	}

	void Update()
    {
		OnBufferReadable();
	}

	public void OnBufferReadable()
	{
		while (true)
		{
			lock(m_lock)
			{
				if (m_writtenBytes == 0) break;
			}

			m_reader.SetBuffer(m_buffer, m_readPos);
			PacketHandler.Handle(m_reader);
			MoveReadPos(m_reader.Size);
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

		lock (m_lock)
		{
			m_writtenBytes -= _readBytes;
		}
		InGameConsole.Inst.Log($"MoveReadPos : {_readBytes}");
	}

	public void MoveWritePos(int _recvBytes)
	{
		m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
		if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;
		Debug.Log($"UDPBuffer MoveWritePos : {m_writePos}");

		lock (m_lock)
		{
			m_writtenBytes += _recvBytes;
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
