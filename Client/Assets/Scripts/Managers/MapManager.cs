using Cinemachine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

	GameObject m_camObj;

	MapData m_mapData = null;
	int m_mapID;
	public int MaxStage { get; private set; } = 0;
	public int CurStage { get; private set; } = 0;
	GameObject m_map = null;

	public Grid CurMap { get; private set; }
	Tilemap m_tmBase, m_tmCollision, m_tmAim, m_tmHitbox;
	Tile m_aimTile, m_hitboxTile;

	public int MinX { get; private set; }
	public int MaxX { get; private set; }
	public int MinY { get; private set; }
	public int MaxY { get; private set; }

	public int XSize { get; private set; }
	public int YSize { get; private set; }

	bool[,] m_collisionMap;
	bool[,] m_monsterCollision;
	MonsterController[,] m_monsterMap;

	public GameObject Load(int _mapID, GameObject _camObj)
	{
		if (m_mapData != null) return null;

		m_mapID = _mapID;
		m_camObj = _camObj;

		string name = "Map_" + m_mapID;
		m_mapData = DataManager.Inst.FindMapData((byte)m_mapID);
		name = m_mapData.mapList[CurStage];
		m_map = ResourceManager.Inst.Instantiate($"Map/{name}");
		//go.name = "Map";
		++CurStage;

		MaxStage = m_mapData.mapList.Count;

		TileInit();

		return m_map;
	}

	void TileInit()
	{
		m_tmBase = Util.FindChild<Tilemap>(m_map, true, "TM_Base");
		m_tmBase.CompressBounds();
		MinX = m_tmBase.cellBounds.xMin;
		MaxX = m_tmBase.cellBounds.xMax;
		MinY = m_tmBase.cellBounds.yMin;
		MaxY = m_tmBase.cellBounds.yMax;
		YSize = MaxY - MinY;
		XSize = MaxX - MinX;

		Debug.Log($"MinX : {MinX}, MaxX : {MaxX}, MinY : {MinY}, MaxY : {MaxY}, XSize : {XSize}, YSize : {YSize}");

		CreateMapBound();
		CreateCollisionMap(m_map);

		CurMap = m_map.GetComponent<Grid>();

		m_tmAim = Util.FindChild<Tilemap>(m_map, true, "TM_Aim");
		m_aimTile = ScriptableObject.CreateInstance<Tile>();
		m_aimTile.sprite = ResourceManager.Inst.LoadSprite("Etc/Aim");

		m_tmHitbox = Util.FindChild<Tilemap>(m_map, true, "TM_Hitbox");
		m_hitboxTile = ScriptableObject.CreateInstance<Tile>();
		m_hitboxTile.sprite = ResourceManager.Inst.LoadSprite("Etc/Hitbox");
	}

	void CreateCollisionMap(GameObject _prefabMap)
	{
		m_tmCollision = Util.FindChild<Tilemap>(_prefabMap, true, "TM_Collision");

		m_collisionMap = new bool[YSize, XSize];
		m_monsterCollision = new bool[YSize, XSize];
		m_monsterMap = new MonsterController[YSize, XSize];
		for (int i = 0; i < YSize; ++i)
		{
			for (int j = 0; j < XSize; ++j)
			{
				m_monsterMap[i, j] = new MonsterController();
			}
		}

		for (int y = MinY, mapY = YSize-1; y < MaxY; ++y, --mapY)
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

	void CreateMapBound()
	{
		CinemachineConfiner confiner = m_camObj.GetComponent<CinemachineConfiner>();
		GameObject collider = ResourceManager.Inst.Instantiate("Map/MapBound");
		PolygonCollider2D polyCollider = collider.GetComponent<PolygonCollider2D>();
		Vector2[] points = polyCollider.points;
		points[0] = new Vector2(0, 1);
		points[1] = new Vector2(XSize, 1);
		points[2] = new Vector2(XSize, -(YSize - 1));
		points[3] = new Vector2(0, -(YSize - 1));
		polyCollider.points = points;
		confiner.m_BoundingShape2D = polyCollider;
		confiner.enabled = false;
		confiner.enabled = true;
	}

	public void LoadNextStage()
	{
		if (m_map == null) return;

		ObjectManager.Inst.ClearMonsters();
		ResourceManager.Inst.Destroy(m_map);

		string name = m_mapData.mapList[CurStage];
		m_map = ResourceManager.Inst.Instantiate($"Map/{name}");
		++CurStage;
	
		GameObject monsters = Util.FindChild(m_map, false, "Monsters");
		int activeCnt = 0;
		for (int i = 0; i < monsters.transform.childCount; i++)
		{
			if (monsters.transform.GetChild(i).gameObject.activeSelf)
			{
				activeCnt++;
			}
		}
		GameManager.Inst.SetMonsterCnt(activeCnt);
		
		TileInit();

		Debug.Log($"플레이어 수 : {GameManager.Inst.PlayerCnt}");
		Debug.Log($"몬스터 수 : {GameManager.Inst.MonsterCnt}");
		Debug.Log($"현재 스테이지 : {CurStage}");
	}

	public bool IsBlocked(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;
		if (_cellXPos < 0 || _cellYPos - hitboxHeight < 0 || _cellXPos + hitboxWidth >= XSize || _cellYPos >= YSize) return true;

		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for(int x= _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				if (m_collisionMap[y, x])
				{
					return m_collisionMap[y, x];
				}
			}
		}

		return false;
	}

	public Vector2Int WorldToCell(float _xpos, float _ypos)
	{
		Vector3Int v = m_tmBase.WorldToCell(new Vector3(_xpos, -_ypos));
		return new Vector2Int(v.x, v.y);
	}

	public MonsterController GetMonsters(int _cellXPos, int _cellYPos)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return null;
		if (m_monsterMap[_cellYPos, _cellXPos] == null) return null;

		return m_monsterMap[_cellYPos, _cellXPos];
	}

	public void AddMonster(MonsterController _mc)
	{
		if (IsBlocked(_mc.CellPos.x, _mc.CellPos.y, _mc.HitboxWidth, _mc.HitboxHeight)) return;

		int hitboxWidth = _mc.HitboxWidth - 1;
		int hitboxHeight = _mc.HitboxHeight - 1;

		for (int y = _mc.CellPos.y; y >= _mc.CellPos.y - hitboxHeight; --y)
		{
			for (int x = _mc.CellPos.x; x <= _mc.CellPos.x + hitboxWidth; ++x)
			{
				m_monsterMap[y, x] = _mc;
				//m_tmHitbox.SetTile(new Vector3Int(x, -y, 0), m_hitboxTile);
				//Debug.Log($"AddMonster : {_mc.transform.position}, {x}, {y}");
			}
		}

	}

	public void RemoveMonster(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return;
		if (m_monsterMap[_cellYPos, _cellXPos] == null) return;

		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;
		// 3,6 3,7 3,8
		// 5,6 5,7 5,8
		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for (int x = _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				if (m_monsterMap[y, x] != null)
				{
					MonsterController mc = m_monsterMap[y, x];
					m_monsterMap[y, x] = null;
					//m_tmHitbox.SetTile(new Vector3Int(x, -y, 0), null);
					//Debug.Log($"RemoveMonster : {mc.transform.position}, {x}, {y}, Remains : {m_monsterMap[y,x].Count}");
				}
			}
		}
	}

	public bool SetAimTile(int _cellXPos, int _cellYPos)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return false;

		m_tmAim.SetTile(new Vector3Int(_cellXPos, -_cellYPos, 0), m_aimTile);
		return true;
	}

	public void RemoveAimTile(int _cellXPos, int _cellYPos)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return;

		m_tmAim.SetTile(new Vector3Int(_cellXPos, -_cellYPos, 0), null);
	}

	public void SetHitboxTile(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		if (IsBlocked(_cellXPos, _cellYPos, _hitboxWidth, _hitboxHeight)) return;

		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;

		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for (int x = _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				m_tmHitbox.SetTile(new Vector3Int(x, -y, 0), m_hitboxTile);
			}
		}
	}

	public void RemoveHitboxTile(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		if (IsBlocked(_cellXPos, _cellYPos, _hitboxWidth, _hitboxHeight)) return;

		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;

		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for (int x = _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				m_tmHitbox.SetTile(new Vector3Int(x, -y, 0), null);
			}
		}
	}

	public void SetMonsterCollision(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight, bool _flag)
	{
		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;

		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for (int x = _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				m_monsterCollision[y, x] = _flag;
				//m_tmHitbox.SetTile(new Vector3Int(x, -y, 0), _flag ? m_hitboxTile : null);
			}
		}
	}

	public bool IsMonsterCollision(int _cellDestXPos, int _cellDestYPos, int _hitboxWidth, int _hitboxHeight)
	{
		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;

		for (int y = _cellDestYPos; y >= _cellDestYPos - hitboxHeight; --y)
		{
			for (int x = _cellDestXPos; x <= _cellDestXPos + hitboxWidth; ++x)
			{
				if (m_monsterCollision[y, x])
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Destroy()
	{
		//Object.Destroy(CurMap.gameObject);
		//Object.Destroy(m_map);
		CurMap = null;
		MinX = 0;
		MaxX = 0;
		MinY = 0;
		MaxY = 0;
		YSize = 0;
		XSize = 0;
		m_mapData = null;
		m_map = null;
		CurStage = 0;
		m_mapID = 0;
		//m_tmBase = null;
		//m_tmCollision = null;
		//m_tmAim = null;
		//m_tmHitbox = null;
		MaxStage = 0;
	}
}
