using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerAttackState : ICreatureState
{
	PlayerController m_player;
	MonsterController m_target;
	string m_playerAnimName;
	AnimatorStateInfo m_stateInfo;
	bool m_hit = false;

	public bool CanEnter(CreatureController _cs)
	{

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

	void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_stateInfo.IsName(m_playerAnimName))
		{
			m_player.ChangeState(new PlayerIdleState());
			Debug.Log("PlayerAttackState Changed");
			return;
		}

		if (!m_hit && m_target && m_stateInfo.normalizedTime >= 0.3f)
		{
			m_target.ChangeState(new MonsterHitState(m_player.AttackDamage));
			m_hit = true;
		}
	}
}
