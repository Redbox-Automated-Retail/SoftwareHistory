using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public class VMZPhase1Converter
{
	private static string m_database;

	public static string LegacyDatabase
	{
		get
		{
			if (string.IsNullOrEmpty(m_database))
			{
				m_database = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data\\HALVMZ.vdb3");
			}
			return m_database;
		}
	}

	private bool IsEnabled => SecretOptions.Instance.GetValue("EnableVMZ", defValue: false);

	public void UpgradeToNewPhase(TextWriter log)
	{
		if (!IsEnabled)
		{
			log.Write("The VMZ is not configured - no upgrade.");
		}
		else
		{
			ConvertLegacyData(log);
		}
	}

	private void ConvertLegacyData(TextWriter log)
	{
		if (File.Exists(LegacyDatabase))
		{
			VistaDBHelper vistaDBHelper = new VistaDBHelper(LegacyDatabase, closeOnDispose: true);
			string query = "SELECT Deck, Slot, InventoryFlags FROM [VMZ]";
			vistaDBHelper.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				int deck = reader.ReadInt(0);
				int slot = reader.ReadInt(1);
				MerchFlags flags = (MerchFlags)reader.ReadInt64(2);
				Deck deck2 = DecksManager.Instance.GetDeck(deck);
				deck2.ExcludeSlot(slot, excluded: false);
				deck2.UpdateMerchFlag(flags, slot);
			}, query);
		}
	}
}
