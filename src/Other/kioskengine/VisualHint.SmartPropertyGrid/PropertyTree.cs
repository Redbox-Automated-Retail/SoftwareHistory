using System.Collections;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyTree
{
	private LinkedList _PropertyTree;

	public PropertyTree(int baseDepth)
	{
		_PropertyTree = new LinkedList(null, baseDepth);
	}

	public PropertyDeepEnumerator GetDeepEnumerator()
	{
		return new PropertyDeepEnumerator(_PropertyTree.LeftBound);
	}

	public PropertyVisibleDeepEnumerator GetVisibleDeepEnumerator()
	{
		return new PropertyVisibleDeepEnumerator(_PropertyTree.LeftBound);
	}

	public PropertySiblingEnumerator GetSiblingEnumerator()
	{
		return new PropertySiblingEnumerator(_PropertyTree.LeftBound);
	}

	public PropertyVisibleSiblingEnumerator GetVisibleSiblingEnumerator()
	{
		return new PropertyVisibleSiblingEnumerator(_PropertyTree.LeftBound);
	}

	public PropertyEnumerator Find(int propertyId)
	{
		PropertyDeepEnumerator deepEnumerator = GetDeepEnumerator();
		return FindAfter(deepEnumerator, propertyId);
	}

	public PropertyEnumerator FindAfter(PropertyEnumerator afterEnumerator, int propertyId)
	{
		PropertyEnumerator propertyEnumerator = new PropertyDeepEnumerator(afterEnumerator.Node);
		while (propertyEnumerator.MoveNext() != propertyEnumerator.RightBound && propertyEnumerator.Property.Id != propertyId)
		{
		}
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertBefore(PropertyEnumerator beforeEnumerator, Property property)
	{
		return new PropertyDeepEnumerator(beforeEnumerator.Node.Owner.InsertBefore(beforeEnumerator.Node, property));
	}

	public PropertyEnumerator Append(PropertyEnumerator atEnumerator, Property property)
	{
		PropertySiblingEnumerator propertySiblingEnumerator = new PropertySiblingEnumerator(atEnumerator.Node);
		return InsertBefore(propertySiblingEnumerator.MoveToRightBound(), property);
	}

	public PropertyEnumerator Delete(PropertyEnumerator propEnum)
	{
		PropertyDeepEnumerator propertyDeepEnumerator = new PropertyDeepEnumerator(propEnum.Node);
		propertyDeepEnumerator.MovePrev();
		propEnum.Node.Owner.Remove(propEnum.Node);
		if (propertyDeepEnumerator.MoveNext() == propertyDeepEnumerator.RightBound)
		{
			propertyDeepEnumerator.MovePrev();
		}
		if (propertyDeepEnumerator == propertyDeepEnumerator.LeftBound)
		{
			propertyDeepEnumerator = propertyDeepEnumerator.RightBound.GetDeepEnumerator();
		}
		propEnum.Node = propertyDeepEnumerator.Node;
		return propertyDeepEnumerator;
	}

	public override string ToString()
	{
		string text = "Deep++\n";
		PropertyDeepEnumerator deepEnumerator = GetDeepEnumerator();
		while (deepEnumerator.MoveNext() != deepEnumerator.RightBound)
		{
			for (int i = 0; i < deepEnumerator.Depth; i++)
			{
				text += "-";
			}
			text = text + deepEnumerator.Property.ToString() + "\n";
		}
		text += "\n\nDeep--\n";
		deepEnumerator.MoveToRightBound();
		while (deepEnumerator.MovePrev() != deepEnumerator.LeftBound)
		{
			for (int j = 0; j < deepEnumerator.Depth; j++)
			{
				text += "-";
			}
			text = text + deepEnumerator.Property.ToString() + "\n";
		}
		return text;
	}

	public void Clear()
	{
		_PropertyTree.Clear();
	}

	public void Sort(IComparer comparer)
	{
		_PropertyTree.Sort(comparer);
	}
}
