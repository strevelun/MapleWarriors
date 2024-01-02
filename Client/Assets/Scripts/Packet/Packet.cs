using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

public class Packet
{
    private byte[] m_buffer = new byte[Define.PacketBufferMax];
    private int m_addPos = 2;

	public byte[] GetBuffer() { return m_buffer; }
	public int Size
	{
		private set { Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_buffer, 0, sizeof(ushort)); }
		get { return m_addPos; }
	}
	public Packet Add(bool _type) { return Add(GetBytes(_type)); }
	public Packet Add(byte _type) { return Add(new byte[1] { _type }); }
	public Packet Add(char _type) { return Add(GetBytes(_type)); }
	public Packet Add(ushort _type) { return Add(GetBytes(_type)); }
	public Packet Add(uint _type) { return Add(GetBytes(_type)); }
	public Packet Add(ulong _type) { return Add(GetBytes(_type)); }
	public Packet Add(PacketType.Client _type) { return Add(GetBytes(_type)); }
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

	private byte[] GetBytes(bool _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(char _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(ushort _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(uint _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(ulong _type) { return BitConverter.GetBytes(_type); }
	private byte[] GetBytes(string _type) { return Encoding.Unicode.GetBytes((string)(object)_type); }
	private byte[] GetBytes(PacketType.Client _type) { return BitConverter.GetBytes((ushort)_type); }
}
