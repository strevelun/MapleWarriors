using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingBuffer : MonoBehaviour
{
	private static RingBuffer s_inst = null;
	public static RingBuffer Inst
	{
		get
		{
			return s_inst;
		}
	}

	object m_lock = new object();

	PacketReader m_reader = new PacketReader();
	private byte[] m_buffer = new byte[Define.BufferMax];
    private byte[] m_tempBuffer = new byte[Define.BufferMax];
	private int m_readPos = 0;
	private int m_writePos = 0;
	private int m_tempPos = 0;
	private bool m_bIsTempUsed = false;
	private int m_writtenBytes = 0;

	//public byte[] ReadAddr { get { return m_bIsTempUsed ? m_tempBuffer : m_buffer.Skip(m_readPos).ToArray(); } }
	//public byte[] WriteAddr { get { return m_buffer.Skip(m_writePos).ToArray(); } }
	public ArraySegment<byte> ReadAddr { 
		get 
		{
			if (m_bIsTempUsed) return new ArraySegment<byte>(m_tempBuffer, 0, m_tempPos);
			
			return new ArraySegment<byte>(m_buffer, m_readPos, ReadableSize);
		}}
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
			if (m_bIsTempUsed) return m_tempPos;

			return (m_readPos <= m_writePos) ? m_writePos - m_readPos : Define.BufferMax - m_readPos;
		}
	}

	private void Awake()
	{
		DontDestroyOnLoad(this);
		s_inst = GetComponent<RingBuffer>();
	}

	private void Update()
	{
		int n = 0;
		while (IsBufferReadable())
		{
			m_reader.SetBuffer(this);
			PacketHandler.Handle(m_reader);

			int size = m_reader.Size;
			MoveReadPos(size);

			if (m_bIsTempUsed) m_bIsTempUsed = false;
			++n;
		}
		HandleVerge();
		//if(n > 0)
			//Debug.Log("m_writtenSize : " + m_writtenBytes + ", m_readPos : " + m_readPos + ", m_writePos : " + m_writePos + ", 처리 패킷 수 : " + n);
	}

	// 현재 packetSize가 readableSize보다 큰 경우
	// 현재 패킷 사이즈 = BufferMax - m_readPos + m_writePos 일때만 읽을 수 있다.
	public bool IsBufferReadable()
	{
		if (m_bIsTempUsed) return true;

		int readableSize = ReadableSize;
		if (readableSize < Define.PacketSize)
		{
			//Debug.Log("사이즈를 읽을 수 없는 패킷");
			return false;
		}

		ushort packetSize = BitConverter.ToUInt16(ReadAddr.Array, m_readPos);
		if (packetSize > readableSize)
		{
			return false;
		}
		if (packetSize > Define.PacketBufferMax)
		{
			Debug.Log("패킷 사이즈가 PacketBufferMax보다 큼 : " + packetSize);
			return false;
		}

		return true;
	}

	public bool SetWriteSegment(out ArraySegment<byte> _seg)
	{
		_seg = null;
		int writableSize;
		//if (writableSize == 0) return false;

		while((writableSize = WritableSize) == 0) { ; }

		/*
		lock (m_lock)
		{
			if (m_bIsTempUsed)
			{
				int size = (m_tempPos < Define.PacketSize) ? Define.PacketHeaderSize : BitConverter.ToUInt16(m_tempBuffer, 0);
				_seg = new ArraySegment<byte>(m_tempBuffer, m_tempPos, size - m_tempPos);
				m_bIsTempUsed = false;
				m_tempPos = 0;
			}
			else
			{*/
		_seg = new ArraySegment<byte>(m_buffer, m_writePos, writableSize);
			//}
		//}
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
		}
		//Debug.Log("씀 : " + m_writtenBytes);
		m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
		//if (m_bIsTempUsed) m_tempPos += _recvBytes;
		//Debug.Log("writePos : " + m_writePos);
	}

	// 1. HandleVerge후 바로 tempBuffer에 쓰여지도록 SetWriteSegment해야 함
	public void HandleVerge()
	{
		int readableSize = ReadableSize;
		ushort packetSize = 0;
		if (readableSize >= Define.PacketSize)
			packetSize = BitConverter.ToUInt16(m_buffer, m_readPos);

		if ((readableSize == 1 || packetSize + m_readPos > Define.BufferMax) && packetSize <= Define.BufferMax - m_readPos + m_writePos)
		{
			m_tempPos = packetSize;
			int cpySize = Define.BufferMax - m_readPos;
			Buffer.BlockCopy(m_buffer, m_readPos, m_tempBuffer, 0, cpySize);
			Buffer.BlockCopy(m_buffer, 0, m_tempBuffer, cpySize, packetSize - cpySize);
			m_bIsTempUsed = true;
			Debug.Log("복사");
		}
	}
}
