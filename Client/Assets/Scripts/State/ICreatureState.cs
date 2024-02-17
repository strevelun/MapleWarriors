using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICreatureState 
{
	void Enter(CreatureController _cs);
	void Update();
	void Exit();
}
