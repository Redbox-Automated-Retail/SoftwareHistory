using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class UndoBlockCollection : IList, ICollection, IEnumerable, ICloneable
{
	public class Enumerator : IEnumerator
	{
		private readonly UndoBlockCollection m_collection;

		private readonly int m_version;

		private int m_index;

		public UndoBlock Current => m_collection[m_index];

		object IEnumerator.Current => Current;

		public Enumerator(UndoBlockCollection tc)
		{
			m_collection = tc;
			m_index = -1;
			m_version = tc.m_version;
		}

		public bool MoveNext()
		{
			if (m_version != m_collection.m_version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			m_index++;
			if (m_index >= m_collection.Count)
			{
				return false;
			}
			return true;
		}

		public void Reset()
		{
			if (m_version != m_collection.m_version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			m_index = -1;
		}
	}

	private const int DefaultMinimumCapacity = 16;

	private UndoBlock[] m_array = new UndoBlock[16];

	private int m_count;

	private int m_version;

	public string Name = "UndoAction";

	public UndoBlock this[int index]
	{
		get
		{
			ValidateIndex(index);
			return m_array[index];
		}
		set
		{
			ValidateIndex(index);
			m_version++;
			m_array[index] = value;
		}
	}

	public int Capacity
	{
		get
		{
			return m_array.Length;
		}
		set
		{
			if (value < m_count)
			{
				value = m_count;
			}
			if (value < 16)
			{
				value = 16;
			}
			if (m_array.Length != value)
			{
				m_version++;
				UndoBlock[] array = new UndoBlock[value];
				Array.Copy(m_array, 0, array, 0, m_count);
				m_array = array;
			}
		}
	}

	public int Count => m_count;

	bool ICollection.IsSynchronized => m_array.IsSynchronized;

	object ICollection.SyncRoot => m_array.SyncRoot;

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => false;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (UndoBlock)value;
		}
	}

	public UndoBlockCollection()
	{
	}

	public UndoBlockCollection(UndoBlockCollection collection)
	{
		AddRange(collection);
	}

	public UndoBlockCollection(UndoBlock[] array)
	{
		AddRange(array);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	void ICollection.CopyTo(Array array, int start)
	{
		CopyTo((UndoBlock[])array, start);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		m_version++;
		m_array = new UndoBlock[16];
		m_count = 0;
	}

	public void RemoveAt(int index)
	{
		ValidateIndex(index);
		m_version++;
		m_count--;
		Array.Copy(m_array, index + 1, m_array, index, m_count - index);
		if (NeedsTrimming())
		{
			Trim();
		}
	}

	int IList.Add(object item)
	{
		return Add((UndoBlock)item);
	}

	bool IList.Contains(object item)
	{
		return Contains((UndoBlock)item);
	}

	int IList.IndexOf(object item)
	{
		return IndexOf((UndoBlock)item);
	}

	void IList.Insert(int position, object item)
	{
		Insert(position, (UndoBlock)item);
	}

	void IList.Remove(object item)
	{
		Remove((UndoBlock)item);
	}

	public void CopyTo(UndoBlock[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(UndoBlock[] array, int start)
	{
		if (m_count > array.GetUpperBound(0) + 1 - start)
		{
			throw new ArgumentException("Destination array was not long enough.");
		}
		Array.Copy(m_array, 0, array, start, m_count);
	}

	public int Add(UndoBlock item)
	{
		if (NeedsGrowth())
		{
			Grow();
		}
		m_version++;
		m_array[m_count] = item;
		return m_count++;
	}

	public bool Contains(UndoBlock item)
	{
		if (IndexOf(item) != -1)
		{
			return true;
		}
		return false;
	}

	public int IndexOf(UndoBlock item)
	{
		for (int i = 0; i < m_count; i++)
		{
			if (m_array[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int position, UndoBlock item)
	{
		ValidateIndex(position, allowEqualEnd: true);
		if (NeedsGrowth())
		{
			Grow();
		}
		m_version++;
		Array.Copy(m_array, position, m_array, position + 1, m_count - position);
		m_array[position] = item;
		m_count++;
	}

	public void Remove(UndoBlock item)
	{
		int num = IndexOf(item);
		if (num < 0)
		{
			throw new ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
		}
		RemoveAt(num);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public UndoBlockCollection Clone()
	{
		UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
		undoBlockCollection.AddRange(this);
		undoBlockCollection.Capacity = m_array.Length;
		undoBlockCollection.m_version = m_version;
		return undoBlockCollection;
	}

	public void AddRange(UndoBlockCollection collection)
	{
		m_version++;
		Capacity += collection.Count;
		Array.Copy(collection.m_array, 0, m_array, m_count, collection.m_count);
		m_count += collection.Count;
	}

	public void AddRange(UndoBlock[] array)
	{
		m_version++;
		Capacity += array.Length;
		Array.Copy(array, 0, m_array, m_count, array.Length);
		m_count += array.Length;
	}

	private void ValidateIndex(int index)
	{
		ValidateIndex(index, allowEqualEnd: false);
	}

	private void ValidateIndex(int index, bool allowEqualEnd)
	{
		int num = (allowEqualEnd ? m_count : (m_count - 1));
		if (index < 0 || index > num)
		{
			throw new ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", index, "Specified argument was out of the range of valid values.");
		}
	}

	private bool NeedsGrowth()
	{
		return m_count >= Capacity;
	}

	private void Grow()
	{
		if (NeedsGrowth())
		{
			Capacity = m_count * 2;
		}
	}

	private bool NeedsTrimming()
	{
		return m_count <= Capacity / 2;
	}

	private void Trim()
	{
		if (NeedsTrimming())
		{
			Capacity = m_count;
		}
	}
}
