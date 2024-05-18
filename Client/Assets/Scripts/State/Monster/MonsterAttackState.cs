using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackState : ICreatureState
{
	private MonsterController m_mc;
	private bool m_animStart = false;
	private bool m_hit = false;
	private AnimatorStateInfo m_stateInfo;
	private List<PlayerController> m_targets;

	public MonsterAttackState(List<PlayerController> _targets)
	{
		m_targets = new List<PlayerController>(_targets);
	}

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	// 몬스터는 어택 도중 맞을 수 있다. -> 바로 Hit애니 재생 후 체력 감소
	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetTrigger("Attack");

		foreach (PlayerController pc in m_targets)
		{
			pc.Hit(m_mc.AttackDamage);
		}
	}

	public void Update()
	{

	}

	public void FixedUpdate()
	{
		UpdateAnimation();
		Attack();
	}

	public void Exit()
	{
		m_mc.AttackReady = false;
		if (m_mc.AttackEffect) m_mc.AttackObj.SetActive(false);
	}

	// 몬스터가 AttackSTate일때 맞으면 Attack 끝까지 재생
	private void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Attack"))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName("Attack"))
		{
			m_mc.ChangeState(new MonsterIdleState());
			return;
		}
	}

	private void Attack()
	{
		if (m_mc.AttackEffect)
		{
			if(m_stateInfo.normalizedTime >= 0.5f)
				m_mc.AttackObj.SetActive(true);
		}

		if (UserData.Inst.IsRoomOwner)
		{
			List<bool> targetHit = new List<bool>();
			bool hit;

			if (m_mc.FlyingAttack)
			{
				if (m_mc.TargetHit)
				{
					foreach (PlayerController pc in m_targets)
					{
						hit = pc.ChangeState(new PlayerHitState());
						if(pc.IsDead)
						{
							m_mc.RemoveTarget(pc);
						}
						targetHit.Add(hit);
					}
					Packet pkt = InGamePacketMaker.PlayerHit(m_mc, m_targets, targetHit);
					UDPCommunicator.Inst.SendAll(pkt);
					m_mc.TargetHit = false;
				}
			}
			else
			{
				if (!m_hit && m_targets.Count > 0 && m_stateInfo.normalizedTime >= 0.3f)
				{
					m_hit = true;
					foreach (PlayerController pc in m_targets)
					{
						hit = pc.ChangeState(new PlayerHitState());
						if (pc.IsDead)
						{
							m_mc.RemoveTarget(pc);
						}
						targetHit.Add(hit);
						pc.HitObj.SetActive(true);
					}
					Packet pkt = InGamePacketMaker.PlayerHit(m_mc, m_targets, targetHit);
					UDPCommunicator.Inst.SendAll(pkt);
				}
			}
		}
	}
}
