using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackState : ICreatureState
{
	public bool CanEnter(CreatureController _cs)
	{
		// 이동 중에는 불가능

		return true;
	}

	// 몬스터는 어택 도중 쳐맞을 수 있다. -> 바로 Hit애니 재생 후 체력 감소
	public void Enter(CreatureController _cs)
	{
	}

	public void Update()
	{
	
	}

	public void FixedUpdate()
	{
	
	}

	public void Exit()
	{
	
	}
}
