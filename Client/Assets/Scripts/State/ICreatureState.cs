﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICreatureState 
{
	bool CanEnter(CreatureController _cs);
	void Enter(CreatureController _cs);
	void Update();
	void FixedUpdate();
	void Exit();
}