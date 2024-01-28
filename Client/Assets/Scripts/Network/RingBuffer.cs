using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class RingBuffer : MonoBehaviour
{
	private static RingBuffer s_inst = null;
	public static RingBuffer Inst { get { return s_inst; } }

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

	public int WritableSize // m_readPos�� ���������� �ϴµ� BUFFERMAX�� �Ѿ�� 
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

			return (m_writePos < m_readPos) ? Define.BufferMax - m_readPos + m_writePos : m_writePos - m_readPos; ;
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
			m_reader.SetBuffer(this, m_readPos);
			PacketHandler.Handle(m_reader);
			MoveReadPos(m_reader.Size);
			++n;
		}
		}

	public bool IsBufferReadable()
	{
		if (TotalReadableSize < Define.PacketSize)
		{
			//Debug.Log("����� ���� �� ���� ��Ŷ");
			return false;
		}

		ushort packetSize;
		if (m_readPos + Define.PacketSize > Define.BufferMax)
		{
			byte first = m_buffer[m_readPos];
			byte second = m_buffer[0];
			packetSize = (ushort)((second << 8) | first);
			Debug.Log("��Ŷ������ : " + packetSize);
		}
		else
			packetSize = BitConverter.ToUInt16(ReadAddr.Array, m_readPos);

		if (packetSize > TotalReadableSize)
		{
			return false;
		}
		if (packetSize > Define.PacketBufferMax)
		{
			Debug.Log("��Ŷ ����� PacketBufferMax���� ŭ : " + packetSize);
			return false;
		}

		return true;
	}

	public bool SetWriteSegment(out ArraySegment<byte> _seg)
	{
		int writableSize;

		while ((writableSize = WritableSize) == 0) {; }

		_seg = new ArraySegment<byte>(m_buffer, m_writePos, writableSize);
	
		return true;
	}

	public void MoveReadPos(int _readBytes)
	{
		lock (m_lock)
		{
			m_writtenBytes -= _readBytes;
			m_readPos = (_readBytes + m_readPos) % Define.BufferMax;
		}
		//Debug.Log("���� : " + _readBytes);
	}

	public void MoveWritePos(int _recvBytes)
	{
		lock (m_lock)
		{
			m_writtenBytes += _recvBytes;
		}
		//Debug.Log("�� : " + m_writtenBytes);
		m_writePos = (_recvBytes + m_writePos) % Define.BufferMax;
		//Debug.Log("writePos : " + m_writePos);
	}
}
