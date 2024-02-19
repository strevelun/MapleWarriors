using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerAttackState : ICreatureState
{
	PlayerController m_player;
	MonsterController m_target;
	string m_playerAnimName;
	bool m_animStart = false;
	AnimatorStateInfo m_stateInfo;
	bool m_hit = false;

	public bool CanEnter(CreatureController _cs)
	{
		// 현재 몬스터가 Dead 인 경우 AttackState로 바꿀 수 없음
		//if (_target.CurState is MonsterHitState) return false;
		if (m_target?.CurState is MonsterDeadState) return false;

		return true;
	}

	// 타겟이 없으면 그냥 공격 모션만 하고 끝
	// 스킬 정보(애니이름, 언제 히트가 되는지 등)를 담고 있는 객체를 매개변수로
	public PlayerAttackState(MonsterController _target, string _playerAnimName) 
	{
		m_playerAnimName = _playerAnimName;
		m_target = _target;
	}

	public void Enter(CreatureController _cs)
	{
		m_player = _cs as PlayerController;
		m_player.Anim.SetTrigger(m_playerAnimName);
		// 플레이어의 Dir로 Flip
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

		if (!m_animStart && m_stateInfo.IsName(m_playerAnimName))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName(m_playerAnimName))
		{
			m_player.ChangeState(new PlayerIdleState());
			//Debug.Log("PlayerAttackState Changed");
			return;
		}

		if (!m_hit && m_target) //&& m_stateInfo.normalizedTime >= 0.3f)
		{
			m_target.ChangeState(new MonsterHitState(m_player.AttackDamage));
			m_hit = true;
		}
	}
}
