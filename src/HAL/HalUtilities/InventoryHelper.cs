using System;
using System.IO;
using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class InventoryHelper : IDisposable
{
	private const string DefaultProtocol = "rcp://localhost:7001";

	private bool Disposed;

	private readonly HardwareService Service;

	private readonly ClientHelper m_helper;

	public void Dispose()
	{
		Dispose(fromDispose: true);
	}

	internal void DumpInventoryToFile(string path)
	{
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Errors.ContainsError())
		{
			File.WriteAllText(path, inventoryState.CommandMessages[0]);
		}
	}

	internal bool ImportInventoryFromFile(string fileName)
	{
		try
		{
			string inventoryState = File.ReadAllText(fileName);
			return Service.SetInventoryState(inventoryState).Success;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return false;
		}
	}

	internal bool FixType(string fileName, string type, string label)
	{
		if (type == "UNKNOWN")
		{
			return Service.ServiceUnknownSync().Success;
		}
		if (type == "EMPTY")
		{
			return Service.ServiceEmptySync().Success;
		}
		return false;
	}

	internal bool ScheduleFullSync()
	{
		return Service.FullSync().Success;
	}

	internal InventoryHelper()
	{
		m_helper = new ClientHelper();
		Service = m_helper.AutoConnect();
	}

	private void Dispose(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}
}
