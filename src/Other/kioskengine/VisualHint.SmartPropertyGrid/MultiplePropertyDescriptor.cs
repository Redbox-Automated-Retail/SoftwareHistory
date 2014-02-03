using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VisualHint.SmartPropertyGrid;

internal class MultiplePropertyDescriptor : PropertyDescriptor
{
	private class MultipleAttributeCollection : AttributeCollection
	{
		private AttributeCollection[] _attributeCollections;

		private IDictionary _cachedAttributes;

		private MultiplePropertyDescriptor _owner;

		public override Attribute this[Type attributeType] => GetCommonAttribute(attributeType);

		public override Attribute this[int index]
		{
			get
			{
				if (index < _owner.descriptors[0].Attributes.Count)
				{
					Attribute attribute = _owner.descriptors[0].Attributes[index];
					return this[attribute.GetType()];
				}
				return null;
			}
		}

		public MultipleAttributeCollection(MultiplePropertyDescriptor owner)
		{
			_owner = owner;
		}

		private Attribute GetCommonAttribute(Type attributeType)
		{
			if (_attributeCollections == null)
			{
				_attributeCollections = new AttributeCollection[_owner.descriptors.Length];
				for (int i = 0; i < _owner.descriptors.Length; i++)
				{
					_attributeCollections[i] = _owner.descriptors[i].Attributes;
				}
			}
			if (_attributeCollections.Length == 0)
			{
				return GetDefaultAttribute(attributeType);
			}
			if (_cachedAttributes != null && _cachedAttributes[attributeType] is Attribute result)
			{
				return result;
			}
			Attribute attribute = _attributeCollections[0][attributeType];
			if (attribute == null)
			{
				return null;
			}
			for (int j = 1; j < _attributeCollections.Length; j++)
			{
				Attribute obj = _attributeCollections[j][attributeType];
				if (!attribute.Equals(obj))
				{
					attribute = GetDefaultAttribute(attributeType);
					break;
				}
			}
			if (_cachedAttributes == null)
			{
				_cachedAttributes = new Hashtable();
			}
			_cachedAttributes[attributeType] = attribute;
			return attribute;
		}
	}

	private class MultiMergeCollection : ICollection, IEnumerable
	{
		private object[] items;

		private bool locked;

		public int Count
		{
			get
			{
				if (items != null)
				{
					return items.Length;
				}
				return 0;
			}
		}

		public bool Locked
		{
			get
			{
				return locked;
			}
			set
			{
				locked = value;
			}
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		public MultiMergeCollection(ICollection original)
		{
			SetItems(original);
		}

		public void CopyTo(Array array, int index)
		{
			if (items != null)
			{
				Array.Copy(items, 0, array, index, items.Length);
			}
		}

		public IEnumerator GetEnumerator()
		{
			if (items != null)
			{
				return items.GetEnumerator();
			}
			return new object[0].GetEnumerator();
		}

		public bool MergeCollection(ICollection newCollection)
		{
			if (!locked)
			{
				if (items.Length != newCollection.Count)
				{
					items = new object[0];
					return false;
				}
				object[] array = new object[newCollection.Count];
				newCollection.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == null != (items[i] == null) || (items[i] != null && !items[i].Equals(array[i])))
					{
						items = new object[0];
						return false;
					}
				}
			}
			return true;
		}

