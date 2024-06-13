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

	private readonly PlayerController[] m_arrPlayer = new PlayerController[Define.RoomUserSlotCnt];
	private readonly Dictionary<int, MonsterController> m_dicMonsterObj = new Dictionary<int, MonsterController>();

	public IEnumerable<PlayerController> Players => m_arrPlayer;
	public IEnumerable<MonsterController> Monsters => m_dicMonsterObj.Values;

	public int PlayerNum { get; private set; } = 0;
	public int MonsterNum { get; private set; } = 0;

	public void AddPlayer(int _idx, GameObject _obj)
	{
		if (_idx < 0 || _idx >= Define.RoomUserSlotCnt) return;

		PlayerController pc = _obj.GetComponent<PlayerController>();
		
		++PlayerNum;
		m_arrPlayer[_idx] = pc;
	}

	public void AddMonster(GameObject _obj, int _idx)
	{
		++MonsterNum;
		int key = (_idx << 16) | MonsterNum;
		_obj.name = $"{_idx}_{MonsterNum}";
		MonsterController mc = _obj.GetComponent<MonsterController>();
		mc.Num = MonsterNum;

		m_dicMonsterObj.Add(key, mc);
	}

	public PlayerController FindPlayer(int _idx)
	{
		if (_idx < 0 || _idx >= Define.RoomUserSlotCnt) return null;

		return m_arrPlayer[_idx];
	}

	public MonsterController FindMonster(int _idx, int _num)
	{
		int key = (_idx << 16) | _num;
		m_dicMonsterObj.TryGetValue(key, out MonsterController mc);

		return mc;
	}

	public void RemovePlayer(int _idx)
	{
		if (PlayerNum == 0) return;

		--PlayerNum;
		m_arrPlayer[_idx] = null;
	}

	public void ClearPlayers()
	{
		if (PlayerNum == 0) return;

		PlayerNum = 0;
		for (int i = 0; i < Define.RoomUserSlotCnt; ++i)
			m_arrPlayer[i] = null;
	}

	public void ClearMonsters()
	{
		m_dicMonsterObj.Clear();
		MonsterNum = 0;
	}
}