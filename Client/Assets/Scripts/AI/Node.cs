using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComparable<Node>
{
	public int F { get; private set; }
	public int G { get; private set; }
	public Vector2Int CurPos { get; private set; }

	public Node(int _f, int _g, Vector2Int _curCellPos)
	{
		F = _f;
		G = _g;
		CurPos = _curCellPos;
	}

	public int CompareTo(Node other)
	{
		if (other == null) return 1;

		if (F > other.F) return 1;
		else if (F < other.F) return -1;
		else return 0;
	}
}
