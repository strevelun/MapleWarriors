using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnyState 
{
	void Enter(CreatureController _cs);
	void Update();
	void Exit();
}
