using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class RingBuffer : MonoBehaviour
{
	//private static RingBuffer s_inst = null;
	//public static RingBuffer Inst { get { return s_inst; } }

	object m_lock = new object();

	PacketReader m_reader = new PacketReader();
	private byte[] m_buffer = new byte[Define.BufferMax];
	private int m_readPos = 0;
	private int m_writePos = 0;
	private int m_writtenBytes = 0;

	//public byte[] ReadAddr { get { return m_bIsTempUsed ? m_tempBuffer : m_buffer.Skip(m_readPos).ToArray(); } }
	//public byte[] WriteAddr { get { return m_buffer.Skip(m_writePos).ToArray(); } }
	public ArraySegment<byte> BufferSegment
	{
		get
		{
			return new ArraySegment<byte>(m_buffer, 0, Define.BufferMax);
		}
	}

	public ArraySegment<byte> ReadAddr
	{
		get
		{
			return new ArraySegment<byte>(m_buffer, m_readPos, ReadableSize);
		}
	}

	public int WritableSize // m_readPos를 가져오려고 하는데 BUFFERMAX를 넘어가서 
	{
		get
		{
			int result = 0;
			lock (m_lock)
			{
				if (m_writtenBytes < Define.BufferMax)
				{
					result = (m_readPos <= m_writePos) ? Define.BufferMax - m_writePos : m_readPos - m_writePos;
				}
			}
			return result;
		}
	}

	public int ReadableSize
	{
		get
		{
			if (m_writtenBytes >= Define.BufferMax) return Define.BufferMax - m_readPos;

			return (m_readPos <= m_writePos) ? m_writePos - m_readPos : Define.BufferMax - m_readPos;
		}
	}

	public int TotalReadableSize
	{
		get
		{
			if (m_writtenBytes >= Define.BufferMax) return Define.BufferMax;

			return (m_writePos < m_readPos) ? Define.BufferMax - m_readPos + m_writePos : m_writePos - m_readPos; 
		}
	}

	public void DontDestroyRingBuffer()
	{
		DontDestroyOnLoad(this);
	}

	private void Update()
	{
		while (IsBufferReadable())
		{
			m_reader.SetBuffer(this, m_readPos);
			Debug.Log($"{m_reader.GetPacketType(false)} : 처리시작");
			PacketHandler.Handle(m_reader);
			MoveReadPos(m_reader.Size);
		}
	}

	public bool IsBufferReadable()
	{
		if (TotalReadableSize < Define.PacketSize)
		{
			//Debug.Log("쓸 공간이 적어 사이즈를 읽을 수 없는 패킷");
			return false;
		}

		ushort packetSize;
		if (m_readPos + Define.PacketSize > Define.BufferMax)
		{
			byte first = m_buffer[m_readPos];
			byte second = m_buffer[0];
			packetSize = (ushort)((second << 8) | first);
			Debug.Log("패킷사이즈 : " + packetSize);
		}
		else
			packetSize = BitConverter.ToUInt16(ReadAddr.Array, m_readPos);

		if (packetSize > TotalReadableSize)
		{
			return false;
		}
		else if (packetSize > Define.PacketBufferMax)
		{
			Debug.Log("패킷 사이즈가 PacketBufferMax보다 큼 : " + packetSize);
			return false;
		}
		else if (packetSize < Define.PacketHeaderSize)
		{
			Debug.Log("패킷크기는 최소 4바이트 이상이어야 한다.");
			return false;
		}

		return true;
	}

	public bool SetWriteSegment(out ArraySegment<byte> _seg)
	{
		int writableSize;
		lock (m_lock)
		{
			while ((writableSize = WritableSize) == 0) {; }

			_seg = new ArraySegment<byte>(m_buffer, m_writePos, writableSize);
		}
		return true;
	}

	public void MoveReadPos(int _readBytes)
	{
		lock (m_lock)
		{
			m_writtenBytes -= _readBytes;
			m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
		}
		//Debug.Log("읽음 : " + _readBytes);
	}

	public void MoveWritePos(int _recvBytes)
	{
		lock (m_lock)
		{
			m_writtenBytes += _recvBytes;
			m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
		}
		//Debug.Log("씀 : " + m_writtenBytes);
		Debug.Log("writePos : " + m_writePos);
	}
}
