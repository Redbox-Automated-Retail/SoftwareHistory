using System.Collections;

namespace Belikov.GenuineChannels.Utilities;

internal class CollectionUtility
{
	private CollectionUtility()
	{
	}

	public static object ObtainObjectFromArrayListByKey(Hashtable hashtable, object key)
	{
		lock (hashtable.SyncRoot)
		{
			if (!(hashtable[key] is ArrayList arrayList))
			{
				return null;
			}
			lock (arrayList)
			{
				if (arrayList.Count <= 0)
				{
					return null;
				}
				object result = arrayList[arrayList.Count - 1];
				arrayList.RemoveAt(arrayList.Count - 1);
				return result;
			}
		}
	}

	public static void InsertObjectToArrayListByKey(Hashtable hashtable, object key, object obj)
	{
		lock (hashtable.SyncRoot)
		{
			ArrayList arrayList = hashtable[key] as ArrayList;
			if (arrayList == null)
			{
				arrayList = (ArrayList)(hashtable[key] = new ArrayList());
			}
			lock (arrayList)
			{
				arrayList.Add(obj);
			}
		}
	}
}
