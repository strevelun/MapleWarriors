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
	Tilemap m_tmBase, m_tmCollision, m_tmAim, m_tmHitbox;
	Tile m_aimTile, m_hitboxTile;

	public int MinX { get; private set; }
	public int MaxX { get; private set; }
	public int MinY { get; private set; }
	public int MaxY { get; private set; }

	public int XSize { get; private set; }
	public int YSize { get; private set; }

	bool[,] m_collisionMap;
	Queue<MonsterController>[,] m_monsterMap;

	// Map_1_1, Map_1_2, Map_1_3
	public void Load(int _mapID, int _stageID)
	{

		string name = "Map_" + _mapID + "_" + _stageID;
		GameObject go = ResourceManager.Inst.Instantiate($"Map/{name}");
		go.name = "Map";


		CreateCollisionMap(go);

		CurMap = go.GetComponent<Grid>();

		m_tmAim = Util.FindChild<Tilemap>(go, true, "TM_Aim");
		m_aimTile = ScriptableObject.CreateInstance<Tile>();
		m_aimTile.sprite = ResourceManager.Inst.LoadSprite("Etc/Aim");

		m_tmHitbox = Util.FindChild<Tilemap>(go, true, "TM_Hitbox");
		m_hitboxTile = ScriptableObject.CreateInstance<Tile>();
		m_hitboxTile.sprite = ResourceManager.Inst.LoadSprite("Etc/Hitbox");
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
		m_monsterMap = new Queue<MonsterController>[YSize, XSize];
		for (int i = 0; i < YSize; ++i)
		{
			for (int j = 0; j < XSize; ++j)
			{
				m_monsterMap[i, j] = new Queue<MonsterController>();
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

	public bool IsBlocked(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;
		if (_cellXPos < 0 || _cellYPos - hitboxHeight < 0 || _cellXPos + hitboxWidth >= XSize || _cellYPos >= YSize) return true;

		for(int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
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

	public Queue<MonsterController> GetMonsters(int _cellXPos, int _cellYPos)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return null;
		if (m_monsterMap[_cellYPos, _cellXPos].Count == 0) return null;

		return new Queue<MonsterController>(m_monsterMap[_cellYPos, _cellXPos]);
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
				m_monsterMap[y, x].Enqueue(_mc);
			}
		}

	}

	public void RemoveMonster(int _cellXPos, int _cellYPos, int _hitboxWidth, int _hitboxHeight)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return;
		if (m_monsterMap[_cellYPos, _cellXPos].Count == 0) return;

		int hitboxWidth = _hitboxWidth - 1;
		int hitboxHeight = _hitboxHeight - 1;
		// 3,6 3,7 3,8
		// 5,6 5,7 5,8
		for (int y = _cellYPos; y >= _cellYPos - hitboxHeight; --y)
		{
			for (int x = _cellXPos; x <= _cellXPos + hitboxWidth; ++x)
			{
				m_monsterMap[y, x].Dequeue();
			}
		}
	}

	public void SetAimTile(int _cellXPos, int _cellYPos)
	{
		if (IsBlocked(_cellXPos, _cellYPos, 1, 1)) return;

		m_tmAim.SetTile(new Vector3Int(_cellXPos, -_cellYPos, 0), m_aimTile);
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
