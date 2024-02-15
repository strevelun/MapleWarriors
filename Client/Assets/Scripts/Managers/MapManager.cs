using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager
{
	private static MapManager s_inst = null;
	public static MapManager Inst { get
		{
			if (s_inst == null) s_inst = new MapManager(); 
			return s_inst; 
		} 
	}

	public Grid CurMap { get; private set; }
	Tilemap m_tmBase, m_tmCollision;

	public int MinX { get; private set; }
	public int MaxX { get; private set; }
	public int MinY { get; private set; }
	public int MaxY { get; private set; }

	public int XSize { get; private set; }
	public int YSize { get; private set; }

	bool[,] m_collisionMap;

	// Map_1_1, Map_1_2, Map_1_3
	public void Load(int _mapID, int _stageID)
	{

		string name = "Map_" + _mapID + "_" + _stageID;
		GameObject go = ResourceManager.Inst.Instantiate($"Map/{name}");
		go.name = "Map";


		CreateCollisionMap(go);

		CurMap = go.GetComponent<Grid>();
	}

	private void CreateCollisionMap(GameObject _prefabMap)
	{
		m_tmBase = Util.FindChild<Tilemap>(_prefabMap, true, "TM_Base");
		m_tmCollision = Util.FindChild<Tilemap>(_prefabMap, true, "TM_Collision");

		m_tmBase.CompressBounds();

		MinX = m_tmBase.cellBounds.xMin;
		MaxX = m_tmBase.cellBounds.xMax;
		MinY = m_tmBase.cellBounds.yMin;
		MaxY = m_tmBase.cellBounds.yMax;
		YSize = MaxY - MinY;
		XSize = MaxX - MinX;

		m_collisionMap = new bool[YSize, XSize];

		for(int y = MinY, mapY = YSize-1; y < MaxY; ++y, --mapY)
		{
			for(int x = MinX, mapX = 0; x < MaxX; ++x, ++mapX)
			{
				TileBase t = m_tmCollision.GetTile(new Vector3Int(x, y, 0));
				m_collisionMap[mapY, mapX] = t ? true : false;
				//Vector3 v = tmBase.CellToWorld(new Vector3Int(x, y, 0));
				//Debug.Log(v);
			}
		}
		m_tmCollision.gameObject.SetActive(false);
	}

	public bool IsBlocked(int _cellXPos, int _cellYPos)
	{
		if (_cellXPos < 0 || _cellYPos < 0 || _cellXPos >= XSize || _cellYPos >= YSize) return true;

		return m_collisionMap[_cellYPos, _cellXPos];
	}

	public Vector3Int WorldToCell(float _xpos, float _ypos)
	{
		return m_tmBase.WorldToCell(new Vector3(_xpos, -_ypos));
	}

	public void Destroy()
	{
		Object.Destroy(CurMap.gameObject);
		CurMap = null;
		MinX = 0;
		MaxX = 0;
		MinY = 0;
		MaxY = 0;
		YSize = 0;
		XSize = 0;
	}
}
