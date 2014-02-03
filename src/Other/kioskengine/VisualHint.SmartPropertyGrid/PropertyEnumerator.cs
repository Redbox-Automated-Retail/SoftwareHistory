using System;

namespace VisualHint.SmartPropertyGrid;

public abstract class PropertyEnumerator : ICloneable
{
	protected PropertyEnumerator mLeftBound;

	protected PropertyEnumerator mRightBound;

	private LinkedListNode _node;

	internal LinkedList _restrictToThisLinkedListAndUnder;

	internal LinkedListNode _restrictToThisNodeAndUnder;

	public PropertyEnumerator CategorizedPropertyEnumerator
	{
		get
		{
			if (_node.CategorizedNode != null)
			{
				return new PropertyDeepEnumerator(_node.CategorizedNode);
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				_node.CategorizedNode = value.Node;
			}
		}
	}

	internal LinkedListNode Node
	{
		get
		{
			return _node;
		}
		set
		{
			_node = value;
		}
	}

	public Property Property => (Property)Node.Value;

	public int Count => Node.Owner.Count;

	public bool HasParent
	{
		get
		{
			if (_restrictToThisLinkedListAndUnder == Node.Owner || _restrictToThisNodeAndUnder == Node)
			{
				return false;
			}
			return Node.Owner.Parent != null;
		}
	}

	public PropertyEnumerator Parent
	{
		get
		{
			if (_restrictToThisLinkedListAndUnder == Node.Owner || _restrictToThisNodeAndUnder == Node)
			{
				return (PropertyEnumerator)RightBound.Clone();
			}
			if (Node.Owner.Parent == null)
			{
				return (PropertyEnumerator)RightBound.Clone();
			}
			return new PropertyDeepEnumerator(Node.Owner.Parent);
		}
	}

	public int Depth => Node.Owner.Depth;

	public bool RestrictedToThisLevelAndUnder
	{
		get
		{
			return _restrictToThisLinkedListAndUnder != null;
		}
		set
		{
			_restrictToThisLinkedListAndUnder = (value ? Node.Owner : null);
			_restrictToThisNodeAndUnder = null;
			CalcLeftAndRightBounds();
		}
	}

	public bool RestrictedToThisPropertyAndUnder
	{
		get
		{
			return _restrictToThisNodeAndUnder != null;
		}
		set
		{
			_restrictToThisLinkedListAndUnder = null;
			_restrictToThisNodeAndUnder = (value ? Node : null);
			CalcLeftAndRightBounds();
		}
	}

	public PropertyEnumerator LeftBound => mLeftBound;

	internal PropertyEnumerator LeftBoundInternal
	{
		set
		{
			mLeftBound = value;
		}
	}

	public PropertyEnumerator RightBound => mRightBound;

	internal PropertyEnumerator RightBoundInternal
	{
		set
		{
			mRightBound = value;
		}
	}

	public PropertyEnumerator Children
	{
		get
		{
			_ = Property;
			if (Node.Children.First == null)
			{
				return new PropertySiblingEnumerator(Node.Children.RightBound);
			}
			return new PropertySiblingEnumerator(Node.Children.First);
		}
	}

	protected PropertyEnumerator()
	{
	}

	internal PropertyEnumerator(LinkedListNode node)
	{
		_node = node;
	}

	public bool IsDescendantOf(PropertyEnumerator enumAncestor)
	{
		if (this == enumAncestor)
		{
			return false;
		}
		PropertyDeepEnumerator deepEnumerator = GetDeepEnumerator();
		while (deepEnumerator.HasParent && enumAncestor != deepEnumerator)
		{
			deepEnumerator.MoveParent();
		}
		return enumAncestor == deepEnumerator;
	}

	public abstract PropertyEnumerator MoveParent();

	public abstract PropertyEnumerator MoveNext();

	public abstract PropertyEnumerator MovePrev();

	public abstract PropertyEnumerator MoveFirst();

	public abstract PropertyEnumerator MoveLast();

	public PropertyDeepEnumerator GetDeepEnumerator()
	{
		PropertyDeepEnumerator propertyDeepEnumerator = new PropertyDeepEnumerator(Node);
		propertyDeepEnumerator.CategorizedPropertyEnumerator = CategorizedPropertyEnumerator;
		if (RestrictedToThisLevelAndUnder)
		{
			propertyDeepEnumerator.RestrictedToThisLevelAndUnder = true;
		}
		else if (RestrictedToThisPropertyAndUnder)
		{
			propertyDeepEnumerator.RestrictedToThisPropertyAndUnder = true;
		}
		return propertyDeepEnumerator;
	}

