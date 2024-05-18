using UnityEngine;
using static Define;

public class PlayerAttackState : ICreatureState
{
	private PlayerController m_player;
	private Skill m_skill;
	private bool m_animStart = false;
	private AnimatorStateInfo m_stateInfo, m_skillStateInfo;

	public bool CanEnter(CreatureController _cs)
	{ 
		if (_cs.CurState is PlayerHitState) return false;
		if (_cs.CurState is PlayerDeadState) return false;

		return true;
	}

	// 타겟이 없으면 그냥 공격 모션만 하고 끝
	// 스킬 정보(애니이름, 언제 히트가 되는지 등)를 담고 있는 객체를 매개변수로
	public PlayerAttackState(Skill _skill) // 마우스 어디 클릭했는지 정보 받아서 원거리 스킬 이동
	{
		m_skill = _skill;
	}

	public void Enter(CreatureController _cs)
	{
		m_player = _cs as PlayerController;
		m_player.Anim.SetTrigger("Attack");

		// 스킬 이펙트 있으면 재생
		m_skill.Play(m_player);
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

	void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);
		if(m_skill.GetSkillType() == SkillTypeEnum.Ranged)
		{
			m_skillStateInfo = m_player.RangedSkillAnim.GetCurrentAnimatorStateInfo(0);
		}
		else
			m_skillStateInfo = m_player.SkillAnim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Attack"))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName("Attack") && m_skillStateInfo.IsName("None"))
		{
			if (m_player.IsDead) m_player.ChangeState(new PlayerDeadState());
			else m_player.ChangeState(new PlayerIdleState());
			Debug.Log("PlayerAttackState Changed");
			return;
		}
	}
}
