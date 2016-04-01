using System.Collections.Generic;
using System.IO;

namespace Redbox.HAL.Component.Model;

internal interface IInventoryService
{
	int DumpbinItemCount { get; }

	bool IsTrackingEmptyStuck { get; }

	bool MachineIsFull();

	bool EmptyCountExceeds(int numberOfEmpty);

	ILocation Lookup(string id);

	ILocation Get(int deck, int slot);

	bool Reset(ILocation location);

	bool Reset(IList<ILocation> locations);

	bool Save(ILocation location);

	bool Save(IEnumerable<ILocation> locations);

	int GetExcludedSlotsCount();

	IList<ILocation> ComputeExcludedSlots();

	IList<ILocation> GetEmptySlots();

	IList<ILocation> GetUnknowns();

	bool IsBarcodeDuplicate(string id, out ILocation original);

	int GetMachineEmptyCount();

	bool UpdateEmptyStuck(int deck, int slot);

	bool UpdateEmptyStuck(ILocation location);

	bool ResetCount(ILocation location);

	bool IsStuck(ILocation loc);

	void DumpStore(TextWriter writer);

	bool MarkDeckInventory(IDeck deck, string newMatrix);

	IList<int> SwapEmptyWith(IDeck deck, string id, MerchFlags flags, IRange<int> slotRange);

	bool ResetAndMark(IDeck deck, string id);

	string GetInventoryState();

	void ResetInventoryState(string xml, ErrorList errors);

	bool ClearDumpbinInventory();

	IList<IDumpBinInventoryItem> GetBarcodesInBin();

	bool AddBinItem(string matrix);

	bool ExcludeSlots(IDeck deck, IList<int> slots);

	bool UnExcludeSlots(IDeck deck, IList<int> slots);
}
