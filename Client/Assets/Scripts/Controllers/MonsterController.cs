using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MonsterController : CreatureController
{
	enum eState
	{
		None,
		Chase,
		Patrol,
		Attack
	}

	AStar m_astar = new AStar();

	List<Vector2Int> m_path = null;
	Queue<PlayerController> m_targets = new Queue<PlayerController>();
	Vector2Int m_prevTargetPos;
	eState m_eState = eState.None;
	int m_pathIdx = 0;

	void Start()
    {

	}

	protected override void Update()
    {
		base.Update();


		HandleTarget();
		StartChase();
		UpdateChase();
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_maxSpeed = 4f;
	}

	void HandleTarget()
	{
		if (m_targets.Count == 0) return;
		//if (m_eState != State.None && m_eState != State.Patrol) return;
		//if (m_pathIdx < m_path.Count) return;
		// if (m_dest == m_targets.Peek().CellPos) return;
		if (m_prevTargetPos != null && m_prevTargetPos == m_targets.Peek().CellPos) return;

		m_prevTargetPos = m_targets.Peek().CellPos;

		m_path = m_astar.Search(CellPos, m_targets.Peek().CellPos);

		if (m_path.Count < 3)
			m_eState = eState.None;
		else
		{
			m_pathIdx = 1;
			m_eState = eState.Chase;
		}
	}

	void StartChase()
	{
		if (m_targets.Count == 0) return;
		if (m_eState != eState.Chase) return;
		if (m_path == null) return;
		//if (m_pathIdx >= m_path.Count) return;

		Vector2 dir = CellPos - m_path[m_pathIdx];
		if (dir.x == -1) Dir = eDir.Right;
		else if (dir.x == 1) Dir = eDir.Left;

		else if (dir.y == -1) Dir = eDir.Down;
		else if (dir.y == 1) Dir = eDir.Up;
	}

	void UpdateChase()
	{
		if (m_path == null) return;

		if (m_pathIdx >= m_path.Count)
		{
			m_eState = eState.None;
			return;
		}

		if (m_eState == eState.None)
		{
			Dir = eDir.None;
			return;
		}
		if (m_eState != eState.Chase) return;

		if (CellPos == m_path[m_pathIdx])
		{
			++m_pathIdx;
			Dir = eDir.None;
			if (m_pathIdx >= m_path.Count)
			{
				m_eState = eState.None;
			}
			//Debug.Log(m_pathIdx);
		}
	}



	

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag != "Player") return;

		m_targets.Enqueue(other.gameObject.GetComponent<PlayerController>());

		//Debug.Log(other.gameObject.name + " has entered!");
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag != "Player") return;

		m_targets.Dequeue();
		if(m_targets.Count == 0) m_eState = eState.None;

		//Debug.Log(other.gameObject.name + " has exited!");
	}
}
