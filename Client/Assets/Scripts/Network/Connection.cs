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
        if(_args.BytesTransferred == 0 || _args.SocketError != SocketError.Success)
		{
			ActionQueue.Inst.Enqueue(() => UIManager.Inst.ShowPopupUI(Define.UIPopup.UIDisconnectPopup, "서버와의 연결이 끊어졌습니다!\n" + _args.SocketError.ToString()));

			Disconnect();
            return;
        }

        PacketReader reader = new PacketReader();
        reader.SetBuffer(m_ringBuffer);
        m_ringBuffer.MoveWritePos(_args.BytesTransferred);

        ActionQueue.Inst.Enqueue(() =>
        {
            PacketHandler.Handle(reader);
            m_ringBuffer.MoveReadPos(reader.Size);
		});


        RegisterRecv();
    }

    public void Disconnect()
    {
        try
		{
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
		} catch (SocketException ex)
        {
            Debug.Log(ex.ErrorCode);
		}

        Debug.Log("Disconnected");
    }
}
