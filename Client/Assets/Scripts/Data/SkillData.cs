﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using static Define;

[System.Serializable]
public class SkillData
{
	[JsonConverter(typeof(StringEnumConverter))]
	public SkillTypeEnum type;
	[JsonConverter(typeof(StringEnumConverter))]
	public SkillEnum name;
    public int MP;
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