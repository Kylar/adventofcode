<Query Kind="Statements" />

// https://gist.github.com/ashish01/8593936
/// Minimal generic min heap and priority queue implementation in C#, in less than 100 lines of code

using System;
using System.Collections.Generic;

class MinHeap<T> where T : IComparable<T>
{
	public List<T> list = new List<T>();
	public int size = 0;

	public void Add(T element)
	{
		if (size == list.Count)
		{
			list.Add(element);
		}
		else
		{
			list[size] = element;
		}
		size++;



		int c = size - 1;
		int parent = (c - 1) >> 1;
		while (c > 0 && list[c].CompareTo(list[parent]) < 0)
		{
			T tmp = list[c];
			list[c] = list[parent];
			list[parent] = tmp;
			c = parent;
			parent = (c - 1) >> 1;
		}
	}

	public T RemoveMin()
	{
		T ret = list[0];
		list[0] = list[size - 1];
		size--;

		int c = 0;
		while (c < size)
		{
			int min = c;
			if (2 * c + 1 < size && list[2 * c + 1].CompareTo(list[min]) < 0)
				min = 2 * c + 1;
			if (2 * c + 2 < size && list[2 * c + 2].CompareTo(list[min]) < 0)
				min = 2 * c + 2;

			if (min == c)
				break;
			else
			{
				T tmp = list[c];
				list[c] = list[min];
				list[min] = tmp;
				c = min;
			}
		}
		return ret;
	}

	public T Peek()
	{
		return list[0];
	}

	public int Count
	{
		get
		{
			return size;
		}
	}
}

class PriorityQueue<T>
{
	internal class Node : IComparable<Node>
	{
		public int Priority;
		public T O;
		public int CompareTo(Node other)
		{
			return Priority.CompareTo(other.Priority);
		}
	}

	private MinHeap<Node> minHeap = new MinHeap<Node>();

	public void Add(int priority, T element)
	{
		minHeap.Add(new Node() { Priority = priority, O = element });
	}

	public T RemoveMin()
	{
		return minHeap.RemoveMin().O;
	}

	public T Peek()
	{
		return minHeap.Peek().O;
	}

	public int Count
	{
		get
		{
			return minHeap.Count;
		}
	}
}