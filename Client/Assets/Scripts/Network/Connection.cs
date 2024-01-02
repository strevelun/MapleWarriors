using System;
using System.Net.Sockets;
using UnityEngine;

public class Connection 
{
    Socket m_socket;
    RingBuffer m_ringBuffer = new RingBuffer();

    SocketAsyncEventArgs m_recvArgs = new SocketAsyncEventArgs();

	public Connection(Socket _socket)
    {
        m_socket = _socket; 
       
		m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        RegisterRecv();
	}

	public void Send(Packet _packet)
	{
        m_socket.Send(_packet.GetBuffer(), _packet.Size, SocketFlags.None);
	}

    public void RegisterRecv()
    {
        ArraySegment<byte> seg;
        if(!m_ringBuffer.SetWriteSegment(out seg))
        {
            return;
        }
        m_recvArgs.SetBuffer(seg.Array, seg.Offset, seg.Count);

        bool pending = m_socket.ReceiveAsync(m_recvArgs);
        if (!pending) OnRecvCompleted(null, m_recvArgs);
    }

    public void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
    {
        if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
        {
            byte[] buf =  _args.Buffer;
            PacketReader reader = new PacketReader();
            reader.SetBuffer(m_ringBuffer);
            reader.GetPacketType();
            string str = reader.GetString();
            byte result = reader.GetByte();
            Debug.Log(str);
            Debug.Log("BytesTransferred : " + _args.BytesTransferred);
            m_ringBuffer.MoveWritePos(_args.BytesTransferred);

            RegisterRecv();
        }
        else
            Disconnect();
    }

    public void Disconnect()
    {
        m_socket.Shutdown(SocketShutdown.Both);
        m_socket.Close();
        Debug.Log("Disconnected");
    }
}
