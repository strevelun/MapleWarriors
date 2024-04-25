using System.Collections;
using System.Collections.Generic;
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
	public int PlayerCnt { get; private set; } = 0;
	public int PlayerAliveCnt { get; private set; } = 0;
	public int MonsterCnt { get; private set; } = 0;

	public List<int> OtherPlayersSlot { get; private set; }

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

	public void RemovePlayerSlot(int _idx)
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

	public void OnChangeStage()
	{
		int i = 0;
		foreach (GameObject obj in m_playerObj.Values)
		{
			obj.transform.position = m_positions[i++];
			obj.GetComponent<PlayerController>().OnChangeStage();
		}
	}

	public void Clear()
	{
		PlayerCnt = 0;
		MonsterCnt = 0;
		GameStart = false;
		m_playerObj.Clear();
	}
}
