using System.Collections;
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

	private readonly Dictionary<int, PlayerController> m_dicPlayerObj = new Dictionary<int, PlayerController>();
	private readonly Dictionary<int, MonsterController> m_dicMonsterObj = new Dictionary<int, MonsterController>();

	public IReadOnlyDictionary<int, PlayerController> Players => m_dicPlayerObj;
	public IReadOnlyDictionary<int, MonsterController> Monsters => m_dicMonsterObj;

	private int m_monsterNum = 0;

	public void AddPlayer(int _keyIdx, GameObject _obj)
	{
		PlayerController pc = _obj.GetComponent<PlayerController>();

		m_dicPlayerObj.Add(_keyIdx, pc);
	}

	public void AddMonster(GameObject _obj, int _idx)
	{
		++m_monsterNum;
		int key = (_idx << 16) | m_monsterNum;
		_obj.name = $"{_idx}_{m_monsterNum}";
		MonsterController mc = _obj.GetComponent<MonsterController>();
		mc.Num = m_monsterNum;

		m_dicMonsterObj.Add(key, mc);
	}

	public PlayerController FindPlayer(int _idx)
	{
		PlayerController pc;
		m_dicPlayerObj.TryGetValue(_idx, out pc);

		return pc;
	}

	public MonsterController FindMonster(int _idx, int _num)
	{
		MonsterController mc;
		int key = (_idx << 16) | _num;
		m_dicMonsterObj.TryGetValue(key, out mc);

		return mc;
	}

	public void RemovePlayer(int _idx)
	{
		if (m_dicPlayerObj.Count == 0) return;

		m_dicPlayerObj.Remove(_idx);
	}

	public void ClearPlayers()
	{
		m_dicPlayerObj.Clear();
	}

	public void ClearMonsters()
	{
		m_dicMonsterObj.Clear();
		m_monsterNum = 0;
	}
}