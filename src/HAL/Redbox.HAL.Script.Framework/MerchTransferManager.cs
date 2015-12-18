using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class MerchTransferManager : IDisposable
{
	internal readonly ExecutionContext Context;

	private bool Disposed;

	private int m_successfulTransfers;

	private int m_transferFailures;

	private readonly bool Debug;

	private readonly List<TransferResult> Transfers = new List<TransferResult>();

	internal int SuccessfulTransfers => m_successfulTransfers;

	internal int TransferFailures => m_transferFailures;

	internal MerchandizeResult LastTransfer { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Transfers.Clear();
		}
	}

	internal void ProcessTransfer(Location source, Location dest, string matrix)
	{
		Sum(source, dest, matrix);
	}

	internal void Process(TransferResult xferResult)
	{
		if (xferResult.Transferred)
		{
			Transfers.Add(xferResult);
			Sum(xferResult);
		}
		else if (xferResult.ReturnedToSource)
		{
			LogHelper.Instance.Log("The transfer failed to put the disk in a target, but it was returned to source.");
			m_transferFailures++;
		}
		else if (!string.IsNullOrEmpty(xferResult.ErrorCode))
		{
			LogHelper.Instance.Log("The transfer result had a hardware error: {0}", xferResult.ErrorCode.ToUpper());
			m_transferFailures++;
		}
	}

	internal MerchTransferManager(ExecutionContext context, bool debug)
	{
		LastTransfer = MerchandizeResult.Success;
		Debug = debug;
		Context = context;
	}

	private void Sum(TransferResult xferResult)
	{
		if (xferResult.Transferred)
		{
			Sum(xferResult.TransferredItem.Source, xferResult.TransferredItem.Destination, xferResult.TransferredItem.Matrix);
		}
	}

	private void Sum(Location source, Location dest, string matrix)
	{
		m_successfulTransfers++;
		Context.CreateResult("DiskTransferSuccessful", SegmentManager.Instance.InCompressedZone(source) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", source.Deck, source.Slot, "EMPTY", null, matrix);
		if (DumpBin.Instance.IsBin(dest))
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, matrix, null, null);
		}
		else
		{
			Context.CreateResult("DiskTransferSuccessful", SegmentManager.Instance.InCompressedZone(dest) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", dest.Deck, dest.Slot, matrix, null, "EMPTY");
		}
	}
}