	public PropertyVisibleDeepEnumerator GetVisibleDeepEnumerator()
	{
		PropertyVisibleDeepEnumerator propertyVisibleDeepEnumerator = new PropertyVisibleDeepEnumerator(Node);
		propertyVisibleDeepEnumerator.CategorizedPropertyEnumerator = CategorizedPropertyEnumerator;
		if (RestrictedToThisLevelAndUnder)
		{
			propertyVisibleDeepEnumerator.RestrictedToThisLevelAndUnder = true;
		}
		else if (RestrictedToThisPropertyAndUnder)
		{
			propertyVisibleDeepEnumerator.RestrictedToThisPropertyAndUnder = true;
		}
		return propertyVisibleDeepEnumerator;
	}

	public PropertySiblingEnumerator GetSiblingEnumerator()
	{
		PropertySiblingEnumerator propertySiblingEnumerator = new PropertySiblingEnumerator(Node);
		propertySiblingEnumerator.CategorizedPropertyEnumerator = CategorizedPropertyEnumerator;
		if (RestrictedToThisLevelAndUnder)
		{
			propertySiblingEnumerator.RestrictedToThisLevelAndUnder = true;
		}
		else if (RestrictedToThisPropertyAndUnder)
		{
			propertySiblingEnumerator.RestrictedToThisPropertyAndUnder = true;
		}
		return propertySiblingEnumerator;
	}

	public PropertyVisibleSiblingEnumerator GetVisibleSiblingEnumerator()
	{
		PropertyVisibleSiblingEnumerator propertyVisibleSiblingEnumerator = new PropertyVisibleSiblingEnumerator(Node);
		propertyVisibleSiblingEnumerator.CategorizedPropertyEnumerator = CategorizedPropertyEnumerator;
		if (RestrictedToThisLevelAndUnder)
		{
			propertyVisibleSiblingEnumerator.RestrictedToThisLevelAndUnder = true;
		}
		else if (RestrictedToThisPropertyAndUnder)
		{
			propertyVisibleSiblingEnumerator.RestrictedToThisPropertyAndUnder = true;
		}
		return propertyVisibleSiblingEnumerator;
	}

	public PropertyEnumerator MoveToRightBound()
	{
		Node = mRightBound.Node;
		return this;
	}

	public PropertyEnumerator MoveToLeftBound()
	{
		Node = mLeftBound.Node;
		return this;
	}

	public override bool Equals(object obj)
	{
		return this == (PropertyEnumerator)obj;
	}

	public static bool operator ==(PropertyEnumerator propEnum1, PropertyEnumerator propEnum2)
	{
		if ((!object.ReferenceEquals(null, propEnum1) && object.ReferenceEquals(null, propEnum2)) || (!object.ReferenceEquals(null, propEnum2) && object.ReferenceEquals(null, propEnum1)))
		{
			return false;
		}
		if (object.ReferenceEquals(propEnum1, propEnum2))
		{
			return true;
		}
		return propEnum1.Node == propEnum2.Node;
	}

	public static bool operator !=(PropertyEnumerator propEnum1, PropertyEnumerator propEnum2)
	{
		return !(propEnum1 == propEnum2);
	}

	public static bool operator <(PropertyEnumerator propEnum1, PropertyEnumerator propEnum2)
	{
		PropertyDeepEnumerator propertyDeepEnumerator = new PropertyDeepEnumerator(propEnum1.Node);
		while (propertyDeepEnumerator.MoveNext() != propertyDeepEnumerator.RightBound)
		{
			if (propertyDeepEnumerator == propEnum2)
			{
				return true;
			}
		}
		return false;
	}

	public static bool operator >(PropertyEnumerator propEnum1, PropertyEnumerator propEnum2)
	{
		return !(propEnum1 < propEnum2);
	}

	public override int GetHashCode()
	{
		return Node.GetHashCode();
	}

	protected abstract void CalcLeftAndRightBounds();

	public virtual object Clone()
	{
		return null;
	}
}
