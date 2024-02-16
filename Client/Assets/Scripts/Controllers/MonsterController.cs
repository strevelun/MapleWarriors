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
	eState m_eState = eState.None;
	public int PathIdx { get; set; } = 0;
	public Vector2Int DestPos { get; set; } = new Vector2Int(0, 0);
	bool m_cellArrived = true;

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

		//AdjustPositionToCell();


		UpdateChase();
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_maxSpeed = 2f;
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

	// 타겟의 위치가 변하면 경로를 다시 생성하고 첫번째 위치를 보낸다.
	// 그러나 몬스터가 현재 이동중이면 m_cellArrived가 true가 될때까지 BeginSearch를 계속 호출. true가 되면 보냄. 
	// 그런데 플레이어가 계속해서 이동하고 있다면?
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
			m_eState = eState.None;
			return;
		}

		if (m_path.Count <= 2)
		{
			Dir = eDir.None;
			m_eState = eState.None;
			return;
		}
		//else
		//m_eState = eState.Chase;

		PathIdx = 1;
		//Debug.Log($"경로 알고리즘 : {CellPos} -> {pc.CellPos}");

		Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
		NetworkManager.Inst.Send(pkt);

		m_targetMoved = false;
		Debug.Log($"BeginSearch");
	}

	void AdjustPositionToCell()
	{
		float xpos = transform.position.x, ypos = transform.position.y;
		if (Dir == eDir.Left || Dir == eDir.Right)
		{
			if (Math.Abs(ypos - -CellPos.y) < 0.1f)
				ypos = -CellPos.y;
		}
		else if (Dir == eDir.Up || Dir == eDir.Down)
		{
			if (Math.Abs(xpos - CellPos.x) < 0.1f)
				xpos = CellPos.x;
		}
		transform.position = new Vector3(xpos, ypos);
	}

	// 중심점이 아니면 return ;

	// 중심점이냐??
	//		중심점이면 멈춰라
	//		아니면 통과
	/*
	void UpdateChase()
	{
		if (Dir == eDir.None) return;

		Vector2 dest = new Vector2((float)DestPos.x, (float)-DestPos.y);
		if (Vector2.Distance(transform.position, dest) > 0.05f) return;


		if (!UserData.Inst.IsRoomOwner) return;

		if (m_path == null) return;



		if (PathIdx >= m_path.Count)
		{
			Dir = eDir.None;
			return;
		}

		++PathIdx;

		if (PathIdx < m_path.Count)
		{
			//if (CanGo(CellPos, m_path[PathIdx]) == false) --PathIdx ;

			Debug.Log($"도착후 보냄 : {Dir}, 다음좌표({PathIdx}) : {m_path[PathIdx].x}, {m_path[PathIdx].y}");
			Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
			NetworkManager.Inst.Send(pkt);
		}

	}
	*/

	void UpdateChase()
	{
		if (m_cellArrived) return;
		//Vector2 dest = new Vector2(DestPos.x, -DestPos.y);
		Vector2 dest = new Vector2(m_dest.Peek().x, -m_dest.Peek().y);
		float dist = Vector2.Distance(transform.position, dest);
		/*
		if (dist > 1.1f)
		{
			Dir = eDir.None;
			m_cellArrived = true;
			return;
		}
		*/
		if (Dir == eDir.None) return;
		if (m_eState == eState.None) return;
		/*
		int x = 0, y = 0;
		if (Dir == eDir.Left) x = -1;
		else if (Dir == eDir.Right) x = 1;
		else if (Dir == eDir.Up) y = -1;
		else if (Dir == eDir.Down) y = 1;
		
		Vector2 dest = new Vector2(DestPos.x + x, -(DestPos.y + y));
		*/

		if (dist <= m_maxSpeed * Time.fixedDeltaTime * 2)
		//if(Vector2.Distance(transform.position, dest) <= m_maxSpeed * Time.fixedDeltaTime * 2)
		{
			transform.position = dest;
			//transform.position = new Vector2(DestPos.x, -DestPos.y);

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
				m_eState = eState.None;
				m_cellArrived = true;
			}

			Debug.Log($"도착 : {CellPos}, 새로운 목적지 : {DestPos}");

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
				
				if (PathIdx < m_path.Count)
				{
					Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
					NetworkManager.Inst.Send(pkt);
					Debug.Log($"UpdateChase Packet");
				}
			}
		}
		Debug.Log($"UpdateChase");
	}

	// 특정 목표에 도착하기 전에 플레이어가 움직여서 BeginMove호출되는데 UpdateChase에서 그 다음 목표를 

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

		//if (Dir != eDir.None) return;
		//if (!m_cellArrived)
		{
			m_dest.Enqueue(dest);
		//	return;
		}

		

		//PathIdx = _pathIdx;

		m_eState = eState.Chase;
		//if (CanGo(CellPos, dest) == false) return;

		//CellPos = DestPos;
		//DestPos = dest;
		//transform.position = new Vector3(CellPos.x, -CellPos.y);

		// 도착하기도 전에 진로를 바꾸게 하면 안됨
		//DestPos = CellPos;

		//else Dir = eDir.None;

		m_cellArrived = false;
		//Debug.Log($"{Dir}로 가자");
		Debug.Log($"BeginMove {CellPos} -> {DestPos} : {Dir}");
	}

	public void EndMove()
	{
		PathIdx = 0;
		DestPos = new Vector2Int(0, 0);

		Dir = eDir.None;
		m_eState = eState.None;
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
