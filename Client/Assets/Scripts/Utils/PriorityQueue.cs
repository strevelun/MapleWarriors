using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> where T : IComparable<T>
{
	public class PriorityQueue<T> where T : IComparable<T>
	{
		List<T> m_data;
		bool m_minHeap;

		public PriorityQueue(bool _minHeap = false)
		{
			m_minHeap = _minHeap;
			m_data = new List<T>();
		}

		public void Enqueue(T _item)
		{
			m_data.Add(_item);
			int curIdx = m_data.Count - 1;

			while (curIdx > 0)
			{
				int parentIdx = curIdx % 2 == 0 ? curIdx / 2 - 1 : curIdx / 2;

				if (!m_minHeap && m_data[curIdx].CompareTo(m_data[parentIdx]) == 1)
				{
					ChangeItems(curIdx, parentIdx);
				}
				else if (m_minHeap && m_data[curIdx].CompareTo(m_data[parentIdx]) == -1)
				{
					ChangeItems(curIdx, parentIdx);
				}

				curIdx = parentIdx;
			}
		}

		public T Dequeue()
		{
			int lastIdx = m_data.Count - 1;
			T result = m_data[0];
			m_data[0] = m_data[lastIdx];
			m_data.RemoveAt(lastIdx);

			--lastIdx;
			int parentIdx = 0;
			int changeIdx;

			while (true)
			{
				int leftChildIdx = parentIdx * 2 + 1;
				if (leftChildIdx > lastIdx) break;

				changeIdx = leftChildIdx;

				int rightChildIdx = leftChildIdx + 1;
				if (rightChildIdx <= lastIdx)
				{
					if (m_minHeap && m_data[leftChildIdx].CompareTo(m_data[rightChildIdx]) == 1)
						changeIdx = rightChildIdx;
					else if (!m_minHeap && m_data[leftChildIdx].CompareTo(m_data[rightChildIdx]) == -1)
						changeIdx = rightChildIdx;
				}

				if (m_data[parentIdx].CompareTo(m_data[changeIdx]) == 0) break;
				if (m_minHeap && m_data[parentIdx].CompareTo(m_data[changeIdx]) < 0) break;
				else if (!m_minHeap && m_data[parentIdx].CompareTo(m_data[changeIdx]) > 0) break;

				ChangeItems(parentIdx, changeIdx);
				parentIdx = changeIdx;
			}

			return result;
		}

		public T Peek()
		{
			return m_data[0];
		}

		public int Count()
		{
			return m_data.Count;
		}

		private void ChangeItems(int _a, int _b)
		{
			T temp = m_data[_a];
			m_data[_a] = m_data[_b];
			m_data[_b] = temp;
		}
	}
}
