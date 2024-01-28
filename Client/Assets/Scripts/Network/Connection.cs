using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Connection 
{
    Socket m_socket;

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
        if(!RingBuffer.Inst.SetWriteSegment(out seg))
        {
            Debug.Log("버퍼에 공간이 없습니다");
            return;
        }
        m_recvArgs.SetBuffer(seg.Array, seg.Offset, seg.Count);
        //Debug.Log("offset : " + seg.Offset + ", Count : " + seg.Count);

        bool pending = m_socket.ReceiveAsync(m_recvArgs);
        if (!pending)       OnRecvCompleted(null, m_recvArgs);
	}

	static int maxByte = 0;

	public void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
    {
        if(_args.BytesTransferred == 0 || _args.SocketError != SocketError.Success)
		{
			ActionQueue.Inst.Enqueue(() => UIManager.Inst.ShowPopupUI(Define.UIPopup.UIDisconnectPopup, "서버와의 연결이 끊어졌습니다!\n" + _args.SocketError.ToString()));

			Disconnect();
            return;
        }
        if (maxByte < _args.BytesTransferred) maxByte = _args.BytesTransferred;
		RingBuffer.Inst.MoveWritePos(_args.BytesTransferred);
        Debug.Log("받은 바이트 수 : " + _args.BytesTransferred + ", 최대바이트 : " + maxByte);

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
        maxByte = 0;
        Debug.Log("Disconnected");
    }
}
