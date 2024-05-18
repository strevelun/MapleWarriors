using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

public class Packet
{
    private readonly byte[] m_buffer = new byte[Define.PacketBufferMax];
    private int m_addPos = Define.PacketSize;

	public byte[] GetBuffer() { return m_buffer; }
	public int Size
	{
		private set { Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_buffer, 0, sizeof(ushort)); }
		get { return m_addPos; }
	}

	public PacketType.ClientPacketTypeEnum GetPacketType()
	{
		return (PacketType.ClientPacketTypeEnum)m_buffer[2];
	}

	public Packet Add(byte _type) { return Add(new byte[1] { _type }); }
	//public Packet Add(char _type) { return Add(GetBytes(_type)); }
	public Packet Add(short _type) { return Add(GetBytes(_type)); }
	public Packet Add(ushort _type) { return Add(GetBytes(_type)); }
	public Packet Add(int _type) { return Add(GetBytes(_type)); }
	public Packet Add(uint _type) { return Add(GetBytes(_type)); }
	public Packet Add(long _type) { return Add(GetBytes(_type)); }
	public Packet Add(PacketType.ClientPacketTypeEnum _type) { return Add(GetBytes(_type)); }
	public Packet Add(PacketType.ServerPacketTypeEnum _type) { return Add(GetBytes(_type)); }
	public Packet Add(string _type) 
	{
		Packet p = Add(GetBytes(_type));
		m_addPos += sizeof(char); // ³Î¹®ÀÚ
		Size = m_addPos;
		return p; 
	}
	private Packet Add(byte[] _bytes) 
	{
		int typeSize = _bytes.Length;

		Array.Copy(_bytes, 0, m_buffer, m_addPos, typeSize);
		m_addPos += typeSize;

		Size = m_addPos;

		return this;
	}

	//private byte[] GetBytes(char _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(short _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(ushort _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(int _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(uint _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(long _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(string _type) { return Encoding.Unicode.GetBytes((string)(object)_type); }
	private byte[] GetBytes(PacketType.ClientPacketTypeEnum _type) { return BitConverter.GetBytes((ushort)_type); }
	private byte[] GetBytes(PacketType.ServerPacketTypeEnum _type) { return BitConverter.GetBytes((ushort)_type); }
}
