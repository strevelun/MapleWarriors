using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketHandler
{
    public static void Handle(PacketReader _reader)
    {
		Define.eScene eSceneType = SceneManagerEx.Inst.CurScene.SceneType;
		PacketType.eServer type = _reader.GetPacketType();
		//Debug.Log(eSceneType.ToString());

		if (eSceneType == Define.eScene.Login)
		{
			switch (type)
			{
		
				case PacketType.eServer.LoginFailure_AlreadyLoggedIn:
					LoginPacketHandler.LoginFailure_AlreadyLoggedIn(_reader);
					break;
				case PacketType.eServer.LoginFailure_Full:
					LoginPacketHandler.LoginFailure_Full(_reader);
					break;
				case PacketType.eServer.LoginSuccess:
					LoginPacketHandler.LoginSuccess(_reader);
					break;
			}
		}
		else if (eSceneType == Define.eScene.Lobby)
		{
			switch (type)
			{
				case PacketType.eServer.Test:
					LobbyPacketHandler.Test(_reader);
					break;
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
				case PacketType.eServer.EnterRoom_Success:
					LobbyPacketHandler.EnterRoom_Success(_reader);
					break;
				case PacketType.eServer.EnterRoom_Full:
					LobbyPacketHandler.EnterRoom_Full(_reader);
					break;
				case PacketType.eServer.EnterRoom_InGame:
					LobbyPacketHandler.EnterRoom_InGame(_reader);
					break;
				case PacketType.eServer.EnterRoom_NoRoom:
					LobbyPacketHandler.EnterRoom_NoRoom(_reader);
					break;
			}
		}
		else if (eSceneType == Define.eScene.Room)
		{
			switch (type)
			{
				case PacketType.eServer.RoomChat:
					RoomPacketHandler.RoomChat(_reader);
					break;
				case PacketType.eServer.ExitRoom:
					RoomPacketHandler.ExitRoom(_reader);
					break;
				case PacketType.eServer.NotifyRoomUserExit:
					RoomPacketHandler.NotifyRoomUserExit(_reader);
					break;
				case PacketType.eServer.NotifyRoomUserEnter:
					RoomPacketHandler.NotifyRoomUserEnter(_reader);
					break;
				case PacketType.eServer.RoomUsersInfo:
					RoomPacketHandler.RoomUsersInfo(_reader);
					break;
				case PacketType.eServer.StartGame_Success:
					RoomPacketHandler.StartGame_Success(_reader);
					break;
				case PacketType.eServer.StartGame_Fail:
					RoomPacketHandler.StartGame_Fail(_reader);
					break;
				case PacketType.eServer.RoomReady:
					RoomPacketHandler.RoomReady(_reader);
					break;
				case PacketType.eServer.RoomReady_Fail:
					RoomPacketHandler.RoomReady_Fail(_reader);
					break;
				case PacketType.eServer.RoomStandby:
					RoomPacketHandler.RoomStandby(_reader);
					break;
				case PacketType.eServer.RoomStandby_Fail:
					RoomPacketHandler.RoomStandby_Fail(_reader);
					break;
				case PacketType.eServer.RoomMapChoice:
					RoomPacketHandler.RoomMapChoice(_reader);
					break;
				case PacketType.eServer.RoomCharacterChoice:
					RoomPacketHandler.RoomCharacterChoice(_reader);
					break;
			}
		}
		else if (eSceneType == Define.eScene.InGame)
		{
			switch (type)
			{
				case PacketType.eServer.ResInitInfo:
					InGamePacketHandler.ResInitInfo(_reader);
					break;
				case PacketType.eServer.BeginMove:
					InGamePacketHandler.BeginMove(_reader);
					break;	
				case PacketType.eServer.EndMove:
					InGamePacketHandler.EndMove(_reader);
					break;
				case PacketType.eServer.BeginMoveMonster:
					InGamePacketHandler.BeginMoveMonster(_reader);
					break;	
				case PacketType.eServer.InGameExit:
					InGamePacketHandler.InGameExit(_reader);
					break;
				case PacketType.eServer.Attack:
					InGamePacketHandler.Attack(_reader);
					break;	
				case PacketType.eServer.RangedAttack:
					InGamePacketHandler.RangedAttack(_reader);
					break;
				case PacketType.eServer.GameOver:
					InGamePacketHandler.GameOver(_reader);
					break;
			}
		}
	}

}