using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

public sealed class ExecutionStatus
{
	public ExecutionContextStatus Value { get; internal set; }

	internal ExecutionContextStatus? NextValue { get; set; }

	public static bool operator ==(ExecutionStatus status, ExecutionContextStatus right)
	{
		if (status != null)
		{
			return status.Value.Equals(right);
		}
		return false;
	}

	public static bool operator !=(ExecutionStatus status, ExecutionContextStatus right)
	{
		return !(status == right);
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj is ExecutionStatus executionStatus)
		{
			return executionStatus.Value.Equals(Value);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	internal bool ShouldPromote()
	{
		if (NextValue.HasValue)
		{
			return Value != NextValue;
		}
		return false;
	}

	internal void Promote()
	{
		Promote(NextValue.Value);
	}

	internal void Promote(ExecutionContextStatus value)
	{
		Value = value;
		NextValue = null;
	}
}
