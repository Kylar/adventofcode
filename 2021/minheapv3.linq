<Query Kind="Statements" />

// https://gist.github.com/ashish01/8593936
/// Minimal generic min heap and priority queue implementation in C#, in less than 100 lines of code

using System;
using System.Collections.Generic;

class MinHeap<T> where T : IComparable<T>
{
	public T[] items = new T[4];
	public int size = 0;

	public void Add(T element)
	{
		if (size == items.Length)
		{
			var newitems = new T[items.Length * 2];
			Array.Copy(items, newitems, items.Length);
			items = newitems;
		}

		items[size] = element;
		size++;

		int c = size - 1;
		int parent = (c - 1) >> 1;
		while (c > 0 && items[c].CompareTo(items[parent]) < 0)
		{
			T tmp = items[c];
			items[c] = items[parent];
			items[parent] = tmp;
			c = parent;
			parent = (c - 1) >> 1;
		}
	}

	public T RemoveMin()
	{
		T ret = items[0];
		items[0] = items[size - 1];
		size--;

		int c = 0;
		while (c < size)
		{
			int min = c;
			var n = 2 * c + 2;
			if (n < size)
			{
				if (items[n].CompareTo(items[min]) < 0)
				{
					min = n;
				}
				n--;
				if (n < size)
				{
					if (items[n].CompareTo(items[min]) < 0)
					{
						min = n;
					}
				}
			}

			if (min == c)
				break;
			else
			{
				T tmp = items[c];
				items[c] = items[min];
				items[min] = tmp;
				c = min;
			}
		}
		return ret;
	}

	public T Peek()
	{
		return items[0];
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