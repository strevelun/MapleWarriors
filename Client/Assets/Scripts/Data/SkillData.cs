using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData
{
    public string name;
    public int mp;
    public int hitCount;
    public int attack;
    public int attackCellRange;
    public int attackRadius;
    public float hitTime;
}

[System.Serializable]
public class SkillDataList
{
	public List<SkillData> skills;
}