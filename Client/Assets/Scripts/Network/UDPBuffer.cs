using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPBuffer : MonoBehaviour
{
	private readonly object m_lock = new object();
	private readonly PacketReader m_reader = new PacketReader();
	private readonly byte[] m_buffer = new byte[Define.BufferMax];
	private int m_readPos = 0;
	private int m_writePos = 0;
	private int m_writtenBytes = 0;

	public bool Active { get; set; } = false;

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

	private void Update()
    {
		OnBufferReadable();
	}

	public void OnBufferReadable()
	{
		lock (m_lock)
		{
			while (m_writtenBytes != 0)
			{
				m_reader.SetBuffer(m_buffer, m_readPos);
				if (Active) PacketHandler.Handle(m_reader);
				MoveReadPos(m_reader.Size);
			}
		}
		//InGameConsole.Inst?.Log($"MoveReadPos : {m_writtenBytes}, m_writePos : {m_writePos}, m_readPos : {m_readPos}");
	}

	public void SetWriteSegment(out ArraySegment<byte> _seg)
	{
		if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;

		_seg = new ArraySegment<byte>(m_buffer, m_writePos, Define.BufferMax - m_writePos);
	}

	public void MoveReadPos(int _readBytes)
	{
		if (_readBytes == 0) return;

		m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
		if (m_readPos > Define.BufferMax - Define.PacketBufferMax) m_readPos = 0;

		m_writtenBytes -= _readBytes;
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
}
