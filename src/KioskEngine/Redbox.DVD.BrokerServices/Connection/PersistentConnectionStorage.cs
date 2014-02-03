using System;
using System.Collections;

namespace Belikov.GenuineChannels.Connection;

internal class PersistentConnectionStorage
{
	private class ObjectStorageWithDefaultEntry
	{
		public Hashtable Collection = new Hashtable();

		public object DefaultObject;
	}

	public delegate void ProcessConnectionEventHandler(object connection, object parameter);

	private Hashtable _byUri = new Hashtable();

	public object SyncRoot => this;

	public void Set(string primaryUri, string name, object connection)
	{
		lock (SyncRoot)
		{
			ObjectStorageWithDefaultEntry objectStorageWithDefaultEntry = _byUri[primaryUri] as ObjectStorageWithDefaultEntry;
			if (objectStorageWithDefaultEntry == null)
			{
				objectStorageWithDefaultEntry = (ObjectStorageWithDefaultEntry)(_byUri[primaryUri] = new ObjectStorageWithDefaultEntry());
			}
			objectStorageWithDefaultEntry.Collection[name] = connection;
			objectStorageWithDefaultEntry.DefaultObject = connection;
		}
	}

	public void Remove(string primaryUri, string name)
	{
		lock (SyncRoot)
		{
			if (!(_byUri[primaryUri] is ObjectStorageWithDefaultEntry objectStorageWithDefaultEntry))
			{
				return;
			}
			bool flag = name == null || object.ReferenceEquals(objectStorageWithDefaultEntry.Collection[name], objectStorageWithDefaultEntry.DefaultObject);
			if (name != null)
			{
				objectStorageWithDefaultEntry.Collection.Remove(name);
			}
			if (objectStorageWithDefaultEntry.Collection.Count <= 0)
			{
				_byUri.Remove(primaryUri);
			}
			else
			{
				if (!flag)
				{
					return;
				}
				objectStorageWithDefaultEntry.DefaultObject = null;
				{
					IDictionaryEnumerator dictionaryEnumerator = objectStorageWithDefaultEntry.Collection.GetEnumerator();
					try
					{
						if (dictionaryEnumerator.MoveNext())
						{
							DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
							objectStorageWithDefaultEntry.DefaultObject = dictionaryEntry.Value;
						}
						return;
					}
					finally
					{
						IDisposable disposable = dictionaryEnumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
	}

	public object Get(string primaryUri, string name)
	{
		if (primaryUri == null)
		{
			return null;
		}
		lock (SyncRoot)
		{
			if (!(_byUri[primaryUri] is ObjectStorageWithDefaultEntry objectStorageWithDefaultEntry))
			{
				return null;
			}
			if (name == null || name.Length <= 0)
			{
				return objectStorageWithDefaultEntry.DefaultObject;
			}
			return objectStorageWithDefaultEntry.Collection[name];
		}
	}

	public string[] GetAll(string primaryUri)
	{
		lock (SyncRoot)
		{
			if (!(_byUri[primaryUri] is ObjectStorageWithDefaultEntry objectStorageWithDefaultEntry))
			{
				return null;
			}
			string[] array = new string[objectStorageWithDefaultEntry.Collection.Count];
			objectStorageWithDefaultEntry.Collection.Keys.CopyTo(array, 0);
			return array;
		}
	}

	public void InspectAllConnections(ProcessConnectionEventHandler processConnectionEventHandler, object parameter)
	{
		ArrayList arrayList = new ArrayList();
		lock (SyncRoot)
		{
			foreach (object item in _byUri)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
				ObjectStorageWithDefaultEntry objectStorageWithDefaultEntry = (ObjectStorageWithDefaultEntry)dictionaryEntry.Value;
				foreach (object item2 in objectStorageWithDefaultEntry.Collection)
				{
					DictionaryEntry dictionaryEntry2 = (DictionaryEntry)item2;
					arrayList.Add(dictionaryEntry2.Value);
				}
			}
		}
		foreach (object item3 in arrayList)
		{
			processConnectionEventHandler(item3, parameter);
		}
	}
}
