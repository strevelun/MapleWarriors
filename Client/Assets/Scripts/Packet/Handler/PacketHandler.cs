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
					LoginPacketHandler.LoginFailure_AlreadyLoggedIn(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.LoginFailure_Full:
					LoginPacketHandler.LoginFailure_Full(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.LoginSuccess:
					LoginPacketHandler.LoginSuccess(_reader);
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
					RoomPacketHandler.StartGame_Success();
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
				case PacketType.ServerPacketTypeEnum.ResInitInfo:
					InGamePacketHandler.ResInitInfo(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.BeginMove:
					InGamePacketHandler.BeginMove(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.Moving:
					InGamePacketHandler.Moving(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.EndMove:
					InGamePacketHandler.EndMove(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.MonsterAttack:
					InGamePacketHandler.MonsterAttack(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.BeginMoveMonster:
					InGamePacketHandler.BeginMoveMonster(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.InGameExit:
					InGamePacketHandler.InGameExit(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.Attack:
					InGamePacketHandler.Attack(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.AttackReq:
					InGamePacketHandler.AttackReq(_reader);
					break;	
				case PacketType.ServerPacketTypeEnum.RangedAttack:
					InGamePacketHandler.RangedAttack(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.RangedAttackReq:
					InGamePacketHandler.RangedAttackReq(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.GameOver:
					InGamePacketHandler.GameOver();
					break;
				case PacketType.ServerPacketTypeEnum.AllCreaturesInfo:
					InGamePacketHandler.AllCreaturesInfo(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.Ready:
					InGamePacketHandler.Ready(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.Start:
					InGamePacketHandler.Start(_reader);
					break;
				case PacketType.ServerPacketTypeEnum.NextStage:
					InGamePacketHandler.NextStage();
					break;
				case PacketType.ServerPacketTypeEnum.MapClear:
					InGamePacketHandler.MapClear();
					break;
				case PacketType.ServerPacketTypeEnum.StageClear:
					InGamePacketHandler.StageClear();
					break;
				case PacketType.ServerPacketTypeEnum.Annihilated:
					InGamePacketHandler.Annihilated();
					break;
				case PacketType.ServerPacketTypeEnum.PlayerHit:
					InGamePacketHandler.PlayerHit(_reader);
					break;
			}
		}
	}
}