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

	public Skill(eSkill _skill)
	{
		m_skillData = DataManager.Inst.FindSkillData(_skill.ToString());
	}

	public void SetSkill(eSkill _skill)
	{
		m_skillData = DataManager.Inst.FindSkillData(_skill.ToString());
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

	void FindHitTargets(List<MonsterController> _targets)
	{
		int cnt = 0;
		Queue<MonsterController> mcs = null;
		int r = m_skillData.attackRadius - 1;
		HashSet<MonsterController> hsMonsters = new HashSet<MonsterController>();

		for (int i = m_mouseCellPos.y - r; i <= m_mouseCellPos.y + r; ++i)
		{
			for (int j = m_mouseCellPos.x - r; j <= m_mouseCellPos.x + r; ++j)
			{
				mcs = MapManager.Inst.GetMonsters(j, i);
				if (mcs == null) continue;

				while (mcs.Count > 0)
				{
					MonsterController mc = mcs.Peek();

					if (!hsMonsters.Contains(mc))
					{
						_targets.Add(mc);
						hsMonsters.Add(mc);
						++cnt;

						if (cnt >= m_skillData.hitCount) return;
					}
					mcs.Dequeue();
				}
			}
		}
	}

	public void Update(Vector2Int _cellPos)
    {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		mousePos.y = -mousePos.y + 1.0f;
		m_mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);

		m_dist = m_mouseCellPos - _cellPos;

		if (m_prevMouseCellPos != m_mouseCellPos)
		{
			RemoveAimTiles();

			if (CheckAttackRange(m_dist.x, m_dist.y))
			{
				int r = m_skillData.attackRadius - 1;
				for (int i = m_mouseCellPos.y - r; i <= m_mouseCellPos.y + r; ++i)
				{
					for (int j = m_mouseCellPos.x - r; j <= m_mouseCellPos.x + r; ++j)
					{
						MapManager.Inst.SetAimTile(j, i);
						//Debug.Log($"{j}, {i}");
					}
				}
			}

			
			m_prevMouseCellPos = m_mouseCellPos;
		}
	}

	bool CheckAttackRange(int _distX, int _distY)
	{
		if ((-m_skillData.attackCellRange <= _distX && _distX <= m_skillData.attackCellRange)
			&& (-m_skillData.attackCellRange <= _distY && _distY <= m_skillData.attackCellRange)) return true;
		return false;
	}

	public void RemoveAimTiles()
	{
		int r = m_skillData.attackRadius - 1;

		for (int i = m_prevMouseCellPos.y - r; i <= m_prevMouseCellPos.y + r; ++i)
		{
			for (int j = m_prevMouseCellPos.x - r; j <= m_prevMouseCellPos.x + r; ++j)
			{
				MapManager.Inst.RemoveAimTile(j, i);
			}
		}
	}
}
