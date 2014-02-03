namespace VisualHint.SmartPropertyGrid;

internal class LinkedListNode
{
	private LinkedListNode previous;

	private LinkedListNode next;

	private object value;

	private LinkedList _children;

	private LinkedList _owner;

	private LinkedListNode _categorizedNode;

	public LinkedListNode CategorizedNode
	{
		get
		{
			return _categorizedNode;
		}
		set
		{
			_categorizedNode = value;
		}
	}

	public object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			if (value == null)
			{
				value = null;
			}
		}
	}

	public LinkedList Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	public LinkedListNode Previous
	{
		get
		{
			return previous;
		}
		set
		{
			previous = value;
		}
	}

	public LinkedListNode Next
	{
		get
		{
			return next;
		}
		set
		{
			next = value;
		}
	}

	public LinkedList Children
	{
		get
		{
			return _children;
		}
		set
		{
			_children = value;
		}
	}

	public LinkedListNode(LinkedList owner, object value)
	{
		Owner = owner;
		Value = value;
		if (owner != null && value != null)
		{
			_children = new LinkedList(this, owner.Depth + 1);
		}
	}
}
