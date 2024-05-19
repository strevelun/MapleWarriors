using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connection 
{
	private readonly Socket m_socket;

	private readonly SocketAsyncEventArgs m_recvArgs = new SocketAsyncEventArgs();
	private readonly RingBuffer m_ringBuffer;

    public IPEndPoint LocalEndPoint { get; set; }

	public Connection(Socket _socket)
    {
        m_socket = _socket;
        GameObject ringBufferObj = GameObject.Find("@RingBuffer");
        m_ringBuffer = ringBufferObj.GetComponent<RingBuffer>();
		m_ringBuffer.DontDestroyRingBuffer();

		m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        RegisterRecv();
	}

	public void Send(Packet _packet)
	{
        m_socket.Send(_packet.GetBuffer(), _packet.Size, SocketFlags.None);
	}

	public void RegisterRecv()
    {
        m_ringBuffer.SetWriteSegment(out ArraySegment<byte> seg);
      
        m_recvArgs.SetBuffer(seg.Array, seg.Offset, seg.Count);

		bool readLater = m_socket.ReceiveAsync(m_recvArgs);
        if (!readLater)       OnRecvCompleted(null, m_recvArgs);
	}

	public void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
    {
        if(_args.BytesTransferred == 0 || _args.SocketError != SocketError.Success)
		{
			ActionQueue.Inst.Enqueue(() => UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIDisconnectPopup, "서버와의 연결이 끊어졌습니다!\n" + _args.SocketError.ToString()));

			Close();
            return;
        }

		m_ringBuffer.MoveWritePos(_args.BytesTransferred);
		RegisterRecv();
    }

    public void Shutdown()
    {
		m_socket.Shutdown(SocketShutdown.Send);
        Debug.Log("Shutdown");
	}

	public void Close()
    {
		m_socket.Close();
        Debug.Log("Disconnected");
    }
}
