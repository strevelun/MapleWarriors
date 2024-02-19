using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class DataManager : MonoBehaviour
{
	private static DataManager s_inst = null;
	public static DataManager Inst { get { return s_inst; } }

	Dictionary<string, MonsterData> m_monsterData = new Dictionary<string, MonsterData>();

	private void Awake()
	{
		s_inst = GetComponent<DataManager>();
		DontDestroyOnLoad(this);
		LoadAllData();
	}

	public MonsterData FindMonsterData(string _name)
	{
		MonsterData monster;
		if (m_monsterData.TryGetValue(_name, out monster)) return monster;

		return null;
	}

	void LoadAllData()
	{
		LoadMonsterData();

	}

	public void LoadMonsterData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "Data/MonsterData.json");
		string dataAsJson = File.ReadAllText(filePath);
		MonsterDataList loadedData = JsonUtility.FromJson<MonsterDataList>("{\"monsters\":" + dataAsJson + "}");

		foreach (MonsterData monster in loadedData.monsters)
		{
			m_monsterData.Add(monster.name, monster);
		}
	}
}
