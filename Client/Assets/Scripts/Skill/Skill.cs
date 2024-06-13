using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Skill
{
	private SkillData m_skillData;
	private Vector2Int m_prevMouseCellPos = Vector2Int.zero;
	private Vector2Int m_dist;
	private Vector2Int m_mouseCellPos;
	private Vector2Int m_cellPos, m_lastCellPos = new Vector2Int(0,0);
	private DirEnum m_eDir;
	public SkillEnum CurSkill { get; private set; } = SkillEnum.Slash;

	private readonly Queue<Vector2Int> m_aims = new Queue<Vector2Int>();

	public Skill(SkillEnum _skill)
	{
		m_skillData = DataManager.Inst.FindSkillData(_skill);
	}

	public void SetSkill(SkillEnum _skill)
	{
		CurSkill = _skill;
		RemoveAimTiles();

		m_skillData = DataManager.Inst.FindSkillData(CurSkill);
		if (m_skillData.type == SkillTypeEnum.Melee && m_skillData.attackRadius >= 2) 
			m_skillData.attackRadius = 2;
	}

	public SkillTypeEnum GetSkillType()
	{
		return m_skillData.type;
	}

	public string GetSkillName()
	{
		return m_skillData.name.ToString();
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

	private void FindHitTargets(List<MonsterController> _targets)
	{
		int cnt = 0;
		MonsterController mc;
		int r = m_skillData.attackRadius - 1;
		HashSet<MonsterController> hsMonsters = new HashSet<MonsterController>();

		switch (m_skillData.type)
		{
			case SkillTypeEnum.Melee:
				{
					if (m_eDir == DirEnum.None) break;

					int startX = 0, endX = 0, startY = 0, endY = 0;
					int pnt;

					if (m_eDir == DirEnum.Left || m_eDir == DirEnum.UpLeft)
					{
						pnt = m_cellPos.y;
						startX = m_mouseCellPos.x;
						endX = m_cellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == DirEnum.Right || m_eDir == DirEnum.DownRight)
					{
						pnt = m_cellPos.y;
						startX = m_cellPos.x;
						endX = m_mouseCellPos.x;
						startY = pnt - (m_skillData.attackRadius - 1);
						endY = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == DirEnum.Up || m_eDir == DirEnum.UpRight)
					{
						pnt = m_cellPos.x;
						startY = m_mouseCellPos.y;
						endY = m_cellPos.y;
						startX = pnt - (m_skillData.attackRadius - 1);
						endX = pnt + (m_skillData.attackRadius - 1);
					}
					else if (m_eDir == DirEnum.Down || m_eDir == DirEnum.DownLeft)
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
			case SkillTypeEnum.Ranged:
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
			case SkillTypeEnum.Auto:
				break;
		}
	}

	public void SetDist(Vector2Int _mouseCellPos, Vector2Int _cellPos, Define.DirEnum _eDir)
	{
		m_eDir = _eDir;
		m_cellPos = _cellPos;
		m_dist = _mouseCellPos - _cellPos;
		m_mouseCellPos = _mouseCellPos;
	}

	public void Update(Vector2Int _cellPos, Define.DirEnum _eDir)
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

		if (m_prevMouseCellPos != m_mouseCellPos || m_cellPos != m_lastCellPos)
		{
			m_cellPos = _cellPos;
			RemoveAimTiles();

			if (CheckAttackRange(m_dist.x, m_dist.y))
				SetAimTiles();
			m_prevMouseCellPos = m_mouseCellPos;
		}
	}

	private bool CheckAttackRange(int _distX, int _distY)
	{
		if (GetSkillType() == SkillTypeEnum.Melee)
		{
			if ((m_eDir == DirEnum.Left || m_eDir == DirEnum.UpLeft) && -m_skillData.attackCellRange <= _distX && _distX <= 0) return true;
			if ((m_eDir == DirEnum.Right || m_eDir == DirEnum.DownRight) && 0 <= _distX && _distX <= m_skillData.attackCellRange) return true;
			if ((m_eDir == DirEnum.Up || m_eDir == DirEnum.UpRight) && -m_skillData.attackCellRange <= _distY && _distY <= 0) return true;
			if ((m_eDir == DirEnum.Down || m_eDir == DirEnum.DownLeft) && 0 <= _distY && _distY <= m_skillData.attackCellRange) return true;
		}
		else if (GetSkillType() == SkillTypeEnum.Ranged)
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
			case SkillTypeEnum.Melee:
				{
					while (m_aims.Count > 0)
					{
						Vector2Int aim = m_aims.Peek();
						MapManager.Inst.RemoveAimTile(aim.x, aim.y);
						m_aims.Dequeue();
					}
				}
				break;
			case SkillTypeEnum.Ranged:
				{
					while (m_aims.Count > 0)
					{
						Vector2Int aim = m_aims.Peek();
						MapManager.Inst.RemoveAimTile(aim.x, aim.y);
						m_aims.Dequeue();
					}
				}
				break;
			case SkillTypeEnum.Auto:
				break;
		}
	}

	private void SetAimTiles()
	{
		switch (m_skillData.type)
		{
			case SkillTypeEnum.Melee:
				{
					// FindDir();

					if (m_eDir == DirEnum.None) break;

					int startX, endX, startY, endY;
					int pnt;

					if (m_eDir == DirEnum.Left || m_eDir == DirEnum.UpLeft)
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
					else if(m_eDir == DirEnum.Right || m_eDir == DirEnum.DownRight)
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
					else if (m_eDir == DirEnum.Up || m_eDir == DirEnum.UpRight)
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
					else if (m_eDir == DirEnum.Down || m_eDir == DirEnum.DownLeft)
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
			case SkillTypeEnum.Ranged:
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
			case SkillTypeEnum.Auto:
				break;
		}
	}
}
