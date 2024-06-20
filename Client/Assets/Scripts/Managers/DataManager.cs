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

	private readonly Dictionary<Define.MonsterEnum, MonsterData> m_monsterData = new Dictionary<Define.MonsterEnum, MonsterData>();
	private readonly Dictionary<Define.SkillEnum, SkillData> m_skillData = new Dictionary<Define.SkillEnum, SkillData>();
	private readonly Dictionary<byte, MapData> m_mapData = new Dictionary<byte, MapData>();

	public IEnumerable<MapData> MapDataValues => m_mapData.Values;

	private void Awake()
	{
		s_inst = GetComponent<DataManager>();
		DontDestroyOnLoad(this);
		LoadAllData();
	}

	public MonsterData FindMonsterData(Define.MonsterEnum _name)
	{
		m_monsterData.TryGetValue(_name, out MonsterData monster);

		return monster;
	}

	public SkillData FindSkillData(Define.SkillEnum _name)
	{
		m_skillData.TryGetValue(_name, out SkillData skill);

		return skill;
	}

	public MapData FindMapData(byte _id)
	{ 
		m_mapData.TryGetValue(_id, out MapData map);

		return map;
	}

	private void LoadAllData()
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
			m_mapData.Add(byte.Parse(map.name), map);
		}
	}
}
