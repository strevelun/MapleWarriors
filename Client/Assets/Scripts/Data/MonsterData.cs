using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class MonsterData 
{
	public int			idx;
	[JsonConverter(typeof(StringEnumConverter))]
	public MonsterEnum  name;
	public int			HP;
	public int			attack;
	public int			attackCellRange;	
	public float		speed;
	public int			visionCellRange;
	public int			maxHitPlayer;
	public int			hitboxWidth;
	public int			hitboxHeight;
}

[System.Serializable]
public class MonsterDataList
{
	public List<MonsterData> monsters;
}