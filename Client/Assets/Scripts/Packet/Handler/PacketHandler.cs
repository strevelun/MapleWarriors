using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketHandler
{
    public static void Handle(PacketReader _reader)
    {
		Define.SceneEnum eSceneType = SceneManagerEx.Inst.CurScene.SceneType;
		PacketType.ServerPacketTypeEnum type = _reader.GetPacketType();
		
		if (eSceneType == Define.SceneEnum.Login)
		{
			switch (type)
			{
		
				case PacketType.ServerPacketTypeEnum.LoginFailure_AlreadyLoggedIn:
					LoginPacketHandler.LoginFailure_AlreadyLoggedIn();
					break;
				case PacketType.ServerPacketTypeEnum.LoginFailure_Full:
					LoginPacketHandler.LoginFailure_Full();
					break;
				case PacketType.ServerPacketTypeEnum.LoginSuccess:
					LoginPacketHandler.LoginSuccess();
					break;
				case PacketType.ServerPacketTypeEnum.CheckedClientInfo:
					LoginPacketHandler.CheckedClientInfo(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.ConnectionID:
					LoginPacketHandler.ConnectionID(_reader);
					break;
			}
		}
		else if (eSceneType == Define.SceneEnum.Lobby)
		{
			switch (type)
			{
				case PacketType.ServerPacketTypeEnum.Test:
					LobbyPacketHandler.Test();
					break;
				case PacketType.ServerPacketTypeEnum.LobbyChat:
					LobbyPacketHandler.LobbyChat(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.LobbyUpdateInfo_UserList:
					LobbyPacketHandler.LobbyUpdateInfo_UserList(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.LobbyUpdateInfo_RoomList:
					LobbyPacketHandler.LobbyUpdateInfo_RoomList(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.CreateRoom_Success:
					LobbyPacketHandler.CreateRoom_Success();
					break;
				case PacketType.ServerPacketTypeEnum.CreateRoom_Fail:
					LobbyPacketHandler.CreateRoom_Fail();
					break;
				case PacketType.ServerPacketTypeEnum.EnterRoom_Success:
					LobbyPacketHandler.EnterRoom_Success();
					break;
				case PacketType.ServerPacketTypeEnum.EnterRoom_Full:
					LobbyPacketHandler.EnterRoom_Full();
					break;
				case PacketType.ServerPacketTypeEnum.EnterRoom_InGame:
					LobbyPacketHandler.EnterRoom_InGame();
					break;
				case PacketType.ServerPacketTypeEnum.EnterRoom_NoRoom:
					LobbyPacketHandler.EnterRoom_NoRoom();
					break;
			}
		}
		else if (eSceneType == Define.SceneEnum.Room)
		{
			switch (type)
			{
				case PacketType.ServerPacketTypeEnum.RoomChat:
					RoomPacketHandler.RoomChat(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.ExitRoom:
					RoomPacketHandler.ExitRoom();
					break;
				case PacketType.ServerPacketTypeEnum.NotifyRoomUserExit:
					RoomPacketHandler.NotifyRoomUserExit(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.NotifyRoomUserEnter:
					RoomPacketHandler.NotifyRoomUserEnter(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.RoomUsersInfo:
					RoomPacketHandler.RoomUsersInfo(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.StartGame_Success:
					RoomPacketHandler.StartGame_Success(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.StartGame_Fail:
					RoomPacketHandler.StartGame_Fail();
					break;
				case PacketType.ServerPacketTypeEnum.RoomReady:
					RoomPacketHandler.RoomReady(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.RoomReady_Fail:
					RoomPacketHandler.RoomReady_Fail();
					break;
				case PacketType.ServerPacketTypeEnum.RoomStandby:
					RoomPacketHandler.RoomStandby(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.RoomStandby_Fail:
					RoomPacketHandler.RoomStandby_Fail();
					break;
				case PacketType.ServerPacketTypeEnum.RoomMapChoice:
					RoomPacketHandler.RoomMapChoice(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.RoomCharacterChoice:
					RoomPacketHandler.RoomCharacterChoice(_reader);
					break;
			}
		}
		else if (eSceneType == Define.SceneEnum.InGame)
		{
			switch (type)
			{
				case PacketType.ServerPacketTypeEnum.C_BeginMove:
					InGamePacketHandler.BeginMove(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_Moving:
					InGamePacketHandler.Moving(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_EndMove:
					InGamePacketHandler.EndMove(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_MonsterAttack:
					InGamePacketHandler.MonsterAttack(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_BeginMoveMonster:
					InGamePacketHandler.BeginMoveMonster(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.InGameExit:
					InGamePacketHandler.InGameExit(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_Attack:
					InGamePacketHandler.Attack(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.C_AttackReq:
					InGamePacketHandler.AttackReq(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.C_RangedAttack:
					InGamePacketHandler.RangedAttack(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_RangedAttackReq:
					InGamePacketHandler.RangedAttackReq(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.GameOver:
					InGamePacketHandler.GameOver();
					break;
				case PacketType.ServerPacketTypeEnum.C_AllCreaturesInfo:
					InGamePacketHandler.AllCreaturesInfo(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_Ready:
					InGamePacketHandler.Ready(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_Start:
					InGamePacketHandler.Start(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.C_NextStage:
					InGamePacketHandler.NextStage();
					break;
				case PacketType.ServerPacketTypeEnum.C_MapClear:
					InGamePacketHandler.MapClear();
					break;
				case PacketType.ServerPacketTypeEnum.C_StageClear:
					InGamePacketHandler.StageClear();
					break;
				case PacketType.ServerPacketTypeEnum.C_Annihilated:
					InGamePacketHandler.Annihilated();
					break;
				case PacketType.ServerPacketTypeEnum.C_PlayerHit:
					InGamePacketHandler.PlayerHit(_reader);
					break;
			}
		}
	}
}