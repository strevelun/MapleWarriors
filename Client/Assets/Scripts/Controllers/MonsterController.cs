using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CreatureController
{
	public struct Pos
	{
		public int x;
		public int y;
	}

	Pos[] m_dir = new Pos[8];
	int[] m_cost = {
		10, 
		10,
		10,
		10,
		14,
		14,
		14,
		14 };

	void Start()
    {
        
    }

	protected override void Update()
    {
		base.Update();

    }

	public override void Init()
	{
		base.Init();

		m_dir[0] = new Pos { x = 0, y = -1 };
		m_dir[1] = new Pos { x = 1, y = -1 };
		m_dir[2] = new Pos { x = 1, y = 0 };
		m_dir[3] = new Pos { x = 1, y = 1 };
		m_dir[4] = new Pos { x = 0, y = 1 };
		m_dir[5] = new Pos { x = -1, y = 1 };
		m_dir[6] = new Pos { x = -1, y = 0 };
		m_dir[7] = new Pos { x = -1, y = -1 };

		// 정점에 도달할때마다 인접 정점 검색
		// 뒤늦게 더 좋은 경로 발견될 경우


	}
}
