using System;
using System.Collections;

namespace VisualHint.SmartPropertyGrid;

internal class LinkedList
{
	private int _count;

	private LinkedListNode _leftBound;

	private LinkedListNode _head;

	private LinkedListNode _tail;

	private LinkedListNode _rightBound;

	private LinkedListNode _parent;

	private int _depth;

	public int Depth => _depth;

	public int Count => _count;

	public object this[int index]
	{
		get
		{
			if (index > Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "ArgumentOutOfRange_Index");
			}
			LinkedListNode linkedListNode = First;
			for (int i = 0; i <= index; i++)
			{
				linkedListNode = linkedListNode.Next;
			}
			return linkedListNode.Value;
		}
		set
		{
			if (index > Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "ArgumentOutOfRange_Index");
			}
			LinkedListNode linkedListNode = First;
			for (int i = 0; i < index; i++)
			{
				linkedListNode = linkedListNode.Next;
			}
			linkedListNode.Value = value;
		}
	}

	public LinkedListNode First => _head;

	public LinkedListNode Last => _tail;

	public LinkedListNode Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	public LinkedListNode LeftBound => _leftBound;

	public LinkedListNode RightBound => _rightBound;

	public LinkedList(LinkedListNode parentNode, int depth)
	{
		_depth = depth;
		_parent = parentNode;
		_leftBound = new LinkedListNode(this, null);
		_rightBound = new LinkedListNode(this, null);
		LinkNode(_leftBound, _rightBound);
	}

	private static void LinkNode(LinkedListNode node1, LinkedListNode node2)
	{
		node1.Next = node2;
		node2.Previous = node1;
	}

	public LinkedListNode Append(object value)
	{
		LinkedListNode linkedListNode = new LinkedListNode(this, value);
		if (_head == null)
		{
			_head = linkedListNode;
			LinkNode(_leftBound, linkedListNode);
			LinkNode(linkedListNode, _rightBound);
		}
		else
		{
			LinkNode(_tail, linkedListNode);
			LinkNode(linkedListNode, _rightBound);
		}
		_tail = linkedListNode;
		_count++;
		return linkedListNode;
	}

	public LinkedListNode Prepend(object value)
	{
		LinkedListNode linkedListNode = new LinkedListNode(this, value);
		if (_tail == null)
		{
			_tail = linkedListNode;
			LinkNode(_leftBound, linkedListNode);
			LinkNode(linkedListNode, _rightBound);
		}
		else
		{
			LinkNode(_leftBound, linkedListNode);
			LinkNode(linkedListNode, _head);
		}
		_head = linkedListNode;
		_count++;
		return linkedListNode;
	}

	public LinkedListNode InsertBefore(LinkedListNode before, object value)
	{
		if (before == _head)
		{
			return Prepend(value);
		}
		if (before == _rightBound)
		{
			return Append(value);
		}
		LinkedListNode linkedListNode = new LinkedListNode(this, value);
		LinkedListNode previous = before.Previous;
		LinkNode(previous, linkedListNode);
		LinkNode(linkedListNode, before);
		_count++;
		return linkedListNode;
	}

	public LinkedListNode InsertAfter(LinkedListNode after, object value)
	{
		if (after == _tail)
		{
			return Append(value);
		}
		if (after == _leftBound)
		{
			return Prepend(value);
		}
		LinkedListNode linkedListNode = new LinkedListNode(this, value);
		LinkedListNode next = after.Next;
		LinkNode(after, linkedListNode);
		LinkNode(linkedListNode, next);
		_count++;
		return linkedListNode;
	}

	public void Clear()
	{
		LinkNode(_leftBound, _rightBound);
		_count = 0;
		_head = null;
		_tail = null;
	}

	public void Remove(LinkedListNode node)
	{
		LinkedListNode previous = node.Previous;
		LinkedListNode next = node.Next;
		if (node == _head && node == _tail)
		{
			_head = (_tail = null);
		}
		else if (node == _head)
		{
			_head = next;
		}
		else if (node == _tail)
		{
			_tail = previous;
		}
		LinkNode(previous, next);
		_count--;
	}

	public void Sort(IComparer comparer)
	{
		for (int num = Count - 1; num > 0; num--)
		{
			LinkedListNode linkedListNode = _head;
			for (int i = 0; i < Count - 1; i++)
			{
				if (comparer.Compare(linkedListNode.Value, linkedListNode.Next.Value) > 0)
				{
					if (linkedListNode == _head)
					{
						_head = linkedListNode.Next;
					}
					else if (linkedListNode.Next == _tail)
					{
						_tail = linkedListNode;
					}
					LinkNode(linkedListNode.Previous, linkedListNode.Next);
					LinkedListNode next = linkedListNode.Next.Next;
					LinkNode(linkedListNode.Next, linkedListNode);
					LinkNode(linkedListNode, next);
				}
				else
				{
					linkedListNode = linkedListNode.Next;
				}
			}
		}
	}
}
