using Newtonsoft.Json;
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
	Dictionary<string, SkillData> m_skillData = new Dictionary<string, SkillData>();
	Dictionary<string, MapData> m_mapData = new Dictionary<string, MapData>();

	private void Awake()
	{
		s_inst = GetComponent<DataManager>();
		DontDestroyOnLoad(this);
		LoadAllData();
	}

	public MonsterData FindMonsterData(string _name)
	{
		MonsterData monster = null;
		m_monsterData.TryGetValue(_name, out monster);

		return monster;
	}


	public SkillData FindSkillData(string _name)
	{
		SkillData skill = null;
		m_skillData.TryGetValue(_name, out skill);

		return skill;
	}

	public MapData FindMapData(string _name)
	{
		MapData map = null;
		m_mapData.TryGetValue(_name, out map);

		return map;
	}

	void LoadAllData()
	{
		LoadMonsterData();
		LoadSkillData();
		LoadMapData();
	}

	public void LoadMonsterData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "Data/MonsterData.json");
		string dataAsJson = File.ReadAllText(filePath);
		MonsterDataList loadedData = JsonConvert.DeserializeObject<MonsterDataList>(dataAsJson);

		foreach (MonsterData monster in loadedData.monsters)
		{
			m_monsterData.Add(monster.name, monster);
		}
	}

	public void LoadSkillData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "Data/SkillData.json");
		string dataAsJson = File.ReadAllText(filePath);
		SkillDataList loadedData = JsonConvert.DeserializeObject<SkillDataList>(dataAsJson);

		foreach (SkillData skill in loadedData.skills)
		{
			m_skillData.Add(skill.name, skill);
		}
	}

	public void LoadMapData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "Data/MapData.json");
		string dataAsJson = File.ReadAllText(filePath);
		MapDataList loadedData = JsonConvert.DeserializeObject<MapDataList>(dataAsJson);

		foreach (MapData map in loadedData.maps)
		{
			m_mapData.Add(map.name, map);
		}
	}
}
