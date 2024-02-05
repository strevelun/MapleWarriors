using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComparable<Node>
{
	int m_f, m_g;
	int m_xpos, m_ypos;

	public int CompareTo(Node other)
	{
		if (other == null) return 1;

		if (m_f > other.m_f) return 1;
		else if (m_f < other.m_f) return -1;
		else return 0;
	}
}
