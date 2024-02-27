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


	public int PlayerCnt { get; private set; } = 0;
	int m_monsterCnt = 0;

	public void Update()
	{
		CheckGameOver();
	}

	void CheckGameOver()
	{
		if (PlayerCnt > 0) return;


	}

	public bool CheckMapClear()
	{
		if (m_monsterCnt == 0) return true;

		return false;
	}

	public void SetPlayerCnt(int _cnt) { PlayerCnt = _cnt; }
	public void SetMonsterCnt(int _cnt) { m_monsterCnt = _cnt; }

	public void SubPlayerCnt() { --PlayerCnt; }
	public void SubMonsterCnt() 
	{
		if (m_monsterCnt <= 0) return;
		
		--m_monsterCnt; 
	}
}
