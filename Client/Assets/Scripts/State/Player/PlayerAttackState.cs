using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Define;

public class PlayerAttackState : ICreatureState
{
	PlayerController m_player;
	Skill m_skill;
	List<MonsterController> m_targets;
	bool m_animStart = false;
	AnimatorStateInfo m_stateInfo, m_skillStateInfo;
	bool m_hit = false;

	public bool CanEnter(CreatureController _cs)
	{
		// 현재 몬스터가 Dead 인 경우 AttackState로 바꿀 수 없음
		//if (_target.CurState is MonsterHitState) return false;
		//if (m_target?.CurState is MonsterDeadState) return false;

		return true;
	}

	// 타겟이 없으면 그냥 공격 모션만 하고 끝
	// 스킬 정보(애니이름, 언제 히트가 되는지 등)를 담고 있는 객체를 매개변수로
	public PlayerAttackState(List<MonsterController> _targets, Skill _skill) // 마우스 어디 클릭했는지 정보 받아서 원거리 스킬 이동
	{
		m_skill = _skill;
		m_targets = new List<MonsterController>(_targets);
	}

	public void Enter(CreatureController _cs)
	{
		m_player = _cs as PlayerController;
		m_player.Anim.SetTrigger("Attack");
		// 플레이어의 Dir로 Flip
		// 스킬 이펙트 있으면 재생

		m_skill.Play(m_player);

		//m_player.SkillAnim.transform.position = new Vector3(m_player.CellPos.x, -m_player.CellPos.y);
		
		foreach(MonsterController mc in m_targets)
			mc.Hit(m_skill);

		// 몬스터 멈추기 Dir = None
	}

	public void Update()
	{
		UpdateAnimation();
	}

	public void FixedUpdate()
	{
	}

	public void Exit()
	{
	}

	// 인게임 패킷 핸들러에서 PlayerAttackState로 바꾸지만 애니메이터에서 아직 스테이트 전환이 안되기 때문에 바로 PlayerIdleState로 전환됨.
	void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);
		if(m_skill.GetSkillType() == eSkillType.Ranged)
		{
			m_skillStateInfo = m_player.RangedSkillAnim.GetCurrentAnimatorStateInfo(0);
		}
		else
			m_skillStateInfo = m_player.SkillAnim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Attack"))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName("Attack") && m_skillStateInfo.IsName("None"))
		{
			m_player.ChangeState(new PlayerIdleState());
			Debug.Log("PlayerAttackState Changed");
			return;
		}

		if (!m_hit && m_targets.Count > 0 && m_stateInfo.normalizedTime >= 0.3f)
		{
			foreach (MonsterController mc in m_targets)
			{
				if(mc.ChangeState(new MonsterHitState()) == false)
				{
					if (mc.IsDead) mc.ChangeState(new MonsterDeadState());
				}
			}
			
			m_hit = true;
		}
	}
}
