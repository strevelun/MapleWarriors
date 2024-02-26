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


	int m_playerCnt = 0;
	int m_monsterCnt = 0;

	public void Update()
	{
		CheckGameOver();
	}

	void CheckGameOver()
	{
		if (m_playerCnt > 0) return;


	}

	public bool CheckMapClear()
	{
		if (m_monsterCnt > 0) return true;

		return false;
	}

	public void AddPlayerCnt() { ++m_playerCnt; }
	public void AddMonsterCnt() { ++m_monsterCnt; }

	public void SubPlayerCnt() { --m_playerCnt; }
	public void SubMonsterCnt() { --m_monsterCnt; }
}
