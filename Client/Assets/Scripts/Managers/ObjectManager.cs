using System.Collections;
using System.Collections.Generic;
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

	public void Add<T>(string _key, GameObject _obj) where T : PlayerController
	{
		T pc = _obj.GetComponent<T>();
		if (!pc) pc = _obj.AddComponent<T>();

		m_dicPlayerObj.Add(_key, pc);
	}

	public PlayerController Find(string _key)
	{
		PlayerController pc = null;
		m_dicPlayerObj.TryGetValue(_key, out pc);
	
		return pc;
	}
}
