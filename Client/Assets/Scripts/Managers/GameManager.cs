using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using UnityEngine;

public class GameManager 
{
	private static GameManager s_inst = null;
	public static GameManager Inst
	{
		get
		{
			if (s_inst == null) s_inst = new GameManager();
			return s_inst;
		}
	}

	private readonly Vector3[] m_positions = new Vector3[] {
	new Vector3(2, -1),
	new Vector3(2, -3),
	new Vector3(4, -1),
	new Vector3(4, -3) };

	private readonly Define.StPlayerInfo[] m_arrPlayerInfo = new Define.StPlayerInfo[Define.RoomUserSlotCnt];

	public bool GameStart { get; set; } = false;
	public bool StageLoading { get; set; } = false;
	public int PlayerCnt { get; private set; } = 0;
	public int PlayerAliveCnt { get; private set; } = 0;
	public int MonsterCnt { get; private set; } = 0;
	public bool AllClear { get; set; } = false;
	public bool StageClear { get; set; } = false;
	public bool GameOver { get; set; } = false;
	public bool PlayersOnPortal { get; set; } = false;

	public List<int> OtherPlayersSlot { get; private set; } = new List<int>();
	private bool[] m_playerReady = null;
	private int m_playerReadyCnt = 0;

	public float StartTime { get; set; }
	public float Timer { get; set; }

	private InGameScene m_inGameScene;

	public void Init(InGameScene _inGameScene)
	{
		Timer = 0.0f;
		m_inGameScene = _inGameScene;
		m_playerReady = new bool[Define.RoomUserSlotCnt];
	}

	public bool CheckGameOver()
	{
		if (PlayerCnt == 0) return true;
		if (PlayerAliveCnt == 0) return true;

		return false;
	}

	public bool CheckMapClear()
	{
		if (!GameStart) return false;
		if (MonsterCnt > 0) return false;

		return true;
	}

	public void SetPlayerCnt(int _cnt) { PlayerCnt = _cnt; }
	public void SetPlayerAliveCnt(int _cnt) { PlayerAliveCnt = _cnt; }
	public void SetMonsterCnt(int _cnt) { MonsterCnt = _cnt; }

	public void SetPlayerReady(int _slot)
	{
		if (m_playerReady[_slot]) return;

		m_playerReady[_slot] = true;
		++m_playerReadyCnt;
	}

	public void SetPlayerInfo(int _idx, string _nickname, int _characterChoice, string _ip, string _privateIP, int _port)
	{
		if (_idx < 0 || _idx >= Define.RoomUserSlotCnt) return;

		Define.StPlayerInfo info = new Define.StPlayerInfo
		{
			idx = _idx,
			nickname = _nickname,
			characterChoice = _characterChoice,
			ip = _ip,
			privateIP = _privateIP,
			port = _port
		};

		m_arrPlayerInfo[_idx] = info;
	}

	public bool GetPlayerInfo(int _idx, out Define.StPlayerInfo _info)
	{
		_info = new Define.StPlayerInfo();
		if (_idx < 0 || _idx >= Define.RoomUserSlotCnt) return false;

		_info = m_arrPlayerInfo[_idx];

		return true;
	}

	public void AddPlayerAliveCnt() { ++PlayerAliveCnt; }

	public void UpdateTimer(float _deltaTime)
	{
		if (!IsTimerOn()) return;

		Timer += _deltaTime;
		//InGameConsole.Inst.Log($"Timer : {Timer}, {StartTime}");
	}

	public bool IsTimerOn()
	{
		return StartTime != 0;
	}

	public bool CheckStartTimer()
	{
		if (StartTime == 0) return false;
		if (StartTime > Timer) return false;
		
		GameStart = true;
		StartTime = 0;
		PlayersOnPortal = false;
		InGameConsole.Inst.Log("********** 게임 시작! *********");
		return true;
	}

	public bool StartGame()
	{
		if (m_playerReadyCnt < PlayerCnt - 1) return false;

		for (int i = 0; i < Define.RoomUserSlotCnt; ++i)
			m_playerReady[i] = false;
		m_playerReadyCnt = 0;
		return true;
	}

	public void SubPlayerCnt()
	{
		if (PlayerCnt <= 0) return;

		--PlayerCnt;
	}

	public void SubPlayerAliveCnt()
	{
		if (PlayerAliveCnt <= 0) return;

		--PlayerAliveCnt;
	}

	public void RemoveOtherPlayerSlot(int _idx)
	{
		OtherPlayersSlot.Remove(_idx);
	}

	public void SubMonsterCnt() 
	{
		if (MonsterCnt <= 0) return;
		
		--MonsterCnt; 
	}

	public void SetOtherPlayerSlot(List<int> _slotList)
	{
		OtherPlayersSlot = _slotList;
	}

	public void OnChangeStage()
	{
		if (!GameStart) return;
		if (!StageClear) return;

		m_inGameScene.SetClearImageVisible(false);
		GameStart = false;
		StageLoading = true;
		int i = 0;

		foreach (PlayerController pc in ObjectManager.Inst.Players)
		{
			if (pc == null) continue;

			pc.transform.position = m_positions[i++];
			pc.GetComponent<PlayerController>().OnChangeStage();
		}

		m_inGameScene.StartFadeInOutCoroutine(() => MapManager.Inst.LoadNextStage());
	}

	public void ChangeCamera(CinemachineVirtualCamera _vcam, int _cameraIdx)
	{
		int i = 0;
		foreach(PlayerController pc in ObjectManager.Inst.Players)
		{
			if (pc == null) continue;

			if (i == _cameraIdx)
			{
				_vcam.Follow = pc.transform;
				break;
			}
			++i;
		}
	}

	public void Clear()
	{
		PlayerCnt = 0;
		PlayerAliveCnt = 0;
		MonsterCnt = 0;
		GameStart = false;
		AllClear = false;
		m_playerReadyCnt = 0;
		StartTime = 0;
		Timer = 0f;
		m_playerReady = null;
		StageClear = false;
		GameOver = false;
		StageLoading = false;
		PlayersOnPortal = false;
	}
}
