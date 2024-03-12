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

	public bool GameStart { get; set; } = false;
	public int PlayerCnt { get; private set; } = 0;
	public int MonsterCnt { get; private set; } = 0;

	public bool CheckGameOver()
	{
		if (!GameStart) return false;
		if (PlayerCnt > 0) return false;

		// 패배 UI 띄운 후 2초 후 대기실로 돌아감
		return true;
	}

	public bool CheckMapClear()
	{
		if (!GameStart) return false;
		if (MonsterCnt > 0) return false;

		return true;
	}

	public void SetPlayerCnt(int _cnt) { PlayerCnt = _cnt; }
	public void SetMonsterCnt(int _cnt) { MonsterCnt = _cnt; }

	//public void AddMonsterCnt() { ++MonsterCnt; }

	public void SubPlayerCnt() 
	{
		if (PlayerCnt <= 0) return; 

		--PlayerCnt; 
	}
	public void SubMonsterCnt() 
	{
		if (MonsterCnt <= 0) return;
		
		--MonsterCnt; 
	}

	public void Clear()
	{
		PlayerCnt = 0;
		MonsterCnt = 0;
	}
}
