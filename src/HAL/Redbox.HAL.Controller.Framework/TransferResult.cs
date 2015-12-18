using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class TransferResult
{
	public bool ReturnedToSource { get; internal set; }

	public bool GetOk
	{
		get
		{
			if (GetResult != null)
			{
				return GetResult.Success;
			}
			return false;
		}
	}

	public bool SourceStuck
	{
		get
		{
			if (GetResult != null)
			{
				return GetResult.ItemStuck;
			}
			return false;
		}
	}

	public bool SourceEmpty
	{
		get
		{
			if (GetResult != null)
			{
				return GetResult.IsSlotEmpty;
			}
			return false;
		}
	}

	public bool SlotInUse
	{
		get
		{
			if (PutResult != null)
			{
				return PutResult.IsSlotInUse;
			}
			return false;
		}
	}

	public bool HardwareError
	{
		get
		{
			if (FailedMoveResult.HasValue)
			{
				return true;
			}
			if (GetResult != null && !GetResult.Success)
			{
				return true;
			}
			if (PutResult != null && !PutResult.Success)
			{
				return true;
			}
			return false;
		}
	}

	public string ErrorCode
	{
		get
		{
			if (FailedMoveResult.HasValue && FailedMoveResult.Value != MotionResult.Success)
			{
				return FailedMoveResult.Value.ToString();
			}
			if (GetResult != null && !GetResult.Success && !GetResult.EmptyOrStuck)
			{
				return GetResult.ToString();
			}
			if (PutResult != null && !PutResult.Success && !PutResult.IsSlotInUse)
			{
				return PutResult.ToString();
			}
			return null;
		}
	}

	public bool Transferred => TransferredItem != null;

	public TransferredItem TransferredItem { get; private set; }

	public MotionResult? FailedMoveResult { get; internal set; }

	public string FailedError
	{
		get
		{
			if (FailedMoveResult.HasValue)
			{
				return FailedMoveResult.Value.ToString();
			}
			if (!GetResult.Success)
			{
				return GetResult.ToString();
			}
			return PutResult.ToString();
		}
	}

	public GetResult GetResult { get; internal set; }

	public PutResult PutResult { get; internal set; }

	public static TransferResult EmulatedSuccessResult(Location source, Location dest)
	{
		TransferResult transferResult = new TransferResult();
		transferResult.TransferredItem = new TransferredItem
		{
			Source = source,
			Destination = dest
		};
		Deck deck = DecksManager.Instance.GetDeck(dest.Deck);
		deck.SetSlotId(dest.Slot, source.Matrix, source.Flags, null);
		deck.UpdateMerchFlag(source.Flags, dest.Slot);
		deck = DecksManager.Instance.GetDeck(source.Deck);
		deck.ResetLocation(source.Slot);
		return transferResult;
	}

	internal void TransferDone(Location src, Location dest)
	{
		if (TransferredItem != null)
		{
			LogHelper.Instance.Log("Invalid state: the transferred item is already set.");
			return;
		}
		if (src.Equals(dest))
		{
			LogHelper.Instance.Log("TransferDone: the item was not transferred to a new destination.");
			return;
		}
		TransferredItem = new TransferredItem
		{
			Source = src,
			Destination = dest,
			Matrix = GetResult.Previous
		};
	}
}
