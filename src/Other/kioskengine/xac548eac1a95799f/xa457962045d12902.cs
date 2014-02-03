using System;
using System.Collections;
using System.ComponentModel;
using Skybound.ComponentModel;

namespace xac548eac1a95799f;

internal class xa457962045d12902
{
	private class x021e092f71971636 : PropertyDescriptor
	{
		private PropertyDescriptor x1a1659f559841abf;

		public override Type ComponentType => x1a1659f559841abf.ComponentType;

		public override bool IsReadOnly => x1a1659f559841abf.IsReadOnly;

		public override Type PropertyType => x1a1659f559841abf.PropertyType;

		public x021e092f71971636(PropertyDescriptor original)
			: base(original)
		{
			x1a1659f559841abf = original;
		}

		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override object GetValue(object component)
		{
			return x1a1659f559841abf.GetValue(component);
		}

		public override void SetValue(object component, object value)
		{
			x1a1659f559841abf.SetValue(component, value);
		}

		public override void ResetValue(object component)
		{
			(component as x6e74390626fc219b).GetPropertyList().xf6304824c08eb0da(x1a1659f559841abf.Name);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return (component as x6e74390626fc219b).GetPropertyList().x444607252416edae(x1a1659f559841abf.Name);
		}
	}

	private Hashtable x4e35c79189b28e26;

	public event PropertyChangedEventHandler x236e2b71e0b477c3;

	public event PropertyChangedEventHandler x0ad6cb77c00e4e89;

	public void x4917d7d0d6208a97(string xc3513c7f2bbafa84)
	{
		if (x236e2b71e0b477c3 != null)
		{
			x236e2b71e0b477c3(this, new PropertyChangedEventArgs(xc3513c7f2bbafa84));
		}
	}

	public void x255a9125fad2d9bb(string xc3513c7f2bbafa84)
	{
		if (x0ad6cb77c00e4e89 != null)
		{
			x0ad6cb77c00e4e89(this, new PropertyChangedEventArgs(xc3513c7f2bbafa84));
		}
	}

	public object x3f88a25febd23896(string xc3513c7f2bbafa84, object xc6e85c82d0d89508)
	{
		if (x4e35c79189b28e26 == null)
		{
			return xc6e85c82d0d89508;
		}
		if (!x4e35c79189b28e26.Contains(xc3513c7f2bbafa84))
		{
			return xc6e85c82d0d89508;
		}
		return x4e35c79189b28e26[xc3513c7f2bbafa84];
	}

	public void xc7b6e59bfbbe9301(string xc3513c7f2bbafa84, object xbcea506a33cf9111, object xd802c08964af02d1)
	{
		if (object.Equals(xbcea506a33cf9111, xd802c08964af02d1))
		{
			xf6304824c08eb0da(xc3513c7f2bbafa84);
			return;
		}
		if (x4e35c79189b28e26 == null)
		{
			x4e35c79189b28e26 = new Hashtable();
		}
		x4917d7d0d6208a97(xc3513c7f2bbafa84);
		x4e35c79189b28e26[xc3513c7f2bbafa84] = xbcea506a33cf9111;
		x255a9125fad2d9bb(xc3513c7f2bbafa84);
	}

	public bool x444607252416edae(string xc3513c7f2bbafa84)
	{
		if (x4e35c79189b28e26 != null)
		{
			return x4e35c79189b28e26.Contains(xc3513c7f2bbafa84);
		}
		return false;
	}

	public bool xe5a18e5ab4fed2d8()
	{
		if (x4e35c79189b28e26 != null)
		{
			return x4e35c79189b28e26.Count > 0;
		}
		return false;
	}

	public void xf6304824c08eb0da(string xc3513c7f2bbafa84)
	{
		if (x4e35c79189b28e26 != null && x4e35c79189b28e26.Contains(xc3513c7f2bbafa84))
		{
			x4917d7d0d6208a97(xc3513c7f2bbafa84);
			x4e35c79189b28e26.Remove(xc3513c7f2bbafa84);
			x255a9125fad2d9bb(xc3513c7f2bbafa84);
		}
	}

	public void x74f5a1ef3906e23c()
	{
		if (x4e35c79189b28e26 != null)
		{
			string[] array = new string[checked((uint)x4e35c79189b28e26.Count)];
			x4e35c79189b28e26.Keys.CopyTo(array, 0);
			string[] array2 = array;
			foreach (string xc3513c7f2bbafa in array2)
			{
				xf6304824c08eb0da(xc3513c7f2bbafa);
			}
			x4e35c79189b28e26 = null;
		}
	}

	public static PropertyDescriptorCollection xfc2d0dcac70a78ed(object xd3311d815ca25f02, Attribute[] x233f092c536593eb)
	{
		ArrayList arrayList = new ArrayList();
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(xd3311d815ca25f02, x233f092c536593eb, noCustomTypeDesc: true))
		{
			if (property.Attributes.Contains(PropertyListValueAttribute.Value))
			{
				arrayList.Add(new x021e092f71971636(property));
			}
			else
			{
				arrayList.Add(property);
			}
		}
		return new PropertyDescriptorCollection((PropertyDescriptor[])arrayList.ToArray(typeof(PropertyDescriptor)));
	}
}
