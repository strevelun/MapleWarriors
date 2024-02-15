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
	public int PathIdx { get; set; } = 0;
	int m_prevPathIdx = 0;
	public Vector2Int DestPos { get; set; } = new Vector2Int(0, 0);

	void Start()
    {

	}

	protected override void Update()
    {
		base.Update();

		if (UserData.Inst.IsRoomOwner)
		{


			BeginSearch();
			//StartChase();
			//UpdateChase();
		}
		else
		{
			//if(CellPos == DestPos)
			//{
			//	Dir = eDir.None;
			//}
		}
		UpdateChase();
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_maxSpeed = 2f;
	}

	// 플레이어가 포착되면 길을 만들고, PathIdx=1의 위치부터 하나씩 서버로 

	void BoundCheck()
	{
		int x, y;
		if (CellPos.x <= 0) x = 0;
		//else if(CellPos.x >= MapManager.Inst.MaxX)
		if (CellPos.y <= 0) y = 0;
	}

	void BeginSearch()
	{
		//if (m_eState == eState.Chase) return;
		if (m_targets.Count == 0) return;
		if (Dir != eDir.None) return;
			//if (m_prevTargetPos != null && m_prevTargetPos == m_targets.Peek().CellPos) return;

		PlayerController pc = m_targets.Peek();
		if (Math.Abs(CellPos.x - pc.CellPos.x) <= 1 && Math.Abs(CellPos.y - pc.CellPos.y) <= 1) return;

		m_prevTargetPos = pc.CellPos;
		m_path = m_astar.Search(CellPos, pc.CellPos);

		if (m_path.Count <= 2)
		{
			m_eState = eState.None;
			return;
		}
		//else
		PathIdx = 1;

		Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
		NetworkManager.Inst.Send(pkt); 	
	}

	void StartChase()
	{
		if (m_targets.Count == 0) return;
		if (m_eState != eState.Chase) return;
		if (m_path == null) return;
		//if (PathIdx >= m_path.Count) return;

		Vector2 dir = CellPos - m_path[PathIdx];
		if (dir.x == -1) Dir = eDir.Right;
		else if (dir.x == 1) Dir = eDir.Left;
		else if (dir.y == -1) Dir = eDir.Down;
		else if (dir.y == 1) Dir = eDir.Up;
	}

	void UpdateChase()
	{
		if (m_path == null) return;
		if (Dir == eDir.None) return;

		/*
		if (PathIdx >= m_path.Count)
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
		*/

		Vector3 dest = new Vector3(m_path[PathIdx].x, -m_path[PathIdx].y);
		if (Vector3.Distance(transform.position, dest) < 0.05f)
		//if (CellPos == m_path[PathIdx])
		{
			transform.position = dest;
			++PathIdx;
			Dir = eDir.None;
			//if (PathIdx >= m_path.Count)
			//{
			//	m_eState = eState.None;
			//	Packet pkt = InGamePacketMaker.EndMoveMonster(name);
			//	NetworkManager.Inst.Send(pkt);
			//}
			//else
			{
				if (PathIdx < m_path.Count)
				{
					Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
					NetworkManager.Inst.Send(pkt);
					//Debug.Log(m_path[PathIdx + 1]);
				}
			}
			//Debug.Log(PathIdx);
		}
		//Debug.Log(Vector3.Distance(transform.position, dest));
	}

	// 각 몬스터가 플레이어를 발견하면 각자 Search를 하고 패킷을 만들어 올려야 함.

	public void BeginMove(int _pathIdx, int _cellXPos, int _cellYPos)
	{
		PathIdx = _pathIdx;
		DestPos = new Vector2Int(_cellXPos, _cellYPos);
		m_eState = eState.Chase;

		Vector2 dir = CellPos - DestPos;
		if (dir.x == -1) Dir = eDir.Right;
		else if (dir.x == 1) Dir = eDir.Left;
		else if (dir.y == -1) Dir = eDir.Down;
		else if (dir.y == 1) Dir = eDir.Up;
	}

	public void EndMove()
	{
		PathIdx = 0;
		DestPos = new Vector2Int(0, 0);

		Dir = eDir.None;
		m_eState = eState.None;
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
		if (m_targets.Count == 0)
		{
			m_path = null;
			Packet pkt = InGamePacketMaker.EndMoveMonster(name);
			NetworkManager.Inst.Send(pkt);
		}

		//Debug.Log(other.gameObject.name + " has exited!");
	}
}