		public void SetItems(ICollection collection)
		{
			if (!locked)
			{
				items = new object[collection.Count];
				collection.CopyTo(items, 0);
			}
		}
	}

	private enum TriState
	{
		Unknown,
		Yes,
		No
	}

	private TriState canReset;

	private MultiMergeCollection collection;

	private PropertyDescriptor[] descriptors;

	private TriState localizable;

	private TriState readOnly;

	internal PropertyDescriptor[] PropertyDescriptors => descriptors;

	public override Type ComponentType => descriptors[0].ComponentType;

	public override TypeConverter Converter => descriptors[0].Converter;

	public override string DisplayName => descriptors[0].DisplayName;

	public override bool IsLocalizable
	{
		get
		{
			if (localizable == TriState.Unknown)
			{
				localizable = TriState.Yes;
				PropertyDescriptor[] array = descriptors;
				foreach (PropertyDescriptor propertyDescriptor in array)
				{
					if (!propertyDescriptor.IsLocalizable)
					{
						localizable = TriState.No;
						break;
					}
				}
			}
			return localizable == TriState.Yes;
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			if (readOnly == TriState.Unknown)
			{
				readOnly = TriState.No;
				PropertyDescriptor[] array = descriptors;
				foreach (PropertyDescriptor propertyDescriptor in array)
				{
					if (propertyDescriptor.IsReadOnly)
					{
						readOnly = TriState.Yes;
						break;
					}
				}
			}
			return readOnly == TriState.Yes;
		}
	}

	public PropertyDescriptor this[int index] => descriptors[index];

	public override Type PropertyType => descriptors[0].PropertyType;

	public MultiplePropertyDescriptor(PropertyDescriptor[] descriptors)
		: base(descriptors[0].Name, null)
	{
		if (descriptors[0] is MultiplePropertyDescriptor)
		{
			this.descriptors = descriptors;
		}
		this.descriptors = descriptors;
	}

	public override bool CanResetValue(object component)
	{
		if (canReset == TriState.Unknown)
		{
			canReset = TriState.Yes;
			Array a = (Array)component;
			for (int i = 0; i < descriptors.Length; i++)
			{
				if (!descriptors[i].CanResetValue(GetPropertyOwnerForComponent(a, i)))
				{
					canReset = TriState.No;
					break;
				}
			}
		}
		return canReset == TriState.Yes;
	}

	private object CopyValue(object value)
	{
		if (value != null)
		{
			Type type = value.GetType();
			if (type.IsValueType)
			{
				return value;
			}
			object obj = null;
			if (value is ICloneable cloneable)
			{
				obj = cloneable.Clone();
			}
			if (obj == null)
			{
				TypeConverter typeConverter = TypeDescriptor.GetConverter(value);
				if (typeConverter.CanConvertTo(typeof(InstanceDescriptor)))
				{
					InstanceDescriptor instanceDescriptor = (InstanceDescriptor)typeConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(InstanceDescriptor));
					if (instanceDescriptor != null && instanceDescriptor.IsComplete)
					{
						obj = instanceDescriptor.Invoke();
					}
				}
				if (obj == null && typeConverter.CanConvertTo(typeof(string)) && typeConverter.CanConvertFrom(typeof(string)))
				{
					object obj2 = typeConverter.ConvertToInvariantString(value);
					obj = typeConverter.ConvertFromInvariantString((string)obj2);
				}
			}
			if (obj == null && type.IsSerializable)
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream();
				binaryFormatter.Serialize(memoryStream, value);
				memoryStream.Position = 0L;
				obj = binaryFormatter.Deserialize(memoryStream);
			}
			if (obj != null)
			{
				return obj;
			}
		}
		return value;
	}

	protected override AttributeCollection CreateAttributeCollection()
	{
		ArrayList arrayList = new ArrayList();
		Attribute[] array = new Attribute[descriptors[0].Attributes.Count];
		descriptors[0].Attributes.CopyTo(array, 0);
		AttributeCollection[] array2 = new AttributeCollection[descriptors.Length - 1];
		for (int i = 1; i < descriptors.Length; i++)
		{
			array2[i - 1] = descriptors[i].Attributes;
		}
		foreach (Attribute attribute in array)
		{
			bool flag = true;
			AttributeCollection[] array3 = array2;
			foreach (AttributeCollection attributeCollection in array3)
			{
				Attribute attribute2 = attributeCollection[attribute.GetType()];
				if (attribute2 == null)
				{
					flag = false;
					break;
				}
				if (!attribute.Equals(attribute2))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				arrayList.Add(attribute);
			}
		}
		return new AttributeCollection((Attribute[])arrayList.ToArray(typeof(Attribute)));
	}

	public override object GetEditor(Type editorBaseType)
	{
		return descriptors[0].GetEditor(editorBaseType);
	}

	private object GetPropertyOwnerForComponent(Array a, int i)
	{
		object obj = a.GetValue(i);
		if (obj is ICustomTypeDescriptor)
		{
			obj = ((ICustomTypeDescriptor)obj).GetPropertyOwner(descriptors[i]);
		}
		return obj;
	}

	public override object GetValue(object component)
	{
		object value = GetValue((Array)component, out var _);
		if (value is MultiMergeCollection)
		{
			MultiMergeCollection multiMergeCollection = value as MultiMergeCollection;
			object[] array = new object[multiMergeCollection.Count];
			multiMergeCollection.CopyTo(array, 0);
			return array;
		}
		return value;
	}

	public object GetValue(Array components, out bool allEqual)
	{
		allEqual = true;
		object value = descriptors[0].GetValue(GetPropertyOwnerForComponent(components, 0));
		if (value is ICollection)
		{
			if (collection == null)
			{
				collection = new MultiMergeCollection((ICollection)value);
			}
			else
			{
				if (collection.Locked)
				{
					return collection;
				}
				collection.SetItems((ICollection)value);
			}
		}
		for (int i = 1; i < descriptors.Length; i++)
		{
			object value2 = descriptors[i].GetValue(GetPropertyOwnerForComponent(components, i));
			if (collection != null)
			{
				if (!collection.MergeCollection((ICollection)value2))
				{
					allEqual = false;
					return null;
				}
			}
			else if ((value != null || value2 != null) && (value == null || !value.Equals(value2)))
			{
				allEqual = false;
				return null;
			}
		}
		if (collection != null)
		{
			if (allEqual)
			{
				return value;
			}
			return null;
		}
		return value;
	}

	internal object[] GetValues(Array components)
	{
		object[] array = new object[components.Length];
		for (int i = 0; i < components.Length; i++)
		{
			array[i] = descriptors[i].GetValue(GetPropertyOwnerForComponent(components, i));
		}
		return array;
	}

	internal object GetValue(Array components, int index)
	{
		if (index >= components.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return descriptors[index].GetValue(GetPropertyOwnerForComponent(components, index));
	}

	public override void ResetValue(object component)
	{
		Array a = (Array)component;
		for (int i = 0; i < descriptors.Length; i++)
		{
			descriptors[i].ResetValue(GetPropertyOwnerForComponent(a, i));
		}
	}

	private void SetCollectionValues(Array a, IList listValue)
	{
		try
		{
			if (collection != null)
			{
				collection.Locked = true;
			}
			object[] array = new object[listValue.Count];
			listValue.CopyTo(array, 0);
			for (int i = 0; i < descriptors.Length; i++)
			{
				if (descriptors[i].GetValue(GetPropertyOwnerForComponent(a, i)) is IList list)
				{
					list.Clear();
					object[] array2 = array;
					foreach (object value in array2)
					{
						list.Add(value);
					}
				}
			}
		}
		finally
		{
			if (collection != null)
			{
				collection.Locked = false;
			}
		}
	}

	public override void SetValue(object component, object value)
	{
		Array a = (Array)component;
		if (!(value is Array) && value is IList && typeof(IList).IsAssignableFrom(PropertyType))
		{
			SetCollectionValues(a, (IList)value);
			return;
		}
		for (int i = 0; i < descriptors.Length; i++)
		{
			descriptors[i].SetValue(GetPropertyOwnerForComponent(a, i), value);
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		Array a = (Array)component;
		for (int i = 0; i < descriptors.Length; i++)
		{
			if (!descriptors[i].ShouldSerializeValue(GetPropertyOwnerForComponent(a, i)))
			{
				return false;
			}
		}
		return true;
	}
}
