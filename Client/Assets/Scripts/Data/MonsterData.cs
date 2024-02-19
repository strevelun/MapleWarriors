using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterData 
{
	public string		name;
	public int			HP;
	public int			attack;
	public int			attackCellRange;	
	public float		speed;
	public int			visionCellRange;
}

[System.Serializable]
public class MonsterDataList
{
	public List<MonsterData> monsters;
}