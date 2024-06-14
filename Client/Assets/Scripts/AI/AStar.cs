using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class AStar 
{
	private const int DirLen = 8;

	private readonly Vector2Int[] m_dir = new Vector2Int[DirLen];
	private readonly int[] m_cost = { 10, 14, 10, 14, 10, 14, 10, 14 };

	private int[,] m_best;
	private bool[,] m_visited;
	private readonly Dictionary<Vector2Int, Vector2Int> m_dicParent = new Dictionary<Vector2Int, Vector2Int>();

	private readonly PriorityQueue<Node> m_pq = new PriorityQueue<Node>(true); // 민힙

	public AStar()
	{
		m_dir[0] = new Vector2Int(0, -1);
		m_dir[1] = new Vector2Int(1, -1);
		m_dir[2] = new Vector2Int(1, 0);
		m_dir[3] = new Vector2Int(1, 1);
		m_dir[4] = new Vector2Int(0, 1);
		m_dir[5] = new Vector2Int(-1, 1);
		m_dir[6] = new Vector2Int(-1, 0);
		m_dir[7] = new Vector2Int(-1, -1);
	}

	// 시작점부터 목적지까지 path 만들기 (중간에 없던 장애물이 갑자기 생기면 다시 호출할 것)
	public List<Vector2Int> MakePath(Vector2Int _startCellPos, Vector2Int _destCellPos, int _hitboxWidth, int _hitboxHeight, int _attackCellRange)
	{
		int XSize = MapManager.Inst.XSize;
		int YSize = MapManager.Inst.YSize;

		if (XSize <= 0 || YSize <= 0) return null;
		if (_startCellPos.x < 0 || _startCellPos.x >= XSize || _startCellPos.y < 0 || _startCellPos.y >= YSize) return null;
		if (_destCellPos.x < 0 || _destCellPos.x >= XSize || _destCellPos.y < 0 || _destCellPos.y >= YSize) return null;

		MapManager.Inst.SetMonsterCollision(_startCellPos.x, _startCellPos.y, _hitboxWidth, _hitboxHeight, false); // 잠시 현재 몬스터 위치의 collision 정보 지우기. (IsBlocked)

		m_dicParent.Clear();
		m_pq.Clear();

		List<Vector2Int> path = new List<Vector2Int>();

		m_best = new int[YSize, XSize];
		m_visited = new bool[YSize, XSize];

		for (int i = 0; i < YSize; ++i)
			for (int j = 0; j < XSize; ++j)
				m_best[i, j] = int.MaxValue;

		int g = 0;
		int h = 10 * (Math.Abs(_destCellPos.y - _startCellPos.y) + Math.Abs(_destCellPos.x - _startCellPos.x));
		m_pq.Enqueue(new Node(g + h, g, _startCellPos));
		m_best[_startCellPos.y, _startCellPos.x] = g + h; 
		m_dicParent[_startCellPos] = _startCellPos;

		Node node;
		Node finalNode = null; 

		while (m_pq.Count() > 0)
		{
			node = m_pq.Dequeue();

			if (MapManager.Inst.IsBlocked(node.CurPos.x, node.CurPos.y, _hitboxWidth, _hitboxHeight)) continue;
			if (m_visited[node.CurPos.y, node.CurPos.x]) continue;

			m_visited[node.CurPos.y, node.CurPos.x] = true;
			finalNode = node;

			if (node.CurPos == _destCellPos) break;

			for (int i = 0; i < DirLen; ++i)
			{
				Vector2Int nextPos = node.CurPos + m_dir[i];
				if (m_visited[nextPos.y, nextPos.x]) continue;
				if (MapManager.Inst.IsBlocked(nextPos.x, nextPos.y, _hitboxWidth, _hitboxHeight)) continue;
				if (MapManager.Inst.IsMonsterCollision(nextPos.x, nextPos.y, _hitboxWidth, _hitboxHeight)) continue;
				if (CheckDiagonalBlocked(i, nextPos.x, nextPos.y, _hitboxWidth, _hitboxHeight)) continue;

				g = node.G + m_cost[i];
				h = 10 * (Math.Abs(_destCellPos.y - nextPos.y) + Math.Abs(_destCellPos.x - nextPos.x));
				if (m_best[nextPos.y, nextPos.x] <= g + h) continue; 

				m_best[nextPos.y, nextPos.x] = g + h;
				m_pq.Enqueue(new Node(g + h, g, nextPos));
				m_dicParent[nextPos] = node.CurPos;
			}
		}

		MapManager.Inst.SetMonsterCollision(_startCellPos.x, _startCellPos.y, _hitboxWidth, _hitboxHeight, true); // 원래대로 collision 세팅

		// 길찾기를 끝냈는데 마지막 노드의 위치와 원래 목표했던 지점 차이가 몬스터의 공격범위를 넘어서면 가만히 있음
		Vector2Int finalPosDist = finalNode.CurPos - _destCellPos;
		if (Math.Abs(finalPosDist.x) >= _attackCellRange && Math.Abs(finalPosDist.y) >= _attackCellRange) return null;

		int closeDistLimit = _hitboxWidth < _hitboxHeight ? _hitboxHeight - 1 : _hitboxWidth - 1; 
		int k = 0;

		if (finalNode != null)
		{
			Vector2Int pos = finalNode.CurPos;

			while (pos != _startCellPos) 
			{
				if (k++ >= closeDistLimit)
					path.Add(pos);
				pos = m_dicParent[pos];
			}
			path.Add(_startCellPos); 
			path.Reverse();
		}

		return path;
	}

	private bool CheckDiagonalBlocked(int _idx, int _nextPosX, int _nextPosY, int _hitboxWidth, int _hitboxHeight)
    {
		if (_idx == 1) // UpRight
		{
			if (MapManager.Inst.IsBlocked(_nextPosX - 1, _nextPosY, _hitboxWidth, _hitboxHeight)
				|| MapManager.Inst.IsBlocked(_nextPosX, _nextPosY + 1, _hitboxWidth, _hitboxHeight))
				return true;
		}
		else if (_idx == 3) // DownRight
		{
			if (MapManager.Inst.IsBlocked(_nextPosX - 1, _nextPosY, _hitboxWidth, _hitboxHeight)
				|| MapManager.Inst.IsBlocked(_nextPosX, _nextPosY - 1, _hitboxWidth, _hitboxHeight))
				return true;
		}
		else if (_idx == 5) // DownLeft
		{
			if (MapManager.Inst.IsBlocked(_nextPosX + 1, _nextPosY, _hitboxWidth, _hitboxHeight)
				|| MapManager.Inst.IsBlocked(_nextPosX, _nextPosY - 1, _hitboxWidth, _hitboxHeight))
				return true;
		}
		else if (_idx == 7) // UpLeft
		{
			if (MapManager.Inst.IsBlocked(_nextPosX + 1, _nextPosY, _hitboxWidth, _hitboxHeight)
				|| MapManager.Inst.IsBlocked(_nextPosX, _nextPosY + 1, _hitboxWidth, _hitboxHeight))
				return true;
		}
		return false;
	}
}
