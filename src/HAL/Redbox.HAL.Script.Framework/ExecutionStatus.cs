namespace Redbox.HAL.Script.Framework;

public class ExecutionStatus
{
	public ExecutionContextStatus Value { get; internal set; }

	public ExecutionContextStatus? NextValue { get; set; }

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

	public void Promote()
	{
		Value = NextValue.Value;
		NextValue = null;
	}

	public bool ShouldPromote()
	{
		if (NextValue.HasValue)
		{
			return Value != NextValue;
		}
		return false;
	}

	public void Promote(ExecutionContextStatus value)
	{
		Value = value;
		NextValue = null;
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
}
