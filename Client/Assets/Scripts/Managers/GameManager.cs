using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
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

	Vector3[] m_positions = new Vector3[] {
	new Vector3(2, -1),
	new Vector3(2, -3),
	new Vector3(4, -1),
	new Vector3(4, -3)
};

	Dictionary<string, GameObject> m_playerObj = new Dictionary<string, GameObject>();

	public bool GameStart { get; set; } = false;
	public bool StageLoading { get; set; } = false;
	public int PlayerCnt { get; private set; } = 0;
	public int PlayerAliveCnt { get; private set; } = 0;
	public int MonsterCnt { get; private set; } = 0;
	public bool AllClear { get; set; } = false;
	public bool StageClear { get; set; } = false;
	public bool GameOver { get; set; } = false;

	public List<int> OtherPlayersSlot { get; private set; }
	float[] m_observePlayersAwakeTimer = new float[Define.RoomUserSlot];
	bool[] m_playerReady = null;
	int m_playerReadyCnt = 0;

	long m_startTime;
	float m_timer;

	InGameScene m_inGameScene;

	public void Init(InGameScene _inGameScene)
	{
		m_inGameScene = _inGameScene;
		m_playerReady = new bool[Define.RoomUserSlot];
	}

	public bool CheckGameOver()
	{
		if (!GameStart) return false;
		if (PlayerCnt == 0) return true;
		if (PlayerAliveCnt == 0) return true;

		// 패배 UI 띄운 후 2초 후 대기실로 돌아감
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
	
	public void AddPlayerAliveCnt() { ++PlayerAliveCnt; }

	public void SetPlayerReady(int _slot)
	{
		if (m_playerReady[_slot]) return;

		m_playerReady[_slot] = true;
		++m_playerReadyCnt;
	}

	public void SetStartTime(long _startTime)
	{
		m_startTime = _startTime;
	}

	public void SetTimer(long _timer)
	{
		m_timer = _timer;
	}

	public void UpdateTimer(float _deltaTime)
	{
		m_timer += _deltaTime;
	}

	//public long GetCurTimer()
	////{
	//	return m_timer * 1000000;
	//}

	public bool IsTimerOn()
	{
		return m_startTime != 0;
	}

	public void UpdateStartTimer()
	{
		if (m_startTime == 0) return;

		long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		//InGameConsole.Inst.Log($"{(m_startTime - time < 0 ? 0 : m_startTime - time)} ms 후 시작");
		if (m_startTime <= time)
		{
			GameStart = true;
			m_startTime = 0;
			InGameConsole.Inst.Log("********** 게임 시작! *********");
		}
	}

	public bool StartGame()
	{
		if (m_playerReadyCnt < PlayerCnt - 1) return false;

		for (int i = 0; i < Define.RoomUserSlot; ++i)
			m_playerReady[i] = false;
		m_playerReadyCnt = 0;
		return true;
	}

	//public void AddMonsterCnt() { ++MonsterCnt; }

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

	public void AddPlayer(GameObject _playerObj)
	{
		m_playerObj.Add(_playerObj.name, _playerObj);
	}

	public void RemovePlayer(string _playerObjName)
	{
		Debug.Log($"{_playerObjName} 삭제완료");
		m_playerObj.Remove(_playerObjName);
	}

	public void ObservePlayers()
	{
		if (PlayerCnt == 1) return;

		foreach(int slot in OtherPlayersSlot)
		{
			m_observePlayersAwakeTimer[slot] += Time.deltaTime;
			if (m_observePlayersAwakeTimer[slot] > Define.MaxExpelTime)
			{
				//Packet pkt = InGamePacketMaker.RequestExpel(slot);
				//NetworkManager.Inst.Send(pkt);
			}
		}

		// 5초 넘어가면 서버에 추방 요청 패킷 전송
		
	}

	public void Awake(int _slot)
	{ 
		m_observePlayersAwakeTimer[_slot] = 0f;
	}

	public void OnChangeStage()
	{
		m_inGameScene.SetClearImageVisible(false);
		GameStart = false;
		StageLoading = true;
		int i = 0;

		foreach (GameObject obj in m_playerObj.Values)
		{
			obj.transform.position = m_positions[i++];
			obj.GetComponent<PlayerController>().OnChangeStage();
		}

		m_inGameScene.StartFadeInOutCoroutine(() => MapManager.Inst.LoadNextStage());
	}

	public void ChangeCamera(CinemachineVirtualCamera _vcam, int _cameraIdx)
	{
		int i = 0;
		foreach(GameObject pc in m_playerObj.Values)
		{
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
		m_playerObj.Clear();
		AllClear = false;
		m_playerReadyCnt = 0;
		m_startTime = 0;
		m_playerReady = null;
		StageClear = false;
		GameOver = false;
		StageLoading = false;
	}
}
