using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketHandler
{
    public static void Handle(PacketReader _reader)
    {
		PacketType.eServer type = _reader.GetPacketType();
		//Debug.Log(type.ToString());

		switch (type)
        {
			#region Login
            case PacketType.eServer.LoginFailure_AlreadyLoggedIn:
				LoginPacketHandler.LoginFailure_AlreadyLoggedIn(_reader);
                break;
			case PacketType.eServer.LoginFailure_Full:
				LoginPacketHandler.LoginFailure_Full(_reader);
				break;
			case PacketType.eServer.LoginSuccess:
				LoginPacketHandler.LoginSuccess(_reader);
				break;
			#endregion
			#region Lobby
			case PacketType.eServer.LobbyChat:
				LobbyPacketHandler.LobbyChat(_reader);
				break;
			case PacketType.eServer.LobbyUpdateInfo_UserList:
				LobbyPacketHandler.LobbyUpdateInfo_UserList(_reader);
				break;
			case PacketType.eServer.LobbyUpdateInfo_RoomList:
				LobbyPacketHandler.LobbyUpdateInfo_RoomList(_reader);
				break;		
			case PacketType.eServer.CreateRoom_Success:
				LobbyPacketHandler.CreateRoom_Success(_reader);
				break;		
			case PacketType.eServer.CreateRoom_Fail:
				LobbyPacketHandler.CreateRoom_Fail(_reader);
				break;
			#endregion
			#region Room
			case PacketType.eServer.ExitRoom:
				RoomPacketHandler.ExitRoom(_reader);
				break;
				#endregion
				#region InGame
				#endregion
		}
	}

}