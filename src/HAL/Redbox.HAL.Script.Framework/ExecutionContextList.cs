using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.HAL.Script.Framework;

internal class ExecutionContextList : IList<ExecutionContext>, ICollection<ExecutionContext>, IEnumerable<ExecutionContext>, IEnumerable
{
	private readonly List<ExecutionContext> m_list = new List<ExecutionContext>();

	public int Count => m_list.Count;

	public bool IsReadOnly => false;

	public ExecutionContext this[int index]
	{
		get
		{
			return m_list[index];
		}
		set
		{
			m_list[index] = value;
		}
	}

	public ExecutionContext GetByID(string id)
	{
		return m_list.Find((ExecutionContext each) => string.Compare(each.ID, id, ignoreCase: true) == 0);
	}

	public IEnumerable<ExecutionContext> EnumerateGarbage(bool force)
	{
		foreach (ExecutionContext item in m_list.FindAll((ExecutionContext c) => (force && c.IsComplete) || c.Status == ExecutionContextStatus.Garbage))
		{
			yield return item;
		}
	}

	public ExecutionContext GetEligibleExecutionContext()
	{
		List<ExecutionContext> list = m_list.FindAll((ExecutionContext context) => context.EligibleForScheduling);
		list.Sort(delegate(ExecutionContext x, ExecutionContext y)
		{
			int num = x.Priority.CompareTo(y.Priority);
			return (num != 0) ? num : x.CreatedOn.CompareTo(y.CreatedOn);
		});
		foreach (ExecutionContext item in list)
		{
			if (item.StartTime.HasValue)
			{
				if (DateTime.Now.CompareTo(item.StartTime) >= 0)
				{
					return item;
				}
				continue;
			}
			return item;
		}
		return null;
	}

	public IEnumerator<ExecutionContext> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(ExecutionContext item)
	{
		m_list.Add(item);
	}

	public void AddRange(ExecutionContextList list)
	{
		m_list.AddRange(list);
	}

	public void Clear()
	{
		m_list.Clear();
	}

	public bool Contains(ExecutionContext item)
	{
		return m_list.Contains(item);
	}

	public void CopyTo(ExecutionContext[] array, int arrayIndex)
	{
		m_list.CopyTo(array, arrayIndex);
	}

	public bool Remove(ExecutionContext item)
	{
		return m_list.Remove(item);
	}

	public int IndexOf(ExecutionContext item)
	{
		return m_list.IndexOf(item);
	}

	public void Insert(int index, ExecutionContext item)
	{
		m_list.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		m_list.RemoveAt(index);
	}
}
