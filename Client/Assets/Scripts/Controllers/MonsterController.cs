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
	Queue<Vector2Int> m_dest = new Queue<Vector2Int>();
	Vector2Int m_prevTargetPos = new Vector2Int(-1,-1);
	bool m_targetMoved = false;
	//eState m_eState = eState.None;
	public int PathIdx { get; set; } = 0;
	public Vector2Int DestPos { get; set; } = new Vector2Int(0, 0);
	public bool m_cellArrived { get; private set; } = true;



	void Start()
	{
		DestPos = CellPos;
	}

	protected override void Update()
	{
		base.Update();

		if (UserData.Inst.IsRoomOwner)
		{
			CheckTargetPosChanged();
			BeginSearch();
		}
	}


	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		UpdateChase();
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		ChangeState(new MonsterIdleState());
		MaxSpeed = 2f;
		HP = 10;
		AttackDamage = 2;
		AttackRange = 1;
		MapManager.Inst.AddMonster(this, CellPos.x, CellPos.y);
	}

	public void CheckMoveState()
	{
		if (Dir == eDir.None)
			ChangeState(new MonsterIdleState());
		else
			ChangeState(new MonsterRunState());
	}

	void CheckTargetPosChanged()
	{
		if (m_targets.Count == 0) return;
		
		PlayerController pc = m_targets.Peek();
		if (m_prevTargetPos != pc.CellPos)
		{
			m_prevTargetPos = pc.CellPos;
			m_targetMoved = true;
		}
	}

	void BeginSearch()
	{
		if (m_targets.Count == 0) return;
		if (!m_targetMoved) return;
		if (!m_cellArrived) return;

		PlayerController pc = m_targets.Peek();
		if ((Math.Abs(CellPos.x - pc.CellPos.x) <= 1 && CellPos.y == pc.CellPos.y) ||
			(Math.Abs(CellPos.y - pc.CellPos.y) <= 1 && CellPos.x == pc.CellPos.x)) return;

		m_path = m_astar.Search(CellPos, pc.CellPos);
		if (m_path == null)
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			return;
		}

		if (m_path.Count <= 2)
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			return;
		}

		PathIdx = 1;

		Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
		NetworkManager.Inst.Send(pkt);

		m_targetMoved = false;
	}
	
	void UpdateChase()
	{
		if (m_cellArrived) return;
		if (Dir == eDir.None) return;
		//if (m_eState == eState.None) return;

		Vector2 dest = new Vector2(m_dest.Peek().x, -m_dest.Peek().y);
		float dist = Vector2.Distance(transform.position, dest);

		if (dist > MaxSpeed * Time.fixedDeltaTime * 2) return;

		MapManager.Inst.RemoveMonster(LastCellPos.x, LastCellPos.y);
		MapManager.Inst.AddMonster(this, CellPos.x, CellPos.y);
		transform.position = dest;
		m_dest.Dequeue();

		if (m_dest.Count > 0)
		{
			Vector2 dir = CellPos - new Vector2(m_dest.Peek().x, m_dest.Peek().y);

			if (dir.x <= -1) Dir = eDir.Right;
			else if (dir.x >= 1) Dir = eDir.Left;
			else if (dir.y <= -1) Dir = eDir.Down;
			else if (dir.y >= 1) Dir = eDir.Up;
		}
		else
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			m_cellArrived = true;
		}

		if (UserData.Inst.IsRoomOwner)
		{
			if (m_targetMoved) return;

			if (m_path == null) return;
			if (PathIdx >= m_path.Count)
			{
				Dir = eDir.None;
				return;
			}

			++PathIdx;
				
			if (PathIdx < m_path.Count-1)
			{
				Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
				NetworkManager.Inst.Send(pkt);
			}
		}
	}
	

	public void BeginMove(int _pathIdx, int _cellXPos, int _cellYPos)
	{
		Vector2Int dest = new Vector2Int(_cellXPos, _cellYPos);
		if (!CanGo(CellPos, dest)) return;

		if (m_dest.Count == 0)
		{
			Vector2 dir = CellPos - dest;

			if (dir.x <= -1) Dir = eDir.Right;
			else if (dir.x >= 1) Dir = eDir.Left;
			else if (dir.y <= -1) Dir = eDir.Down;
			else if (dir.y >= 1) Dir = eDir.Up;
		}

		m_dest.Enqueue(dest);

		//m_eState = eState.Chase;

		m_cellArrived = false;
	}


	bool CanGo(Vector2Int _from, Vector2Int _to)
	{
		if (Math.Abs(_from.x - _to.x) + Math.Abs(_from.y - _to.y) != 1) return false;
		Vector2 dir = _from - _to;
		if (dir.normalized.x != -1 && dir.normalized.x != 1 && dir.normalized.y != -1 && dir.normalized.y != 1) return false;
		return true;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag != "Player") return;

		m_targets.Enqueue(other.gameObject.GetComponent<PlayerController>());
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag != "Player") return;

		m_targets.Dequeue();
		if (m_targets.Count == 0)
		{
			m_path = null;
			//Packet pkt = InGamePacketMaker.EndMoveMonster(name);
			//NetworkManager.Inst.Send(pkt);
		}
	}
}
