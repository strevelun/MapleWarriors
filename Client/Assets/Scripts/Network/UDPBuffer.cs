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
		lock (m_lock)
		{
			while (m_writePos - m_readPos != 0)
			{
				m_reader.SetBuffer(m_buffer, m_readPos);
				PacketHandler.Handle(m_reader);
				MoveReadPos(m_reader.Size);
			}
		}
	}

	public void SetWriteSegment(out ArraySegment<byte> _seg)
	{
		lock (m_lock)
		{
			if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;

			_seg = new ArraySegment<byte>(m_buffer, m_writePos, Define.BufferMax - m_writePos);
		//	Debug.Log($"SetWriteSegment : {m_writePos}");
		}
	}

	public void OnRecv(UDPCommunicator _comm, int _bytesTransferred)
	{
		lock(m_lock)
		{
			m_reader.SetBuffer(m_buffer, m_writePos);
			PacketType.eServer type = m_reader.GetPacketType(false);
			if(PacketType.eServer.EndMove == type)
			{
				Debug.Log($"EndMove왓따!!! : {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
			}

			MoveWritePos(_bytesTransferred);
			_comm.RegisterRecv();
		}
	}

	public void MoveReadPos(int _readBytes)
	{
		lock (m_lock)
		{
			m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
			if (m_readPos > Define.BufferMax - Define.PacketBufferMax) m_readPos = 0;
			//Debug.Log($"MoveReadPos : {m_readPos}");
		}
	}

	public void MoveWritePos(int _recvBytes)
	{
		lock (m_lock)
		{
			m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
			if (m_writePos > Define.BufferMax - Define.PacketBufferMax) m_writePos = 0;
			//Debug.Log($"MoveWritePos : {m_writePos}");
		}
	}
}
