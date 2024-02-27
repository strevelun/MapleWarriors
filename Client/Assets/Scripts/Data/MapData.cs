using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData 
{
    public string name { get; set; }
	public List<string> mapList { get; set; }
}

public class MapDataList
{
	public List<MapData> maps { get; set; }
}