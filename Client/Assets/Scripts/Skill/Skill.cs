using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Skill
{
	SkillData m_skillData;
	Vector2Int m_prevMouseCellPos = Vector2Int.zero;
	Vector2Int m_dist;
	Vector2Int m_mouseCellPos;
	Vector2Int m_cellPos, m_lastCellPos;
	eDir m_eDir;
	public eSkill eCurSkill { get; private set; } = eSkill.Slash;

	Queue<Vector2Int> m_aims = new Queue<Vector2Int>();

	public Skill(eSkill _skill)
	{
		m_skillData = DataManager.Inst.FindSkillData(_skill.ToString());
	}

	public void SetSkill(eSkill _skill)
	{
		eCurSkill = _skill;
		RemoveAimTiles();

		m_skillData = DataManager.Inst.FindSkillData(eCurSkill.ToString());
		if (m_skillData.type == eSkillType.Melee && m_skillData.attackRadius >= 2) 
			m_skillData.attackRadius = 2;
	}

	public eSkillType GetSkillType()
	{
		return m_skillData.type;
	}

	public string GetSkillName()
	{
		return m_skillData.name;
	}

	public bool Activate(List<MonsterController> _targets)
	{
		bool activated = false;

		if (CheckAttackRange(m_dist.x, m_dist.y))
		{
			activated = true;
			FindHitTargets(_targets);
		}

		return activated;
	}

	public void Play(PlayerController _pc)
	{
		_pc.PlayCurSkillAnim(this);
	}

	public int GetDamage()
	{
		return m_skillData.attack;
	}

	void FindHitTargets(List<MonsterController> _targets)
	{
		int cnt = 0;
		MonsterController mc = null;
		int r = m_skillData.attackRadius - 1;
		HashSet<MonsterController> hsMonsters = new HashSet<MonsterController>();

		switch (m_skillData.type)
		{
			case eSkillType.Melee:
				{
					if (m_eDir == eDir.None) break;

					int startX = 0, endX = 0, startY = 0, endY = 0;
					int pnt = 0;

					if (m_eDir == eDir.Left || m_eDir == eDir.UpLeft)
					{
						pnt = m_cellPos.y;
						startX = m_mouseCellPos.x;
						endX = m_cellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == eDir.Right || m_eDir == eDir.DownRight)
					{
						pnt = m_cellPos.y;
						startX = m_cellPos.x;
						endX = m_mouseCellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == eDir.Up || m_eDir == eDir.UpRight)
					{
						pnt = m_cellPos.x;
						startY = m_mouseCellPos.y;
						endY = m_cellPos.y;
						startX = pnt - (m_skillData.attackRadius - 1);
						endX = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == eDir.Down || m_eDir == eDir.DownLeft)
					{
						pnt = m_cellPos.x;
						startY = m_cellPos.y;
						endY = m_mouseCellPos.y;
						startX = pnt - (m_skillData.attackRadius - 1);
						endX = pnt + (m_skillData.attackRadius - 1);
					}

					for (int i = startX; i <= endX; ++i)
					{
						for (int j = startY; j <= endY; ++j)
						{
							mc = MapManager.Inst.GetMonsters(i, j);
							if (mc == null) continue;

							if (!hsMonsters.Contains(mc))
							{
								_targets.Add(mc);
								hsMonsters.Add(mc);
								++cnt;

								if (cnt >= m_skillData.hitCount) return;
							}
						}
					}
				}
				break;
			case eSkillType.Ranged:
				{
					for (int i = m_mouseCellPos.y - r; i <= m_mouseCellPos.y + r; ++i)
					{
						for (int j = m_mouseCellPos.x - r; j <= m_mouseCellPos.x + r; ++j)
						{
							mc = MapManager.Inst.GetMonsters(j, i);
							if (mc == null) continue;

							if (!hsMonsters.Contains(mc))
							{
								_targets.Add(mc);
								hsMonsters.Add(mc);
								++cnt;

								if (cnt >= m_skillData.hitCount) return;
							}
						}
					}
				}
				break;
			case eSkillType.Auto:
				break;
		}
	}

	public void SetDist(Vector2Int _mouseCellPos, Vector2Int _cellPos, Define.eDir _eDir)
	{
		m_eDir = _eDir;
		m_cellPos = _cellPos;
		m_dist = _mouseCellPos - _cellPos;
		m_mouseCellPos = _mouseCellPos;
	}


	public void Update(Vector2Int _cellPos, Define.eDir _eDir)
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		mousePos.y = -mousePos.y + 1.0f;
		m_mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);

		SetDist(m_mouseCellPos, _cellPos, _eDir);

		if (m_cellPos != _cellPos)
		{
			RemoveAimTiles();
			m_cellPos = _cellPos;
			if (CheckAttackRange(m_dist.x, m_dist.y))
				SetAimTiles();
		}

		if (m_prevMouseCellPos != m_mouseCellPos)
		{
			m_cellPos = _cellPos;
			RemoveAimTiles();

			if (CheckAttackRange(m_dist.x, m_dist.y))
				SetAimTiles();
			m_prevMouseCellPos = m_mouseCellPos;
		}
	}

	bool CheckAttackRange(int _distX, int _distY)
	{
		if (GetSkillType() == eSkillType.Melee)
		{
			if ((m_eDir == eDir.Left || m_eDir == eDir.UpLeft) && -m_skillData.attackCellRange <= _distX && _distX <= 0) return true;
			if ((m_eDir == eDir.Right || m_eDir == eDir.DownRight) && 0 <= _distX && _distX <= m_skillData.attackCellRange) return true;
			if ((m_eDir == eDir.Up || m_eDir == eDir.UpRight) && -m_skillData.attackCellRange <= _distY && _distY <= 0) return true;
			if ((m_eDir == eDir.Down || m_eDir == eDir.DownLeft) && 0 <= _distY && _distY <= m_skillData.attackCellRange) return true;
		}
		else if (GetSkillType() == eSkillType.Ranged)
		{
			if ((-m_skillData.attackCellRange <= _distX && _distX <= m_skillData.attackCellRange)
				&& (-m_skillData.attackCellRange <= _distY && _distY <= m_skillData.attackCellRange)) return true;
		}
		return false;
	}

	public void RemoveAimTiles()
	{
		switch(m_skillData.type)
		{
			case eSkillType.Melee:
				{
					while (m_aims.Count > 0)
					{
						Vector2Int aim = m_aims.Peek();
						MapManager.Inst.RemoveAimTile(aim.x, aim.y);
						m_aims.Dequeue();
					}
				}
				break;
			case eSkillType.Ranged:
				{
					while (m_aims.Count > 0)
					{
						Vector2Int aim = m_aims.Peek();
						MapManager.Inst.RemoveAimTile(aim.x, aim.y);
						m_aims.Dequeue();
					}
				}
				break;
			case eSkillType.Auto:
				break;
		}
		
	}

	void SetAimTiles()
	{
		switch (m_skillData.type)
		{
			case eSkillType.Melee:
				{
					// FindDir();

					if (m_eDir == eDir.None) break;

					int startX = 0, endX = 0, startY = 0, endY = 0;
					int pnt = 0;

					if (m_eDir == eDir.Left || m_eDir == eDir.UpLeft)
					{
						pnt = m_cellPos.y;
						startX = m_cellPos.x;
						endX = m_mouseCellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);

						for (int j = startY; j <= endY; ++j)
						{
							for (int i = startX; i >= endX; --i)
							{
								if (MapManager.Inst.SetAimTile(i, j) == false)
								{
									break;
								}
								m_aims.Enqueue(new Vector2Int(i, j));
							}
						}
					}
					else if(m_eDir == eDir.Right || m_eDir == eDir.DownRight)
					{
						pnt = m_cellPos.y;
						startX = m_cellPos.x;
						endX = m_mouseCellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);
						
						for (int j = startY; j <= endY; ++j)
						{
							for (int i = startX; i <= endX; ++i)
							{
								if (MapManager.Inst.SetAimTile(i, j) == false)
								{
									break;
								}
								m_aims.Enqueue(new Vector2Int(i, j));
							}
						}
					}
					else if (m_eDir == eDir.Up || m_eDir == eDir.UpRight)
					{
						pnt = m_cellPos.x;
						startY = m_cellPos.y;
						endY = m_mouseCellPos.y;
						startX = pnt - (m_skillData.attackRadius - 1);
						endX = pnt + (m_skillData.attackRadius - 1);

						for (int i = startX; i <= endX; ++i)
						{
							for (int j = startY; j >= endY; --j)
							{
								if (MapManager.Inst.SetAimTile(i, j) == false)
								{
									break;
								}
								m_aims.Enqueue(new Vector2Int(i, j));
							}
						}
					}
					else if (m_eDir == eDir.Down || m_eDir == eDir.DownLeft)
					{
						pnt = m_cellPos.x;
						startY = m_cellPos.y;
						endY = m_mouseCellPos.y;
						startX = pnt - (m_skillData.attackRadius - 1);
						endX = pnt + (m_skillData.attackRadius - 1);

						for (int i = startX; i <= endX; ++i)
						{
							for (int j = startY; j <= endY; ++j)
							{
								if (MapManager.Inst.SetAimTile(i, j) == false)
								{
									break;
								}
								m_aims.Enqueue(new Vector2Int(i, j));
							}
						}
					}
				}
				break;
			case eSkillType.Ranged:
				{
					int r = m_skillData.attackRadius - 1;
					for (int i = m_mouseCellPos.y - r; i <= m_mouseCellPos.y + r; ++i)
						for (int j = m_mouseCellPos.x - r; j <= m_mouseCellPos.x + r; ++j)
						{
							MapManager.Inst.SetAimTile(j, i);
							m_aims.Enqueue(new Vector2Int(j, i));
						}
				}
				break;
			case eSkillType.Auto:
				break;
		}
	}

	void FindDir()
	{
		eDir dir;

		if (m_dist.x <= -1 && -1 <= m_dist.y && m_dist.y <= 1) dir = eDir.Left;
		else if (m_dist.x >= 1 && -1 <= m_dist.y && m_dist.y <= 1) dir = eDir.Right;
		else if (m_dist.y <= -1 && -1 <= m_dist.x && m_dist.x <= 1) dir = eDir.Up;
		else if (m_dist.y >= 1 && -1 <= m_dist.x && m_dist.x <= 1) dir = eDir.Down;
		else dir = eDir.None;

		if(dir != m_eDir)
		{
			//m_eLastDir = m_eDir;
			m_eDir = dir;
		}

		//Debug.Log(m_eDir);
	}
}
