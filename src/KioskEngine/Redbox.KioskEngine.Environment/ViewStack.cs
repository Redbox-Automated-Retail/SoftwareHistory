using System;
using System.Collections;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class ViewStack : IEnumerable<IViewFrame>, IEnumerable
{
	private sealed class ViewStackEnumerator : IEnumerator<IViewFrame>, IDisposable, IEnumerator
	{
		private readonly IEnumerator<string> m_baseEnumerator;

		public IViewFrame Current => ViewService.Instance.GetViewFrame(m_baseEnumerator.Current);

		object IEnumerator.Current => Current;

		public ViewStackEnumerator(IEnumerator<string> baseEnumerator)
		{
			m_baseEnumerator = baseEnumerator;
		}

		public void Dispose()
		{
			m_baseEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			return m_baseEnumerator.MoveNext();
		}

		public void Reset()
		{
			m_baseEnumerator.Reset();
		}
	}

	private readonly Stack<string> m_stack = new Stack<string>();

	public int Count => m_stack.Count;

	public IEnumerator<IViewFrame> GetEnumerator()
	{
		return new ViewStackEnumerator(m_stack.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		m_stack.Clear();
	}

	public IViewFrame Pop()
	{
		return (m_stack.Count == 0) ? null : ViewService.Instance.GetViewFrame(m_stack.Pop());
	}

	public IViewFrame Peek()
	{
		return (m_stack.Count == 0) ? null : ViewService.Instance.GetViewFrame(m_stack.Peek());
	}

	public void Push(string viewName)
	{
		m_stack.Push(viewName);
	}

	public string PeekViewName()
	{
		return (m_stack.Count == 0) ? null : m_stack.Peek();
	}
}
