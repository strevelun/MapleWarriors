﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ObjectManager
{
	private static ObjectManager s_inst = null;
	public static ObjectManager Inst
	{
		get
		{
			if (s_inst == null) s_inst = new ObjectManager();
			return s_inst;
		}
	}

	Dictionary<string, PlayerController> m_dicPlayerObj = new Dictionary<string, PlayerController>();
	Dictionary<string, MonsterController> m_dicMonsterObj = new Dictionary<string, MonsterController>();

	int m_monsterIdx = 0;

	public void AddPlayer(string _key, GameObject _obj)
	{
		PlayerController pc = _obj.GetComponent<PlayerController>();
		//if (!pc) pc = _obj.AddComponent<PlayerController>();

		m_dicPlayerObj.Add(_key, pc);
	}

	public void AddMonster(GameObject _obj)
	{
		_obj.name = $"{_obj.name}_{m_monsterIdx++}";
		MonsterController pc = _obj.GetComponent<MonsterController>();
		//if (!pc) pc = _obj.AddComponent<MonsterController>();

		m_dicMonsterObj.Add(_obj.name, pc);
	}

	public PlayerController FindPlayer(string _key)
	{
		PlayerController pc = null;
		m_dicPlayerObj.TryGetValue(_key, out pc);

		return pc;
	}

	public MonsterController FindMonster(string _key)
	{
		MonsterController mc = null;
		m_dicMonsterObj.TryGetValue(_key, out mc);

		return mc;
	}

	public void RemovePlayer(string _key)
	{
		if (m_dicPlayerObj.Count == 0) return;

		m_dicPlayerObj.Remove(_key);
	}

	public void ClearPlayers()
	{
		m_dicPlayerObj.Clear();
	}

	public void ClearMonsters()
	{
		m_dicMonsterObj.Clear();
	}
}
